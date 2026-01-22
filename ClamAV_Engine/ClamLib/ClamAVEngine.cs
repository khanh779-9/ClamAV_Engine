using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using ClamAV_Engine.ClamLib.Helpers;

namespace ClamAV_Engine.ClamLib
{
    public class ClamAVEngine
    {
        public ClamAVDatabase DailyDatabase { get; } = new ClamAVDatabase();
        public ClamAVDatabase MainDatabase { get; } = new ClamAVDatabase();

        public int TotalSignatures => DailyDatabase.TotalSignatures + MainDatabase.TotalSignatures;

        public bool IsDatabaseLoaded => TotalSignatures > 0;
 
        public Action<string> Logger { get; set; }

        public bool LoadDatabaseFolder(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
                return false;

            DailyDatabase.Clear();
            MainDatabase.Clear();

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
                // Nếu thư mục không có daily/main, coi như một database đơn (đưa vào Daily)
                loaded += LoadDatabaseFromDirectory(folderPath, DailyDatabase);
            }

            return loaded > 0;
        }

        private int LoadDatabaseFromDirectory(string directory, ClamAVDatabase db)
        {
            int count = 0;

            if (!Directory.Exists(directory))
                return 0;

            foreach (var file in Directory.EnumerateFiles(directory, "*.*", SearchOption.TopDirectoryOnly))
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
                    case ".fp": type = SignatureType.FP; break;
                    default:
                        break;
                }

                if (!type.HasValue)
                    continue;

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
                if (sig == null)
                    continue;

                var dict = GetDictionaryForType(db, sig.Type);
                if (dict == null || string.IsNullOrEmpty(sig.Name))
                    continue;

                string key = $"SIG__{sig.Target}__{loaded}";

                dict[key] = sig;
                loaded++;
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
                case SignatureType.FP: return db.FpSignatures;
                case SignatureType.CDB: return db.CdbSignatures;
                default:
                    return null;
            }
        }

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
                result.ErrorMessage = "File does not exist";
                return result;
            }
            if (!IsDatabaseLoaded)
            {
                result.Status = ScanStatus.Error;
                result.ErrorMessage = "Database not loaded";
                return result;
            }

            var sw = Stopwatch.StartNew();

            try
            {
                Logger?.Invoke($"[SCAN] Starting scan of file: {filePath}");

                var fileInfo = new FileInfo(filePath);
                result.FileSize = fileInfo.Length;

                byte[] data = File.ReadAllBytes(filePath);

                // Calculate MD5 and SHA256
                using (var md5 = MD5.Create())
                using (var sha256 = SHA256.Create())
                {
                    var md5Bytes = md5.ComputeHash(data);
                    var shaBytes = sha256.ComputeHash(data);

                    result.MD5 = BytesToHex(md5Bytes);
                    result.SHA256 = BytesToHex(shaBytes);
                }

                TargetType fileTarget = TargetTypeHelper.DetectTarget(data);

                // 1. Check whitelist (FP) first
                var fpSig = FindHashMatch(result.MD5, result.FileSize, fileTarget,
                    DailyDatabase.FpSignatures,
                    MainDatabase.FpSignatures);

                Logger?.Invoke($"[SCAN] Checking whitelist (FP)... ");

                if (fpSig != null)
                {
                    Logger?.Invoke($"[FP] Whitelisted by signature: {fpSig.Name} - [{fpSig.Target}]");
                    result.Status = ScanStatus.Whitelisted;
                    result.VirusName = fpSig.Name;
                    result.DetectionType = fpSig.Type;
                    return result;
                }

                Logger?.Invoke($"[SCAN] Continuing scan...");

                // 2. Check hash-based (HDB/HSB)
                var hdbSig = FindHashMatch(result.MD5, result.FileSize, fileTarget,
                    DailyDatabase.HdbSignatures,
                    DailyDatabase.HsbSignatures,
                    MainDatabase.HdbSignatures,
                    MainDatabase.HsbSignatures);

                Logger?.Invoke($"[SCAN] Checking hash-based signatures (HDB/HSB)...");

                if (hdbSig != null)
                {
                    Logger?.Invoke($"[HASH] Match HDB/HSB: {hdbSig.Name} - [{hdbSig.Target}]");
                    result.Status = ScanStatus.Infected;
                    result.VirusName = hdbSig.Name;
                    result.DetectionType = hdbSig.Type;
                    return result;
                }

                Logger?.Invoke($"[NDB] Checking body-based signatures (NDB)...");

                // 3. Check NDB (hex/body signatures)
                var ndbSig = FindNdbMatch(data, fileTarget,
                    DailyDatabase.NdbSignatures,
                    MainDatabase.NdbSignatures);

                if (ndbSig != null)
                {
                    Logger?.Invoke($"[NDB] Match body signature: {ndbSig.Name} - [{ndbSig.Target}]");
                    result.Status = ScanStatus.Infected;
                    result.VirusName = ndbSig.Name;
                    result.DetectionType = ndbSig.Type;
                    return result;
                }

                Logger?.Invoke($"[LDB] Checking logical signatures (LDB/LDU)...");

                //var arrLdbSigs = DailyDatabase.LdbSignatures.Values.Concat(DailyDatabase.LduSignatures.Values)
                //    .Concat(MainDatabase.LdbSignatures.Values)
                //    .Concat(MainDatabase.LduSignatures.Values)
                //    .ToArray();

                // 4. Check LDB/LDU (logical signatures)
                var ldbSig = FindLdbMatch(data,
                    fileTarget,
                     DailyDatabase.LdbSignatures,
                     DailyDatabase.LduSignatures,
                     MainDatabase.LdbSignatures,
                     MainDatabase.LduSignatures
                );

                if (ldbSig != null)
                {
                    Logger?.Invoke($"[LDB] Logical match: {ldbSig.Name} - [{ldbSig.Type}]");
                    result.Status = ScanStatus.Infected;
                    result.VirusName = ldbSig.Name;
                    result.DetectionType = ldbSig.Type;
                    return result;
                }

                result.Status = ScanStatus.Clean;
                Logger?.Invoke("[SCAN] File is clean (no matching signatures found).");
                return result;
            }
            catch (Exception ex)
            {
                result.Status = ScanStatus.Error;
                result.ErrorMessage = ex.Message;
                Logger?.Invoke($"[ERROR] Error scanning file: {ex.Message}");
                return result;
            }
            finally
            {
                sw.Stop();
                result.ScanDuration = sw.Elapsed;
            }
        }

        private string BytesToHex(byte[] bytes)
        {
            char[] c = new char[bytes.Length * 2];
            int b;
            for (int i = 0; i < bytes.Length; i++)
            {
                b = bytes[i] >> 4;
                c[i * 2] = (char)(55 + b + (((b - 10) >> 31) & -7));
                b = bytes[i] & 0xF;
                c[i * 2 + 1] = (char)(55 + b + (((b - 10) >> 31) & -7));
            }
            return new string(c).ToLowerInvariant();
        }

        private ClamAVSignature FindHashMatch(string md5, long size, TargetType targetType, params Dictionary<string, ClamAVSignature>[] sources)
        {
            if (string.IsNullOrEmpty(md5))
                return null;

            foreach (var dict in sources)
            {
                if (dict == null)
                    continue;

                var sd = dict.Values.Where(x => x.Target == TargetType.Any || x.Target == targetType).ToList();
                foreach (var sig in sd)
                {
                    if (!sig.Properties.TryGetValue("Hash", out var hashObj))
                        continue;


                    var hash = hashObj as string;
                    if (string.IsNullOrEmpty(hash))
                        continue;

                    if (!hash.Equals(md5, StringComparison.OrdinalIgnoreCase))
                        continue;

                    Logger?.Invoke($"[HASH] Scanning signature: {sig.Name} - [Index: {sd.IndexOf(sig)}, Target: {sig.Target}]");

                    // Check file size if available
                    if (sig.Properties.TryGetValue("FileSize", out var sizeObj) && sizeObj is long sigSize)
                    {
                        if (sigSize != size)
                            continue;
                    }

                    return sig;
                }
            }

            return null;
        }

        private ClamAVSignature FindNdbMatch(byte[] data, TargetType targetType, params Dictionary<string, ClamAVSignature>[] sources)
        {
            if (data == null || data.Length == 0)
                return null;

            foreach (var dict in sources.Where(x => x != null))
            {
                //if (dict == null)
                //    continue;

                var sd = dict.Values.Where(x => x.Target == TargetType.Any || x.Target == targetType).ToList();
                foreach (var sig in sd)
                {
                    if (!sig.Properties.TryGetValue("RawPattern", out var patObj))
                        continue;

                    var pattern = patObj as string;
                    if (string.IsNullOrEmpty(pattern))
                        continue;

                    sig.Properties.TryGetValue("Offset", out var offObj);
                    var offset = offObj as string ?? "*";

                    Logger?.Invoke($"[NDB] Scanning signature: {sig.Name} - [Index: {sd.IndexOf(sig)}, Target: {sig.Target}]");

                    try
                    {
                        if (PatternMatcher.Match(data, pattern, offset))
                            return sig;
                    }
                    catch
                    {
                        // If the pattern is too complex/unsupported, skip that signature
                    }
                }
            }

            return null;
        }


        private ClamAVSignature FindLdbMatch(
            byte[] data,
            TargetType targetType,
            params Dictionary<string, ClamAVSignature>[] sigs)
        {
            if (data == null || data.Length == 0)
                return null;

            long fileSize = data.LongLength;

            var sources = sigs
                .Where(x => x != null)
                .SelectMany(x => x.Values)
                .ToList();

            var sd = sources.Where(x =>
            {
                try
                {
                    if (x == null || x.Properties == null)
                        return false;

                    if (x.Target != TargetType.Any && x.Target != targetType)
                        return false;

                    if (x.Properties.TryGetValue("FileSizeMin", out var minObj))
                        if (!(minObj is long minSize) || fileSize < minSize)
                            return false;

                    if (x.Properties.TryGetValue("FileSizeMax", out var maxObj))
                        if (!(maxObj is long maxSize) || fileSize > maxSize)
                            return false;

                    if (!x.Properties.TryGetValue("LogicalExpression", out var exprObj) ||
                        !(exprObj is string expr) ||
                        string.IsNullOrWhiteSpace(expr))
                        return false;

                    return true;
                }
                catch
                {
                    return false;
                }
            }).ToList();


            foreach (var sig in sd)
            {
                //if (sig.Properties.TryGetValue("FileSizeMax", out var maxObj) && maxObj is long maxSize && fileSize > maxSize)
                //    continue;

                //if (!sig.Properties.TryGetValue("LogicalExpression", out var exprObj))
                //    continue;

                //if (!(exprObj is string expr) || string.IsNullOrWhiteSpace(expr))
                //    continue;

                if (!sig.Properties.TryGetValue("RawPatterns", out var patsObj))
                    continue;

                if (!(patsObj is System.Collections.Generic.List<string> rawSubs) || rawSubs.Count == 0)
                    continue;

                Logger?.Invoke($"[LDB] Scanning signature: {sig.Name} - [Index: {sources.ToList().IndexOf(sig)}, Target: {sig.Target}]");

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
                    var expr = (string)sig.Properties["LogicalExpression"];
                    if (ExpressionEvaluator.Evaluate(expr, counts))
                        return sig;
                }
                catch
                {
                    // If the expression is too complex/cannot be parsed, skip this signature
                }
            }
            return null;
        }


        private ClamAVSignature FindLdbMatch(byte[] data, params Dictionary<string, ClamAVSignature>[] sources)
        {
            if (data == null || data.Length == 0)
                return null;

            foreach (var dict in sources)
            {
                if (dict == null)
                    continue;

                foreach (var sig in dict.Values)
                {
                    // Log the name of the signature being scanned (adjust frequency if needed later)
                    Logger?.Invoke($"[LDB] Scanning signature: {sig.Name} - {sig.Target}");

                    if (!sig.Properties.TryGetValue("LogicalExpression", out var exprObj))
                        continue;
                    if (!(exprObj is string expr) || string.IsNullOrWhiteSpace(expr))
                        continue;

                    if (!sig.Properties.TryGetValue("RawPatterns", out var patsObj))
                        continue;
                    if (!(patsObj is System.Collections.Generic.List<string> rawSubs) || rawSubs.Count == 0)
                        continue;

                    Logger?.Invoke($"[LDB] Scanning signature: {sig.Name} - {sig.Target}");

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
                            return sig;
                    }
                    catch
                    {
                        // If the expression is too complex/cannot be parsed, skip this signature
                    }
                }
            }

            return null;
        }
    }
}