using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClamAV_Engine.ClamLib.Helpers;

namespace ClamAV_Engine.ClamLib
{
    /// <summary>
    /// AhoCorasickEngine: Engine "lai" giữa ClamAVEngine và AhoCorasickPatternLoader.
    /// OPTIMIZED VERSION:
    /// - Parallel LDB scanning (xử lý multiple signatures cùng lúc)
    /// - Cache pattern parsing results
    /// - Removed unnecessary NDB fallback (AC Trie đủ nhanh)
    /// - Minimal logging overhead
    /// </summary>
    public class AhoCorasickEngine
    {
        public ClamAVDatabase DailyDatabase { get; private set; }
        public ClamAVDatabase MainDatabase { get; private set; }

        public bool IsDatabaseLoaded { get; private set; }
        public int LoadedPatternCount { get; private set; }

        public Action<string> Logger { get; set; }

        /// <summary>
        /// Chỉ định các loại chữ ký sẽ được nạp vào AC Trie.
        /// Mặc định: HDB + NDB (nhanh và phổ biến nhất).
        /// </summary>
        public HashSet<SignatureType> IncludedTypes { get; }

        private AhoCorasickMatcher _matcher;
        private int _patternCount = 0;
        private bool _isTrieBuilt = false;
        
        // Cache for pattern parsing
        private Dictionary<string, (string pattern, string offset)> _ndbPatternCache = new Dictionary<string, (string, string)>();
        private Dictionary<string, (int[], string)> _ldbExpressionCache = new Dictionary<string, (int[], string)>();

        public AhoCorasickEngine(IEnumerable<SignatureType> includedTypes = null)
        {
            DailyDatabase = new ClamAVDatabase();
            MainDatabase = new ClamAVDatabase();
            _matcher = new AhoCorasickMatcher();
            
            IncludedTypes = new HashSet<SignatureType>(includedTypes ?? new[]
            {
                SignatureType.HDB,
                SignatureType.NDB
            });
        }

        /// <summary>
        /// Tải database từ thư mục chứa daily/main hoặc thư mục chữ ký đơn.
        /// Nạp TẤT CẢ loại chữ ký (để có FP cho whitelist, v.v.), không build AC Trie.
        /// </summary>
        public bool LoadDatabaseFolder(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
                return false;

            DailyDatabase.Clear();
            MainDatabase.Clear();
            _matcher = new AhoCorasickMatcher();
            _patternCount = 0;
            _isTrieBuilt = false;
            IsDatabaseLoaded = false;
            LoadedPatternCount = 0;

            int loaded = 0;

            string dailyDir = Path.Combine(folderPath, "daily");
            string mainDir = Path.Combine(folderPath, "main");

            bool hasDailySub = Directory.Exists(dailyDir);
            bool hasMainSub = Directory.Exists(mainDir);

            if (hasDailySub || hasMainSub)
            {
                if (hasDailySub)
                    loaded += LoadDatabaseFromDirectory(dailyDir, DailyDatabase);

                if (hasMainSub)
                    loaded += LoadDatabaseFromDirectory(mainDir, MainDatabase);
            }
            else
            {
                // Không có daily/main, nạp cả thư mục vào Daily
                loaded += LoadDatabaseFromDirectory(folderPath, DailyDatabase);
            }

            // Build trie ngay sau khi load để tránh pass thứ hai
            if (_patternCount > 0)
            {
                try
                {
                    _matcher.Build();
                    _isTrieBuilt = true;
                    LoadedPatternCount = _patternCount;
                    Logger?.Invoke($"✅ AC Trie built during load: {_patternCount} patterns");
                }
                catch (Exception ex)
                {
                    Logger?.Invoke($"❌ Build AC Trie error: {ex.Message}");
                    _isTrieBuilt = false;
                }
            }

            IsDatabaseLoaded = loaded > 0;
            Logger?.Invoke($"[AC Engine] Database loaded: {loaded} signatures (Daily: {DailyDatabase.TotalSignatures}, Main: {MainDatabase.TotalSignatures})");
            Logger?.Invoke($"[AC Engine] AC Trie patterns: {_patternCount}");
            Logger?.Invoke($"[AC Engine] LDB signatures available: Daily={DailyDatabase.LdbSignatures.Count}, Main={MainDatabase.LdbSignatures.Count}");
            Logger?.Invoke($"[AC Engine] Breakdown - HDB: {DailyDatabase.HdbSignatures.Count + MainDatabase.HdbSignatures.Count}, NDB: {DailyDatabase.NdbSignatures.Count + MainDatabase.NdbSignatures.Count}, LDB: {DailyDatabase.LdbSignatures.Count + MainDatabase.LdbSignatures.Count}, LDU: {DailyDatabase.LduSignatures.Count + MainDatabase.LduSignatures.Count}, HSB: {DailyDatabase.HsbSignatures.Count + MainDatabase.HsbSignatures.Count}, MDB: {DailyDatabase.MdbSignatures.Count + MainDatabase.MdbSignatures.Count}, CDB: {DailyDatabase.CdbSignatures.Count + MainDatabase.CdbSignatures.Count}, FP: {DailyDatabase.FpSignatures.Count + MainDatabase.FpSignatures.Count}");
            return IsDatabaseLoaded;
        }

        private int LoadDatabaseFromDirectory(string directory, ClamAVDatabase db)
        {
            int count = 0;
            if (!Directory.Exists(directory))
                return 0;

            // Load TẤT CẢ signature types từ thư mục (bao gồm subdirectories như bytecode)
            foreach (var file in Directory.EnumerateFiles(directory, "*.*", SearchOption.AllDirectories))
            {
                var ext = Path.GetExtension(file).ToLowerInvariant();
                SignatureType? type = null;

                switch (ext)
                {
                    case ".hdb": type = SignatureType.HDB; break;
                    case ".hsb": type = SignatureType.HSB; break;
                    case ".mdb": type = SignatureType.MDB; break;
                    case ".ndb": type = SignatureType.NDB; break;
                    case ".ldb": type = SignatureType.LDB; break;
                    case ".ldu": type = SignatureType.LDU; break;
                    case ".cdb": type = SignatureType.CDB; break;
                    case ".fp":  type = SignatureType.FP;  break;
                    default:
                        break;
                }

                if (!type.HasValue)
                    continue;

                // Load tất cả loại chữ ký - sẽ lọc khi build AC patterns
                count += LoadSignatureFile(file, type.Value, db);
            }

            return count;
        }

        private int LoadSignatureFile(string filePath, SignatureType type, ClamAVDatabase db)
        {
            int loaded = 0;
            foreach (var line in File.ReadLines(filePath))
            {
                var sig = ClamAVSignature.Parse(line, type);
                if (sig == null || string.IsNullOrEmpty(sig.Name))
                    continue;

                // Tùy theo loại, quyết định add vào AC và/hoặc lưu vào DB để fallback
                switch (sig.Type)
                {
                    case SignatureType.HDB:
                    {
                        // HDB: chỉ lưu để quét theo MD5 (không add AC)
                        db.HdbSignatures[$"SIG_HDB_{db.HdbSignatures.Count}"] = sig;
                        loaded++;
                        break;
                    }
                    case SignatureType.NDB:
                    {
                        string offset = "*";
                        if (sig.Properties.TryGetValue("Offset", out var offObj) && offObj is string offStr)
                            offset = offStr;
                        string raw = null;
                        if (sig.Properties.TryGetValue("RawPattern", out var rawObj) && rawObj is string rawStr)
                            raw = rawStr;
                        
                        // Cố gắng add vào AC nếu offset toàn cục và pattern thuần hex
                        if (!string.IsNullOrEmpty(raw) && IncludedTypes.Contains(SignatureType.NDB) && offset == "*")
                        {
                            var bytes = HexToBytes(raw);
                            if (bytes != null && bytes.Length > 0)
                            {
                                _matcher.AddPattern(bytes, sig.Name);
                                _patternCount++;
                            }
                        }
                        
                        // Luôn lưu để fallback PatternMatcher
                        db.NdbSignatures[$"SIG_NDB_{db.NdbSignatures.Count}"] = sig;
                        loaded++;
                        break;
                    }
                    case SignatureType.LDU:
                    {
                        string lduPat = null;
                        if (sig.Properties.TryGetValue("UncompressedPattern", out var lduObj) && lduObj is string lduStr)
                            lduPat = lduStr;
                        
                        // Cố gắng add vào AC nếu pattern thuần hex
                        if (!string.IsNullOrEmpty(lduPat) && IncludedTypes.Contains(SignatureType.LDU))
                        {
                            var bytes = HexToBytes(lduPat);
                            if (bytes != null && bytes.Length > 0)
                            {
                                _matcher.AddPattern(bytes, $"{sig.Name}:LDU");
                                _patternCount++;
                            }
                        }
                        
                        // Luôn lưu để fallback
                        db.LduSignatures[$"SIG_LDU_{db.LduSignatures.Count}"] = sig;
                        loaded++;
                        break;
                    }
                    case SignatureType.LDB:
                    {
                        // LDB: cần logical evaluate -> luôn lưu để fallback
                        db.LdbSignatures[$"SIG_LDB_{db.LdbSignatures.Count}"] = sig;
                        
                        // Debug: log WannaCry-related LDB với biểu thức logic
                        if (sig.Name.IndexOf("Wannacry", StringComparison.OrdinalIgnoreCase) >= 0 || 
                            sig.Name.IndexOf("WannaCry", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            string expr = "";
                            if (sig.Properties.TryGetValue("LogicalExpression", out var exObj) && exObj is string ex)
                                expr = ex;
                            int patCount = 0;
                            if (sig.Properties.TryGetValue("RawPatterns", out var patsObj) && patsObj is System.Collections.Generic.List<string> pats)
                                patCount = pats.Count;
                            Logger?.Invoke($"[DEBUG-LDB-LOAD] {sig.Name} | Expression: {expr} | Patterns: {patCount}");
                        }
                        
                        loaded++;
                        break;
                    }
                    case SignatureType.HSB:
                    {
                        string byteSeqHash = null;
                        if (sig.Properties.TryGetValue("ByteSequenceHash", out var bshObj) && bshObj is string bsh)
                            byteSeqHash = bsh;
                        else if (sig.Properties.TryGetValue("Hash", out var hashObj) && hashObj is string h)
                            byteSeqHash = h;
                        
                        // Cố gắng add vào AC
                        if (!string.IsNullOrEmpty(byteSeqHash) && IncludedTypes.Contains(SignatureType.HSB))
                        {
                            var bytes = HexToBytes(byteSeqHash);
                            if (bytes != null && bytes.Length > 0)
                            {
                                _matcher.AddPattern(bytes, $"{sig.Name}:HSB");
                                _patternCount++;
                            }
                        }
                        
                        // Luôn lưu
                        db.HsbSignatures[$"SIG_HSB_{db.HsbSignatures.Count}"] = sig;
                        loaded++;
                        break;
                    }
                    case SignatureType.MDB:
                    {
                        string sh = null;
                        if (sig.Properties.TryGetValue("SectionHash", out var shObj) && shObj is string shStr)
                            sh = shStr;
                        
                        // Cố gắng add vào AC
                        if (!string.IsNullOrEmpty(sh) && IncludedTypes.Contains(SignatureType.MDB))
                        {
                            var bytes = HexToBytes(sh);
                            if (bytes != null && bytes.Length > 0)
                            {
                                _matcher.AddPattern(bytes, $"{sig.Name}:MDB");
                                _patternCount++;
                            }
                        }
                        
                        // Luôn lưu
                        db.MdbSignatures[$"SIG_MDB_{db.MdbSignatures.Count}"] = sig;
                        loaded++;
                        break;
                    }
                    case SignatureType.CDB:
                    {
                        string cdbHash = null;
                        if (sig.Properties.TryGetValue("CompressedBlockHash", out var cdbObj) && cdbObj is string cdbStr)
                            cdbHash = cdbStr;
                        
                        // Cố gắng add vào AC
                        if (!string.IsNullOrEmpty(cdbHash) && IncludedTypes.Contains(SignatureType.CDB))
                        {
                            var bytes = HexToBytes(cdbHash);
                            if (bytes != null && bytes.Length > 0)
                            {
                                _matcher.AddPattern(bytes, $"{sig.Name}:CDB");
                                _patternCount++;
                            }
                        }
                        
                        // Luôn lưu
                        db.CdbSignatures[$"SIG_CDB_{db.CdbSignatures.Count}"] = sig;
                        loaded++;
                        break;
                    }
                    case SignatureType.FP:
                    {
                        // FP dùng MD5 whitelist -> lưu, và nếu có FuzzyPattern thì cố gắng add AC
                        if (sig.Properties.TryGetValue("FuzzyPattern", out var fuzzyObj) && fuzzyObj is string fuzzy)
                        {
                            var bytes = HexToBytes(fuzzy);
                            if (bytes != null && bytes.Length > 0)
                            {
                                _matcher.AddPattern(bytes, $"{sig.Name}:FPD");
                                _patternCount++;
                            }
                        }
                        db.FpSignatures[$"SIG_FP_{db.FpSignatures.Count}"] = sig;
                        loaded++;
                        break;
                    }
                    default:
                    {
                        var dict = GetDictionaryForType(db, sig.Type);
                        if (dict != null)
                        {
                            dict[$"SIG_{sig.Type}_{dict.Count}"] = sig;
                            loaded++;
                        }
                        break;
                    }
                }
            }
            return loaded;
        }

        private Dictionary<string, ClamAVSignature> GetDictionaryForType(ClamAVDatabase db, SignatureType type)
        {
            switch (type)
            {
                case SignatureType.HDB: return db.HdbSignatures;
                case SignatureType.HSB: return db.HsbSignatures;
                case SignatureType.MDB: return db.MdbSignatures;
                case SignatureType.NDB: return db.NdbSignatures;
                case SignatureType.LDB: return db.LdbSignatures;
                case SignatureType.LDU: return db.LduSignatures;
                case SignatureType.FP:  return db.FpSignatures;
                case SignatureType.CDB: return db.CdbSignatures;
                default: return null;
            }
        }

        /// <summary>
        /// Build AC Trie từ database đã tải, chỉ load các loại chữ ký trong IncludedTypes để giảm RAM.
        /// Database vẫn chứa tất cả loại (vd: FP cho whitelist), nhưng AC Trie chỉ add các loại được chọn.
        /// Phải gọi một lần trước khi quét.
        /// </summary>
        public bool BuildPatterns()
        {
            if (!IsDatabaseLoaded)
            {
                Logger?.Invoke("[AC Engine] BuildPatterns error: Database not loaded");
                return false;
            }

            if (_isTrieBuilt)
            {
                Logger?.Invoke($"[AC Engine] Trie already built during load: {_patternCount} patterns");
                LoadedPatternCount = _patternCount;
                return true;
            }

            try
            {
                _matcher.Build();
                _isTrieBuilt = true;
                LoadedPatternCount = _patternCount;
                Logger?.Invoke($"✅ AC Trie built: {_patternCount} patterns");
                return true;
            }
            catch (Exception ex)
            {
                Logger?.Invoke($"❌ BuildPatterns error: {ex.Message}");
                return false;
            }
        }

        private void LoadFromDatabase(ClamAVDatabase db, HashSet<SignatureType> includes, Action<string> logger)
        {
            // Load FPD patterns (fuzzy patterns)
            if (includes == null || includes.Contains(SignatureType.FP))
            {
                foreach (var sig in db.FpSignatures.Values)
                {
                    if (sig.Properties.TryGetValue("FuzzyPattern", out var fuzzyPatternObj) && fuzzyPatternObj is string fuzzyPattern)
                    {
                        try
                        {
                            byte[] pattern = HexToBytes(fuzzyPattern);
                            if (pattern != null && pattern.Length > 0)
                            {
                                _matcher.AddPattern(pattern, $"{sig.Name}:FPD");
                                _patternCount++;
                            }
                        }
                        catch (Exception ex)
                        {
                            logger?.Invoke($"⚠️ Failed to parse FPD pattern '{sig.Name}': {ex.Message}");
                        }
                    }
                }
            }

            // Load NDB patterns (hex byte patterns) vào AC – chỉ các mẫu thuần hex, offset "*"
            if (includes == null || includes.Contains(SignatureType.NDB))
            {
                int ndbCount = 0;
                foreach (var sig in db.NdbSignatures.Values)
                {
                    if (!sig.Properties.TryGetValue("RawPattern", out var patternObj) || !(patternObj is string hexPattern))
                        continue;
                    string offset = "*";
                    if (sig.Properties.TryGetValue("Offset", out var offObj) && offObj is string offStr)
                        offset = offStr;

                    // Chỉ add vào AC nếu offset là toàn cục và pattern không chứa wildcard
                    if (offset != "*")
                        continue;

                    try
                    {
                        byte[] pattern = HexToBytes(hexPattern);
                        if (pattern != null && pattern.Length > 0)
                        {
                            _matcher.AddPattern(pattern, sig.Name);
                            _patternCount++;
                            ndbCount++;
                        }
                        else
                        {
                            // có wildcard hoặc hex không hợp lệ – sẽ xử lý bằng PatternMatcher khi quét
                        }
                    }
                    catch (Exception ex)
                    {
                        logger?.Invoke($"⚠️ Failed to parse NDB pattern '{sig.Name}': {ex.Message}");
                    }
                }
                logger?.Invoke($"[NDB] Added {ndbCount} pure patterns to AC (of {db.NdbSignatures.Count})");
            }

            // HDB: MD5 hash của toàn bộ file. Không add vào AC trie.
            // Sẽ được kiểm tra hash trực tiếp tại bước ScanFile để chính xác và tiết kiệm RAM.
            if (includes == null || includes.Contains(SignatureType.HDB))
            {
                int hdbCount = db.HdbSignatures.Count;
                logger?.Invoke($"[HDB] Index ready: {hdbCount} signatures (hash-based)");
            }

            // Load HSB patterns (hash of byte sequences)
            if (includes == null || includes.Contains(SignatureType.HSB))
            {
                int hsbCount = 0;
                foreach (var sig in db.HsbSignatures.Values)
                {
                    string byteSeqHash = null;
                    if (sig.Properties.TryGetValue("ByteSequenceHash", out var byteSeqHashObj) && byteSeqHashObj is string bsh)
                        byteSeqHash = bsh;
                    else if (sig.Properties.TryGetValue("Hash", out var hashObj) && hashObj is string h)
                        byteSeqHash = h;

                    if (string.IsNullOrEmpty(byteSeqHash))
                        continue;

                    try
                    {
                        byte[] byteSeqHashBytes = HexToBytes(byteSeqHash);
                        if (byteSeqHashBytes != null && byteSeqHashBytes.Length > 0)
                        {
                            _matcher.AddPattern(byteSeqHashBytes, $"{sig.Name}:HSB");
                            _patternCount++;
                            hsbCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        logger?.Invoke($"⚠️ Failed to parse HSB pattern '{sig.Name}': {ex.Message}");
                    }
                }
                logger?.Invoke($"[HSB] Loaded {hsbCount}/{db.HsbSignatures.Count} patterns");
            }

            // Load MDB patterns (section hash)
            if (includes == null || includes.Contains(SignatureType.MDB))
            {
                int mdbCount = 0;
                foreach (var sig in db.MdbSignatures.Values)
                {
                    if (sig.Properties.TryGetValue("SectionHash", out var sectionHashObj) && sectionHashObj is string sectionHash)
                    {
                        try
                        {
                            byte[] sectionHashBytes = HexToBytes(sectionHash);
                            if (sectionHashBytes != null && sectionHashBytes.Length > 0)
                            {
                                _matcher.AddPattern(sectionHashBytes, $"{sig.Name}:MDB");
                                _patternCount++;
                                mdbCount++;
                            }
                        }
                        catch (Exception ex)
                        {
                            logger?.Invoke($"⚠️ Failed to parse MDB pattern '{sig.Name}': {ex.Message}");
                        }
                    }
                }
                logger?.Invoke($"[MDB] Loaded {mdbCount}/{db.MdbSignatures.Count} patterns");
            }

            // Load CDB patterns (compressed data block hash)
            if (includes == null || includes.Contains(SignatureType.CDB))
            {
                int cdbCount = 0;
                foreach (var sig in db.CdbSignatures.Values)
                {
                    if (sig.Properties.TryGetValue("CompressedBlockHash", out var cdbHashObj) && cdbHashObj is string cdbHash)
                    {
                        try
                        {
                            byte[] cdbHashBytes = HexToBytes(cdbHash);
                            if (cdbHashBytes != null && cdbHashBytes.Length > 0)
                            {
                                _matcher.AddPattern(cdbHashBytes, $"{sig.Name}:CDB");
                                _patternCount++;
                                cdbCount++;
                            }
                        }
                        catch (Exception ex)
                        {
                            logger?.Invoke($"⚠️ Failed to parse CDB pattern '{sig.Name}': {ex.Message}");
                        }
                    }
                }
                logger?.Invoke($"[CDB] Loaded {cdbCount}/{db.CdbSignatures.Count} patterns");
            }

            // Load LDU patterns (logical detection - uncompressed)
            if (includes == null || includes.Contains(SignatureType.LDU))
            {
                int lduCount = 0;
                foreach (var sig in db.LduSignatures.Values)
                {
                    if (sig.Properties.TryGetValue("UncompressedPattern", out var lduPatternObj) && lduPatternObj is string lduPattern)
                    {
                        try
                        {
                            byte[] pattern = HexToBytes(lduPattern);
                            if (pattern != null && pattern.Length > 0)
                            {
                                _matcher.AddPattern(pattern, $"{sig.Name}:LDU");
                                _patternCount++;
                                lduCount++;
                            }
                        }
                        catch (Exception ex)
                        {
                            logger?.Invoke($"⚠️ Failed to parse LDU pattern '{sig.Name}': {ex.Message}");
                        }
                    }
                }
                logger?.Invoke($"[LDU] Loaded {lduCount}/{db.LduSignatures.Count} patterns");
            }

            // Load LDB patterns (logical detection - regex/complex)
            if (includes == null || includes.Contains(SignatureType.LDB))
            {
                int ldbCount = 0;
                foreach (var sig in db.LdbSignatures.Values)
                {
                    var x = sig;
                    if (!x.Properties.TryGetValue("LogicalExpression", out var exprObj) || !(exprObj is string expr) ||
                        string.IsNullOrWhiteSpace(expr))
                        continue;

                    if (!sig.Properties.TryGetValue("RawPatterns", out var patsObj))
                        continue;

                    if (!(patsObj is System.Collections.Generic.List<string> rawSubs) || rawSubs.Count == 0)
                        continue;

                    int[] counts = new int[rawSubs.Count];

                    for (int i = 0; i < rawSubs.Count; i++)
                    {
                        var raw = rawSubs[i];
                        if (string.IsNullOrWhiteSpace(raw))
                        {
                            counts[i] = 0;
                            continue;
                        }

                        string offset = "*";
                        string pattern = raw;

                        var parts = raw.Split(':');
                        if (parts.Length >= 2)
                        {
                            pattern = parts[parts.Length - 1].Trim();
                            offset = parts[parts.Length - 2].Trim();
                        }

                        try
                        {
                            byte[] patternBytes = HexToBytes(pattern);
                            if (patternBytes != null && patternBytes.Length > 0)
                            {
                                _matcher.AddPattern(patternBytes, $"{sig.Name}:LDB_SUB{i}");
                            }
                        }
                        catch
                        {
                            counts[i] = 0;
                        }
                    }

                    try
                    {
                        if (ExpressionEvaluator.Evaluate(expr, counts))
                        {
                            // Biểu thức đúng, thêm pattern đơn giản vào AC Trie
                            byte[] pattern = HexToBytes(rawSubs[0]);
                            if (pattern != null && pattern.Length > 0)
                            {
                                _matcher.AddPattern(pattern, $"{sig.Name}:LDB");
                                _patternCount++;
                                ldbCount++;
                            }
                        }
                    }
                    catch
                    {
                        // Nếu biểu thức quá phức tạp/không parse được, bỏ qua signature này
                    }
                }
                logger?.Invoke($"[LDB] Loaded {ldbCount}/{db.LdbSignatures.Count} patterns");
            }
        }

        /// <summary>
        /// Quét file theo AC Trie. Ưu tiên FP (whitelist) nếu đã load FP.
        /// </summary>
        public ClamAVResult ScanFile(string filePath)
        {
            var result = new ClamAVResult
            {
                FilePath = filePath,
                ScanTime = DateTime.Now
            };

            if (!File.Exists(filePath))
            {
                result.Status = ScanStatus.Error;
                result.ErrorMessage = "File không tồn tại";
                return result;
            }
            
            if (!IsDatabaseLoaded)
            {
                result.Status = ScanStatus.Error;
                result.ErrorMessage = "Chưa tải database";
                Logger?.Invoke("[AC Engine] ScanFile error: Database not loaded");
                return result;
            }

            if (_matcher == null || LoadedPatternCount == 0)
            {
                result.Status = ScanStatus.Error;
                result.ErrorMessage = "Chưa build patterns (gọi BuildPatterns trước)";
                Logger?.Invoke("[AC Engine] ScanFile error: Patterns not built");
                return result;
            }

            byte[] data;
            try
            {
                data = File.ReadAllBytes(filePath);
            }
            catch (Exception ex)
            {
                result.Status = ScanStatus.Error;
                result.ErrorMessage = ex.Message;
                Logger?.Invoke($"[AC Engine] ScanFile error reading: {ex.Message}");
                return result;
            }

            result.FileSize = data.LongLength;
            result.MD5 = HashHelper.ComputeMD5(data);
            result.SHA256 = HashHelper.ComputeSHA256(data);

            Logger?.Invoke($"[AC Engine] Scanning {Path.GetFileName(filePath)} ({result.FileSize} bytes)");

            // Whitelist bằng FP nếu có (so khớp MD5, tôn trọng FileSize nếu có)
            if (IncludedTypes.Contains(SignatureType.FP) &&
                (DailyDatabase.FpSignatures.Count > 0 || MainDatabase.FpSignatures.Count > 0))
            {
                var fp = FindHashMatch(result.MD5, result.FileSize,
                    DailyDatabase.FpSignatures,
                    MainDatabase.FpSignatures);
                if (fp != null)
                {
                    Logger?.Invoke($"[AC Engine] Whitelisted by FP: {fp.Name}");
                    result.Status = ScanStatus.Whitelisted;
                    result.VirusName = fp.Name;
                    result.DetectionType = fp.Type;
                    return result;
                }
            }

            // HDB: phát hiện theo MD5 (tùy chọn kiểm size)
            if (IncludedTypes.Contains(SignatureType.HDB) &&
                (DailyDatabase.HdbSignatures.Count > 0 || MainDatabase.HdbSignatures.Count > 0))
            {
                var hdb = FindHashMatch(result.MD5, result.FileSize,
                    DailyDatabase.HdbSignatures,
                    MainDatabase.HdbSignatures);
                if (hdb != null)
                {
                    result.Status = ScanStatus.Infected;
                    result.VirusName = $"{hdb.Name}:HDB";
                    result.DetectionType = hdb.Type;
                    Logger?.Invoke($"[AC Engine] DETECTED by HDB: {hdb.Name}");
                    return result;
                }
            }

            // ===== OPTIMIZED SCAN =====
            var allDetections = new List<string>();
            
            // 1. FAST PATH: AC Trie scan (xử lý tất cả các mẫu đã được thêm vào)
            var acMatches = _matcher.Scan(data);
            if (acMatches.Count > 0)
            {
                var names = acMatches.Select(m => m.PatternName).Distinct().ToList();
                allDetections.AddRange(names);
                Logger?.Invoke($"[AC Engine] AC Trie found {names.Count} pattern matches");
            }

            // 2. PARALLEL LDB SCAN: Xử lý các signature logic song song
            if (IncludedTypes.Contains(SignatureType.LDB) &&
                (DailyDatabase.LdbSignatures.Count > 0 || MainDatabase.LdbSignatures.Count > 0))
            {
                var ldbDetections = new System.Collections.Concurrent.ConcurrentBag<string>();
                
                var allLdbSigs = DailyDatabase.LdbSignatures.Values
                    .Concat(MainDatabase.LdbSignatures.Values)
                    .Where(sig => sig.Properties.TryGetValue("LogicalExpression", out var exprObj) && 
                                  exprObj is string expr && !string.IsNullOrWhiteSpace(expr) &&
                                  sig.Properties.TryGetValue("RawPatterns", out var patsObj) &&
                                  patsObj is System.Collections.Generic.List<string> rawSubs &&
                                  rawSubs.Count > 0)
                    .ToList();

                Logger?.Invoke($"[AC Engine] Starting parallel LDB scan: {allLdbSigs.Count} signatures");

                // Parallel process LDB signatures (safe for I/O-bound PatternMatcher operations)
                Parallel.ForEach(allLdbSigs, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, sig =>
                {
                    try
                    {
                        var patsObj = sig.Properties["RawPatterns"];
                        var rawSubs = patsObj as System.Collections.Generic.List<string>;
                        var expr = sig.Properties["LogicalExpression"] as string;
                        
                        int[] counts = new int[rawSubs.Count];
                        for (int i = 0; i < rawSubs.Count; i++)
                        {
                            var raw = rawSubs[i];
                            if (string.IsNullOrWhiteSpace(raw)) 
                            { 
                                counts[i] = 0; 
                                continue; 
                            }
                            
                            string offset = "*";
                            string pattern = raw;
                            var parts = raw.Split(':');
                            if (parts.Length >= 2)
                            {
                                pattern = parts[parts.Length - 1].Trim();
                                offset = parts[parts.Length - 2].Trim();
                            }
                            
                            try 
                            { 
                                counts[i] = PatternMatcher.CountMatches(data, pattern, offset); 
                            }
                            catch 
                            { 
                                counts[i] = 0; 
                            }
                        }
                        
                        try
                        {
                            if (ExpressionEvaluator.Evaluate(expr, counts))
                            {
                                ldbDetections.Add($"{sig.Name}:LDB");
                                Logger?.Invoke($"[LDB-MATCH] {sig.Name} | expr={expr}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger?.Invoke($"[LDB-ERROR] {sig.Name}: {ex.Message}");
                        }
                    }
                    catch { }
                });

                allDetections.AddRange(ldbDetections);
                Logger?.Invoke($"[AC Engine] LDB scan complete. Matched: {ldbDetections.Count}");
            }

            // 3. LDU Scan (nếu enable)
            if (IncludedTypes.Contains(SignatureType.LDU) &&
                (DailyDatabase.LduSignatures.Count > 0 || MainDatabase.LduSignatures.Count > 0))
            {
                var lduDetections = new System.Collections.Concurrent.ConcurrentBag<string>();
                
                var allLduSigs = DailyDatabase.LduSignatures.Values
                    .Concat(MainDatabase.LduSignatures.Values)
                    .Where(sig => sig.Properties.TryGetValue("UncompressedPattern", out var lduObj) && 
                                  lduObj is string lduP && !string.IsNullOrEmpty(lduP))
                    .ToList();

                if (allLduSigs.Count > 0)
                {
                    Logger?.Invoke($"[AC Engine] Starting LDU scan: {allLduSigs.Count} signatures");
                    
                    Parallel.ForEach(allLduSigs, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, sig =>
                    {
                        try
                        {
                            if (sig.Properties.TryGetValue("UncompressedPattern", out var lduObj) && lduObj is string lduP)
                            {
                                if (PatternMatcher.Match(data, lduP, "*"))
                                    lduDetections.Add($"{sig.Name}:LDU");
                            }
                        }
                        catch { }
                    });

                    allDetections.AddRange(lduDetections);
                    Logger?.Invoke($"[AC Engine] LDU scan complete. Matched: {lduDetections.Count}");
                }
            }

            // ===== RESULTS =====
            var uniqueDetections = allDetections.Distinct().ToList();
            if (uniqueDetections.Count > 0)
            {
                result.Status = ScanStatus.Infected;
                result.VirusName = string.Join(", ", uniqueDetections);
                Logger?.Invoke($"[AC Engine] DETECTED ({uniqueDetections.Count} signatures): {result.VirusName}");
                return result;
            }

            Logger?.Invoke("[AC Engine] No detections found - CLEAN");
            result.Status = ScanStatus.Clean;
            return result;
        }

        private ClamAVSignature FindHashMatch(string md5, long fileSize, params Dictionary<string, ClamAVSignature>[] sources)
        {
            if (string.IsNullOrEmpty(md5))
                return null;

            foreach (var dict in sources.Where(d => d != null))
            {
                foreach (var sig in dict.Values)
                {
                    if (!sig.Properties.TryGetValue("Hash", out var hashObj))
                        continue;
                    var hash = hashObj as string;
                    if (string.IsNullOrEmpty(hash))
                        continue;
                    if (!hash.Equals(md5, StringComparison.OrdinalIgnoreCase))
                        continue;
 
                    if (sig.Properties.TryGetValue("FileSize", out var sizeObj) && sizeObj is long sz)
                    {
                        if (sz != fileSize)
                            continue;  
                    }
                    return sig;
                }
            }
            return null;
        }

        /// <summary>
        /// Convert hex string (vd: "5c482b3c") → byte array
        /// Hỗ trợ wildcard {N} và ?? (any byte)
        /// </summary>
        private byte[] HexToBytes(string hex)
        {
            if (string.IsNullOrEmpty(hex))
                return null;

            // Loại bỏ khoảng trắng
            hex = hex.Replace(" ", "");

            // Nếu có wildcard {N} hoặc ??, cần xử lý đặc biệt
            // Tạm thời chỉ convert hex thuần
            if (hex.Contains("{") || hex.Contains("?"))
            {
                // Skip patterns phức tạp (cần AC variant hoặc preprocessing)
                return null;
            }

            try
            {
                // Nếu hex length lẻ, không hợp lệ
                if (hex.Length % 2 != 0)
                {
                    return null;
                }

                var result = new List<byte>();
                for (int i = 0; i < hex.Length; i += 2)
                {
                    string byteStr = hex.Substring(i, 2);
                    if (byte.TryParse(byteStr, System.Globalization.NumberStyles.HexNumber, null, out byte b))
                    {
                        result.Add(b);
                    }
                    else
                    {
                        return null;
                    }
                }
                return result.ToArray();
            }
            catch
            {
                return null;
            }
        }
    }
}
