using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ClamAV_Engine.ClamLib;

namespace ClamAV_Engine
{
    public partial class Form1 : Form
    {
     
        private bool isScanning = false;

        ClamAVEngine _clamEng;
        private BackgroundWorker scanWorker;
      
        public Form1()
        {
            InitializeComponent();
            _clamEng = new ClamAVEngine();

            // Gán logger để engine có thể báo tiến trình (đặc biệt khi dò LDB)
            _clamEng.Logger = AddLog;

            // Add event handlers for ListView
            lvSignatures.DoubleClick += LvSignatures_DoubleClick;

            // Initialize BackgroundWorker
            scanWorker = new BackgroundWorker();
            scanWorker.WorkerReportsProgress = true;
            scanWorker.DoWork += ScanWorker_DoWork;
            scanWorker.ProgressChanged += ScanWorker_ProgressChanged;
            scanWorker.RunWorkerCompleted += ScanWorker_RunWorkerCompleted;
        }

        private void LvSignatures_DoubleClick(object sender, EventArgs e)
        {
            if (lvSignatures.SelectedItems.Count == 0)
                return;

            var selectedItem = lvSignatures.SelectedItems[0];
            string sigName = selectedItem.Text;

            // Find the signature
            ClamAVSignature sig = FindSignatureByName(sigName);
            if (sig == null)
            {
                MessageBox.Show("Không tìm thấy signature!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Show detailed info
            ShowSignatureDetails(sig);
        }

        private ClamAVSignature FindSignatureByName(string name)
        {
            // Search in Daily database
            if (_clamEng.DailyDatabase.HdbSignatures.ContainsKey(name))
                return _clamEng.DailyDatabase.HdbSignatures[name];
            if (_clamEng.DailyDatabase.HsbSignatures.ContainsKey(name))
                return _clamEng.DailyDatabase.HsbSignatures[name];
            if (_clamEng.DailyDatabase.MdbSignatures.ContainsKey(name))
                return _clamEng.DailyDatabase.MdbSignatures[name];
            if (_clamEng.DailyDatabase.NdbSignatures.ContainsKey(name))
                return _clamEng.DailyDatabase.NdbSignatures[name];
            if (_clamEng.DailyDatabase.LdbSignatures.ContainsKey(name))
                return _clamEng.DailyDatabase.LdbSignatures[name];
            if (_clamEng.DailyDatabase.LduSignatures.ContainsKey(name))
                return _clamEng.DailyDatabase.LduSignatures[name];
            if (_clamEng.DailyDatabase.FpSignatures.ContainsKey(name))
                return _clamEng.DailyDatabase.FpSignatures[name];
            if (_clamEng.DailyDatabase.CdbSignatures.ContainsKey(name))
                return _clamEng.DailyDatabase.CdbSignatures[name];

            // Search in Main database
            if (_clamEng.MainDatabase.HdbSignatures.ContainsKey(name))
                return _clamEng.MainDatabase.HdbSignatures[name];
            if (_clamEng.MainDatabase.HsbSignatures.ContainsKey(name))
                return _clamEng.MainDatabase.HsbSignatures[name];
            if (_clamEng.MainDatabase.MdbSignatures.ContainsKey(name))
                return _clamEng.MainDatabase.MdbSignatures[name];
            if (_clamEng.MainDatabase.NdbSignatures.ContainsKey(name))
                return _clamEng.MainDatabase.NdbSignatures[name];
            if (_clamEng.MainDatabase.LdbSignatures.ContainsKey(name))
                return _clamEng.MainDatabase.LdbSignatures[name];
            if (_clamEng.MainDatabase.LduSignatures.ContainsKey(name))
                return _clamEng.MainDatabase.LduSignatures[name];
            if (_clamEng.MainDatabase.FpSignatures.ContainsKey(name))
                return _clamEng.MainDatabase.FpSignatures[name];
            if (_clamEng.MainDatabase.CdbSignatures.ContainsKey(name))
                return _clamEng.MainDatabase.CdbSignatures[name];

            return null;
        }

        private void ShowSignatureDetails(ClamAVSignature sig)
        {
            var sb = new StringBuilder();
            sb.AppendLine("========== SIGNATURE DETAILS ==========");
            sb.AppendLine($"Name: {sig.Name}");
            sb.AppendLine($"Type: {sig.Type}");
            sb.AppendLine($"Target: {sig.Target}");
            sb.AppendLine($"Official: {(sig.IsUnofficial ? "No" : "Yes")}");
            sb.AppendLine($"Wildcard: {sig.IsWildcard}");
            sb.AppendLine();

            // Properties
            sb.AppendLine("Properties:");
            foreach (var prop in sig.Properties)
            {
                string value = FormatPropertyValue(prop.Key, prop.Value);
                sb.AppendLine($"  {prop.Key}: {value}");
            }

            // Special handling for LDB subsignatures
            if (sig.Type == SignatureType.LDB || sig.Type == SignatureType.LDU)
            {
                sb.AppendLine();
                sb.AppendLine("Subsignatures:");
                
                if (sig.Properties.ContainsKey("RawPatterns"))
                {
                    var rawPatterns = sig.Properties["RawPatterns"] as System.Collections.Generic.List<string>;
                    if (rawPatterns != null)
                    {
                        for (int i = 0; i < rawPatterns.Count && i < 20; i++) // Limit to first 20
                        {
                            sb.AppendLine($"  [{i}] {rawPatterns[i]}");
                        }
                        if (rawPatterns.Count > 20)
                        {
                            sb.AppendLine($"  ... và {rawPatterns.Count - 20} subsignatures nữa");
                        }
                    }
                }
            }

            sb.AppendLine();
            sb.AppendLine("======================================");

            // Show in message box
            MessageBox.Show(sb.ToString(), "Chi tiết Signature", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private string FormatPropertyValue(string key, object value)
        {
            if (value == null)
                return "null";

            if (value is System.Collections.Generic.List<byte[]> byteArrayList)
            {
                return $"List<byte[]> ({byteArrayList.Count} items)";
            }

            if (value is System.Collections.Generic.List<string> stringList)
            {
                return $"List<string> ({stringList.Count} items)";
            }

            if (value is byte[] byteArray)
            {
                if (byteArray.Length > 50)
                    return $"byte[{byteArray.Length}] (too long to display)";
                return BitConverter.ToString(byteArray).Replace("-", "");
            }

            string strValue = value.ToString();
            if (strValue.Length > 200)
                return strValue.Substring(0, 197) + "...";

            return strValue;
        }

        private void btnLoadDb_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtDbPath.Text))
            {
                MessageBox.Show("Vui lòng chọn thư mục database!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!Directory.Exists(txtDbPath.Text))
            {
                MessageBox.Show("Thư mục không tồn tại!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                btnLoadDb.Enabled = false;
                lblDbStatus.Text = "Đang tải database...";
                lblDbStatus.ForeColor = Color.Blue;
                AddLog("Bắt đầu tải database từ: " + txtDbPath.Text);

                // Load database using Engine API
                bool loaded = _clamEng.LoadDatabaseFolder(txtDbPath.Text);
                
                if (loaded)
                {
                    lblDbStatus.ForeColor = Color.Green;
                    lblDbStatus.Text = $"Database đã tải: {_clamEng.TotalSignatures:N0} signatures";
                    AddLog($"Database đã tải: {_clamEng.TotalSignatures:N0} signatures");
                    
                    // Load signatures into ListView
                    LoadSignaturesToListView();
                }
                else
                {
                    lblDbStatus.ForeColor = Color.Red;
                    lblDbStatus.Text = "Không tìm thấy database";
                    AddLog("Lỗi: Không tìm thấy file database");
                }
            }
            catch (Exception ex)
            {
                lblDbStatus.Text = "Lỗi khi tải database";
                lblDbStatus.ForeColor = Color.Red;
                AddLog("LỖI: " + ex.Message);
                MessageBox.Show("Lỗi khi tải database: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnLoadDb.Enabled = true;
            }
        }

        private void LoadSignaturesToListView()
        {
            lvSignatures.BeginUpdate();
            lvSignatures.Items.Clear();

            try
            {
                AddLog("Đang tải signatures vào ListView...");
                int count = 0;
                int limit = 999999999;

                // Load Daily Database signatures
                count += AddSignaturesToListView(_clamEng.DailyDatabase.HdbSignatures, "Daily", limit - count);
                if (count >= limit) goto Finish;
                
                count += AddSignaturesToListView(_clamEng.DailyDatabase.HsbSignatures, "Daily", limit - count);
                if (count >= limit) goto Finish;
                
                count += AddSignaturesToListView(_clamEng.DailyDatabase.MdbSignatures, "Daily", limit - count);
                if (count >= limit) goto Finish;
                
                count += AddSignaturesToListView(_clamEng.DailyDatabase.NdbSignatures, "Daily", limit - count);
                if (count >= limit) goto Finish;
                
                count += AddSignaturesToListView(_clamEng.DailyDatabase.LdbSignatures, "Daily", limit - count);
                if (count >= limit) goto Finish;
                
                count += AddSignaturesToListView(_clamEng.DailyDatabase.LduSignatures, "Daily", limit - count);
                if (count >= limit) goto Finish;
                
                count += AddSignaturesToListView(_clamEng.DailyDatabase.FpSignatures, "Daily", limit - count);
                if (count >= limit) goto Finish;
                
                count += AddSignaturesToListView(_clamEng.DailyDatabase.CdbSignatures, "Daily", limit - count);
                if (count >= limit) goto Finish;

                // Load Main Database signatures
                count += AddSignaturesToListView(_clamEng.MainDatabase.HdbSignatures, "Main", limit - count);
                if (count >= limit) goto Finish;
                
                count += AddSignaturesToListView(_clamEng.MainDatabase.HsbSignatures, "Main", limit - count);
                if (count >= limit) goto Finish;
                
                count += AddSignaturesToListView(_clamEng.MainDatabase.MdbSignatures, "Main", limit - count);
                if (count >= limit) goto Finish;
                
                count += AddSignaturesToListView(_clamEng.MainDatabase.NdbSignatures, "Main", limit - count);
                if (count >= limit) goto Finish;
                
                count += AddSignaturesToListView(_clamEng.MainDatabase.LdbSignatures, "Main", limit - count);
                if (count >= limit) goto Finish;
                
                count += AddSignaturesToListView(_clamEng.MainDatabase.LduSignatures, "Main", limit - count);
                if (count >= limit) goto Finish;
                
                count += AddSignaturesToListView(_clamEng.MainDatabase.FpSignatures, "Main", limit - count);
                if (count >= limit) goto Finish;
                
                count += AddSignaturesToListView(_clamEng.MainDatabase.CdbSignatures, "Main", limit - count);

            Finish:
                AddLog($"Đã tải {count:N0} signatures vào ListView (giới hạn: {limit:N0})");
                
                if (_clamEng.TotalSignatures > limit)
                {
                    AddLog($"Chú ý: Chỉ hiển thị {limit:N0}/{_clamEng.TotalSignatures:N0} signatures để tránh UI lag");
                }
            }
            catch (Exception ex)
            {
                AddLog($"Lỗi khi load signatures: {ex.Message}");
            }
            finally
            {
                lvSignatures.EndUpdate();
            }
        }

        private int AddSignaturesToListView(Dictionary<string, ClamAVSignature> signatures, string database, int maxCount)
        {
            if (signatures == null || signatures.Count == 0 || maxCount <= 0)
                return 0;

            int added = 0;
            foreach (var kvp in signatures)
            {
                if (added >= maxCount)
                    break;

                var sig = kvp.Value;
                var item = new ListViewItem(kvp.Key ?? "N/A");

                item.SubItems.Add(sig.Name);

                // Type
                item.SubItems.Add(sig.Type.ToString());
                
                // Target
                item.SubItems.Add(sig.Target.ToString());
                
                // Database (Daily/Main)
                item.SubItems.Add(database);
                
                // Hash/Pattern - hiển thị khác nhau cho từng type
                string hashOrPattern = GetSignaturePattern(sig);
                item.SubItems.Add(hashOrPattern);
                
                // File Size
                string fileSize = GetSignatureFileSize(sig);
                item.SubItems.Add(fileSize);
                
                // Official
                item.SubItems.Add(sig.IsUnofficial ? "No" : "Yes");
                
                // Color code by type
                switch (sig.Type)
                {
                    case SignatureType.HDB:
                    case SignatureType.HSB:
                    case SignatureType.MDB:
                        item.BackColor = Color.LightBlue;
                        break;
                    case SignatureType.LDB:
                    case SignatureType.LDU:
                        item.BackColor = Color.LightYellow;
                        break;
                    case SignatureType.NDB:
                        item.BackColor = Color.LightGreen;
                        break;
                    case SignatureType.FP:
                        item.BackColor = Color.LightGray;
                        break;
                }
                
                lvSignatures.Items.Add(item);
                added++;
            }

            return added;
        }

        private string GetSignaturePattern(ClamAVSignature sig)
        {
            // Hash signatures (HDB, HSB, MDB, FP)
            if (sig.Properties.ContainsKey("Hash"))
            {
                string hash = sig.Properties["Hash"].ToString();
                if (hash.Length > 50)
                    return hash.Substring(0, 47) + "...";
                return hash;
            }

            // NDB - single hex pattern
            if (sig.Type == SignatureType.NDB && sig.Properties.ContainsKey("RawPattern"))
            {
                string pattern = sig.Properties["RawPattern"].ToString();
                if (pattern.Length > 50)
                    return pattern.Substring(0, 47) + "...";
                return pattern;
            }

            // LDB/LDU - multiple subsignatures
            if ((sig.Type == SignatureType.LDB || sig.Type == SignatureType.LDU))
            {
                // Show logical expression + subsig count
                string logicExpr = "N/A";
                int subsigCount = 0;

                if (sig.Properties.ContainsKey("LogicalExpression"))
                {
                    logicExpr = sig.Properties["LogicalExpression"].ToString();
                }

                if (sig.Properties.ContainsKey("PatternBytes"))
                {
                    var patterns = sig.Properties["PatternBytes"] as System.Collections.Generic.List<byte[]>;
                    if (patterns != null)
                        subsigCount = patterns.Count;
                }
                else if (sig.Properties.ContainsKey("RawPatterns"))
                {
                    var rawPatterns = sig.Properties["RawPatterns"] as System.Collections.Generic.List<string>;
                    if (rawPatterns != null)
                        subsigCount = rawPatterns.Count;
                }

                // Show first subsignature if available
                string firstSubsig = "";
                if (sig.Properties.ContainsKey("RawPatterns"))
                {
                    var rawPatterns = sig.Properties["RawPatterns"] as System.Collections.Generic.List<string>;
                    if (rawPatterns != null && rawPatterns.Count > 0)
                    {
                        firstSubsig = rawPatterns[0];
                        if (firstSubsig.Length > 30)
                            firstSubsig = firstSubsig.Substring(0, 27) + "...";
                    }
                }

                return $"Logic: {logicExpr} | {subsigCount} subsigs | First: {firstSubsig}";
            }

            // CDB - container signature
            if (sig.Type == SignatureType.CDB)
            {
                if (sig.Properties.ContainsKey("FileNamePattern"))
                {
                    return "File: " + sig.Properties["FileNamePattern"].ToString();
                }
            }

            return "N/A";
        }

        private string GetSignatureFileSize(ClamAVSignature sig)
        {
            // File size range (for LDB)
            if (sig.Properties.ContainsKey("FileSizeMin") && sig.Properties.ContainsKey("FileSizeMax"))
            {
                long min = (long)sig.Properties["FileSizeMin"];
                long max = (long)sig.Properties["FileSizeMax"];
                return $"{FormatFileSize(min)} - {FormatFileSize(max)}";
            }

            // Single file size
            if (sig.Properties.ContainsKey("FileSize"))
            {
                long size = (long)sig.Properties["FileSize"];
                if (size > 0)
                    return FormatFileSize(size);
            }

            // Wildcard
            if (sig.IsWildcard)
                return "*";

            return "Any";
        }

        private string FormatFileSize(long bytes)
        {
            if (bytes == 0) return "0 B";
            
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        private void btnBrowseDb_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Chọn thư mục chứa database ClamAV";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    txtDbPath.Text = dialog.SelectedPath;
                }
            }
        }

        private void btnBrowseFile_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "All Files (*.*)|*.*";
                dialog.Title = "Chọn file cần quét";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    txtScanPath.Text = dialog.FileName;
                }
            }
        }

        private void btnBrowseFolder_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Chọn thư mục cần quét";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    txtScanPath.Text = dialog.SelectedPath;
                }
            }
        }

        private void btnScan_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtScanPath.Text))
            {
                MessageBox.Show("Vui lòng chọn file/thư mục cần quét!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!_clamEng.IsDatabaseLoaded)
            {
                MessageBox.Show("Vui lòng tải database trước!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (isScanning)
            {
                MessageBox.Show("Đang quét, vui lòng đợi!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            isScanning = true;
            btnScan.Enabled = false;
            lvResults.Items.Clear();
            progressBar.Value = 0;
            lblProgress.Text = "";

            string path = txtScanPath.Text;

            // Start BackgroundWorker
            scanWorker.RunWorkerAsync(path);
        }

        private void ScanWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            string path = e.Argument as string;
            
            try
            {
                if (File.Exists(path))
                {
                    // Scan single file
                    scanWorker.ReportProgress(0, $"Quét file: {path}");
                    var result = _clamEng.ScanFile(path);
                    // Report result so ListView luôn nhận được kết quả
                    scanWorker.ReportProgress(100, result);
                    e.Result = new List<ClamAVResult> { result };
                }
                else if (Directory.Exists(path))
                {
                    // Scan folder
                    scanWorker.ReportProgress(0, $"Quét thư mục: {path}");
                    var results = ScanFolderWithWorker(path);
                    e.Result = results;
                }
                else
                {
                    e.Result = new Exception("File/thư mục không tồn tại!");
                }
            }
            catch (Exception ex)
            {
                e.Result = ex;
            }
        }

        private void ScanWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState is string message)
            {
                AddLog(message);
            }
            else if (e.UserState is ClamAVResult result)
            {
                DisplayScanResult(result);
            }
            
            progressBar.Value = e.ProgressPercentage;
            if (e.ProgressPercentage > 0)
            {
                lblProgress.Text = $"Đang quét: {e.ProgressPercentage}%";
            }
        }

        private void ScanWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            isScanning = false;
            btnScan.Enabled = true;
            progressBar.Value = 0;
            lblProgress.Text = "";

            if (e.Result is Exception ex)
            {
                AddLog($"LỖI: {ex.Message}");
                MessageBox.Show($"Lỗi khi quét: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (e.Result is List<ClamAVResult> results)
            {
                AddLog("Quét hoàn tất!");
                MessageBox.Show("Quét hoàn tất!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private List<ClamAVResult> ScanFolderWithWorker(string folderPath)
        {
            var results = new List<ClamAVResult>();
            var files = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories);
            int total = files.Length;
            int scanned = 0;

            foreach (var file in files)
            {
                try
                {
                    scanned++;
                    int progress = (int)((scanned * 100.0) / total);
                    
                    // Scan file
                    var result = _clamEng.ScanFile(file);
                    results.Add(result);
                    
                    // Report progress
                    scanWorker.ReportProgress(progress, result);
                }
                catch (Exception ex)
                {
                    scanWorker.ReportProgress(0, $"Lỗi khi quét {file}: {ex.Message}");
                }
            }

            return results;
        }

        private void DisplayScanResult(ClamAVResult result)
        {
            if (result == null) return;

            var item = new ListViewItem(result.FilePath);
            
            string status = "";
            Color statusColor = Color.Black;
            
            switch (result.Status)
            {
                case ScanStatus.Clean:
                    status = "Sạch";
                    statusColor = Color.Green;
                    break;
                case ScanStatus.Infected:
                    status = "Nhiễm virus";
                    statusColor = Color.Red;
                    break;
                case ScanStatus.Whitelisted:
                    status = "Whitelist";
                    statusColor = Color.Blue;
                    break;
                case ScanStatus.Error:
                    status = "Lỗi";
                    statusColor = Color.Orange;
                    break;
            }
            
            item.SubItems.Add(status);
            item.SubItems.Add(result.VirusName ?? "");
            item.ForeColor = statusColor;
            
            lvResults.Items.Add(item);
            
            if (result.Status == ScanStatus.Infected)
            {
                AddLog($"[INFECTED] {result.FilePath} - {result.VirusName}");
            }
        }

        private void AddLog(string message)
        {
            if (txtLog.InvokeRequired)
            {
                txtLog.Invoke(new Action(() => AddLog(message)));
                return;
            }
            
            txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\r\n");
            txtLog.SelectionStart = txtLog.Text.Length;
            txtLog.ScrollToCaret();
        }

        private void btnClearLog_Click(object sender, EventArgs e)
        {
            txtLog.Clear();
        }
    }
}
