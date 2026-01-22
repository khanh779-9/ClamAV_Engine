using ClamAV_Engine.ClamLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClamAV_Engine
{
    public partial class Form2 : Form
    {
        private AhoCorasickEngine _acEngine;
        private bool _isScanning = false;
        private string _selectedFilePath;
        private string _databaseFolder;
        private System.Windows.Forms.ListBox _logListBox = null;
        private CancellationTokenSource _cancellationTokenSource = null;

        public Form2()
        {
            InitializeComponent();
            
            // Thêm button show logs
            var btnShowLogs = new System.Windows.Forms.Button
            {
                Text = "📋 Logs",
                Location = new System.Drawing.Point(630, 12),
                Size = new System.Drawing.Size(70, 28),
                TabIndex = 0
            };
            btnShowLogs.Click += BtnShowLogs_Click;
            this.Controls.Add(btnShowLogs);
        }

        private void BtnShowLogs_Click(object sender, EventArgs e)
        {
            if (_logListBox != null && !_logListBox.IsDisposed)
            {
                _logListBox.FindForm()?.Close();
                _logListBox = null;
                return;
            }

            var logForm = new System.Windows.Forms.Form
            {
                Text = "Debug Logs - AhoCorasickEngine",
                Width = 900,
                Height = 600,
                StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen,
                FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow
            };

            _logListBox = new System.Windows.Forms.ListBox
            {
                Dock = System.Windows.Forms.DockStyle.Fill,
                Font = new System.Drawing.Font("Courier New", 9)
            };

            logForm.Controls.Add(_logListBox);
            logForm.Show();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            // UI đã được khởi tạo trong Designer
            this.FormClosing += Form2_FormClosing;
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Cancel tất cả background tasks khi đóng form
            if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
            }
        }

        private void BrowseDatabase_Click(object sender, EventArgs e)
        {
            var dlg = new FolderBrowserDialog { Description = "Select ClamAV database folder (chứa Daily + Main)" };
            if (dlg.ShowDialog() != DialogResult.OK) return;

            _databaseFolder = dlg.SelectedPath;
            txtDbPath.Text = _databaseFolder;
            lblDbFolder.Text = Path.GetFileName(_databaseFolder);
        }

        private void LoadDatabase_Click(object sender, EventArgs e)
        {
            _databaseFolder = txtDbPath.Text;
            
            if (string.IsNullOrWhiteSpace(_databaseFolder) || !Directory.Exists(_databaseFolder))
            {
                MessageBox.Show("Please enter or browse a valid database folder path.", "Invalid Path", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            lblDbFolder.Text = Path.GetFileName(_databaseFolder);

            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;

            Task.Run(() =>
            {
                try
                {
                    Invoke((MethodInvoker)(() =>
                    {
                        lblDbStatus.Text = "🔄 Loading database...";
                        lblDbStatus.ForeColor = System.Drawing.Color.Orange;
                        btnScan.Enabled = false;
                    }));

                    // Tạo engine theo loại chữ ký người dùng chọn
                    var selectedTypes = new List<SignatureType>();
                    if (chkHDB.Checked) selectedTypes.Add(SignatureType.HDB);
                    if (chkNDB.Checked) selectedTypes.Add(SignatureType.NDB);
                    if (chkHSB.Checked) selectedTypes.Add(SignatureType.HSB);
                    if (chkMDB.Checked) selectedTypes.Add(SignatureType.MDB);
                    if (chkLDU.Checked) selectedTypes.Add(SignatureType.LDU);
                    if (chkLDB.Checked) selectedTypes.Add(SignatureType.LDB);
                    if (chkFP.Checked)  selectedTypes.Add(SignatureType.FP);
                    if (chkCDB.Checked) selectedTypes.Add(SignatureType.CDB);

                    _acEngine = new AhoCorasickEngine(selectedTypes)
                    { 
                        Logger = LogMessage 
                    };
                    
                    cancellationToken.ThrowIfCancellationRequested();

                    if (!_acEngine.LoadDatabaseFolder(_databaseFolder))
                    {
                        Invoke((MethodInvoker)(() =>
                        {
                            lblDbStatus.Text = "❌ Failed to load database";
                            lblDbStatus.ForeColor = System.Drawing.Color.Red;
                        }));
                        return;
                    }

                    cancellationToken.ThrowIfCancellationRequested();

                    Invoke((MethodInvoker)(() =>
                    {
                        lblDbStatus.Text = "🔄 Building AC Trie...";
                        lblDbStatus.ForeColor = System.Drawing.Color.Orange;
                    }));

                    bool built = _acEngine.BuildPatterns();
                    if (!built)
                    {
                        Invoke((MethodInvoker)(() =>
                        {
                            lblDbStatus.Text = "❌ Failed to build AC patterns";
                            lblDbStatus.ForeColor = System.Drawing.Color.Red;
                        }));
                        return;
                    }

                    cancellationToken.ThrowIfCancellationRequested();

                    Invoke((MethodInvoker)(() =>
                    {
                        lblDbStatus.Text = $"✓ Loaded {_acEngine.LoadedPatternCount} AC patterns | Daily: {_acEngine.DailyDatabase.TotalSignatures} | Main: {_acEngine.MainDatabase.TotalSignatures} | NDB: {_acEngine.DailyDatabase.NdbSignatures.Count + _acEngine.MainDatabase.NdbSignatures.Count} | LDB: {_acEngine.DailyDatabase.LdbSignatures.Count + _acEngine.MainDatabase.LdbSignatures.Count} | LDU: {_acEngine.DailyDatabase.LduSignatures.Count + _acEngine.MainDatabase.LduSignatures.Count} | HDB: {_acEngine.DailyDatabase.HdbSignatures.Count + _acEngine.MainDatabase.HdbSignatures.Count}";
                        lblDbStatus.ForeColor = System.Drawing.Color.Green;
                        btnScan.Enabled = true;
                    }));
                }
                catch (OperationCanceledException)
                {
                    Invoke((MethodInvoker)(() =>
                    {
                        lblDbStatus.Text = "⊘ Database load cancelled";
                        lblDbStatus.ForeColor = System.Drawing.Color.Orange;
                    }));
                }
                catch (Exception ex)
                {
                    Invoke((MethodInvoker)(() =>
                    {
                        lblDbStatus.Text = $"❌ Error: {ex.Message}";
                        lblDbStatus.ForeColor = System.Drawing.Color.Red;
                    }));
                }
            }, cancellationToken);
        }

        private void LogMessage(string msg)
        {
            try
            {
                // Batch log messages - only invoke every N messages or critical ones
                if (_logListBox != null && !_logListBox.IsDisposed && 
                    (msg.Contains("DETECTED") || msg.Contains("CLEAN") || msg.Contains("ERROR") || 
                     DateTime.UtcNow.Millisecond % 5 == 0)) // Sample 20% of debug messages
                {
                    Invoke((MethodInvoker)(() =>
                    {
                        if (_logListBox != null && !_logListBox.IsDisposed)
                        {
                            _logListBox.Items.Add($"[{DateTime.Now:HH:mm:ss.fff}] {msg}");
                            _logListBox.TopIndex = Math.Max(0, _logListBox.Items.Count - 1);
                        }
                    }));
                }
            }
            catch { }
        }

        private void BrowseFile_Click(object sender, EventArgs e)
        {
            var dlg = new OpenFileDialog { Title = "Select file to scan" };
            if (dlg.ShowDialog() != DialogResult.OK) return;

            _selectedFilePath = dlg.FileName;
            var fileInfo = new FileInfo(_selectedFilePath);
            lblFileName.Text = Path.GetFileName(_selectedFilePath);
            lblFileInfo.Text = $"Size: {FormatBytes(fileInfo.Length)} | Modified: {fileInfo.LastWriteTime:yyyy-MM-dd HH:mm:ss}";
            dgvResults.Rows.Clear();
            prgProgress.Value = 0;
            lblScanTime.Text = "";
        }

        private void StartScan_Click(object sender, EventArgs e)
        {
            if (_acEngine == null || _acEngine.LoadedPatternCount <= 0)
            {
                MessageBox.Show("Load database first!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrEmpty(_selectedFilePath))
            {
                MessageBox.Show("Select file to scan first!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _isScanning = true;
            btnScan.Enabled = false;
            btnBrowse.Enabled = false;
            btnLoadDb.Enabled = false;
            dgvResults.Rows.Clear();
            prgProgress.Value = 0;

            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;

            Task.Run(() =>
            {
                try
                {
                    var sw = Stopwatch.StartNew();

                    Invoke((MethodInvoker)(() => prgProgress.Value = 30));

                    cancellationToken.ThrowIfCancellationRequested();

                    // Scan file dùng AhoCorasickEngine
                    var result = _acEngine.ScanFile(_selectedFilePath);

                    cancellationToken.ThrowIfCancellationRequested();

                    sw.Stop();

                    Invoke((MethodInvoker)(() =>
                    {
                        prgProgress.Value = 100;
                        DisplayResults(result, sw.ElapsedMilliseconds);
                    }));
                }
                catch (OperationCanceledException)
                {
                    Invoke((MethodInvoker)(() =>
                    {
                        MessageBox.Show("Scan cancelled", "Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        prgProgress.Value = 0;
                    }));
                }
                catch (Exception ex)
                {
                    Invoke((MethodInvoker)(() =>
                    {
                        MessageBox.Show($"Scan error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        prgProgress.Value = 0;
                    }));
                }
                finally
                {
                    _isScanning = false;
                    Invoke((MethodInvoker)(() =>
                    {
                        btnScan.Enabled = true;
                        btnBrowse.Enabled = true;
                        btnLoadDb.Enabled = true;
                    }));
                }
            }, cancellationToken);
        }

        private void DisplayResults(ClamAVResult result, long timeMs)
        {
            dgvResults.Rows.Clear();

            // Dòng 1: Thông tin file
            dgvResults.Rows.Add(
                1,
                "FILE",
                Path.GetFileName(result.FilePath),
                $"Size: {FormatBytes(result.FileSize)}"
            );

            // Dòng 2: Status
            string statusStr;
            System.Drawing.Color statusColor;
            
            switch (result.Status)
            {
                case ScanStatus.Clean:
                    statusStr = "✓ CLEAN";
                    statusColor = System.Drawing.Color.Green;
                    break;
                case ScanStatus.Infected:
                    statusStr = "⚠ INFECTED";
                    statusColor = System.Drawing.Color.Red;
                    break;
                case ScanStatus.Whitelisted:
                    statusStr = "✓ WHITELISTED";
                    statusColor = System.Drawing.Color.Blue;
                    break;
                case ScanStatus.Error:
                    statusStr = "✗ ERROR";
                    statusColor = System.Drawing.Color.DarkRed;
                    break;
                default:
                    statusStr = result.Status.ToString();
                    statusColor = System.Drawing.Color.Gray;
                    break;
            }

            var statusRow = dgvResults.Rows.Add(
                2,
                "STATUS",
                statusStr,
                $"Scanned in {timeMs}ms"
            );
            dgvResults.Rows[statusRow].DefaultCellStyle.ForeColor = statusColor;
            dgvResults.Rows[statusRow].DefaultCellStyle.Font = new System.Drawing.Font("Arial", 9, System.Drawing.FontStyle.Bold);

            // Các detection hoặc whitelist info
            if (!string.IsNullOrEmpty(result.VirusName))
            {
                var detections = result.VirusName.Split(',').Select(x => x.Trim()).ToList();
                int cntHDB = 0, cntNDB = 0, cntLDU = 0, cntLDB = 0, cntHSB = 0, cntMDB = 0, cntFP = 0, cntCDB = 0;
                for (int i = 0; i < detections.Count; i++)
                {
                    string typeStr = "UNKNOWN";
                    if (detections[i].Contains(":"))
                    {
                        var parts = detections[i].Split(':');
                        if (parts.Length > 1)
                            typeStr = parts[parts.Length - 1];
                    }
                    else
                    {
                        typeStr = "NDB"; // Default
                    }

                    switch (typeStr.ToUpperInvariant())
                    {
                        case "HDB": cntHDB++; break;
                        case "NDB": cntNDB++; break;
                        case "LDU": cntLDU++; break;
                        case "LDB": cntLDB++; break;
                        case "HSB": cntHSB++; break;
                        case "MDB": cntMDB++; break;
                        case "FP":  cntFP++;  break;
                        case "CDB": cntCDB++; break;
                    }

                    var detRow = dgvResults.Rows.Add(
                        3 + i,
                        result.Status == ScanStatus.Whitelisted ? "FP" : typeStr,
                        detections[i],
                        result.Status == ScanStatus.Whitelisted ? "Whitelisted" : $"Detection {i + 1}/{detections.Count}"
                    );
                    
                    if (result.Status == ScanStatus.Whitelisted)
                    {
                        dgvResults.Rows[detRow].DefaultCellStyle.BackColor = System.Drawing.Color.LightBlue;
                    }
                    else if (result.Status == ScanStatus.Infected)
                    {
                        dgvResults.Rows[detRow].DefaultCellStyle.BackColor = System.Drawing.Color.LightYellow;
                    }
                }
            }

            // Tóm tắt theo loại
            if (!string.IsNullOrEmpty(result.VirusName))
            {
                var summary = new StringBuilder();
                summary.Append("HDB:" + dgvResults.Rows.Cast<DataGridViewRow>().Count(r => (string)r.Cells[1].Value == "HDB"));
                summary.Append(" | NDB:" + dgvResults.Rows.Cast<DataGridViewRow>().Count(r => (string)r.Cells[1].Value == "NDB"));
                summary.Append(" | LDU:" + dgvResults.Rows.Cast<DataGridViewRow>().Count(r => (string)r.Cells[1].Value == "LDU"));
                summary.Append(" | LDB:" + dgvResults.Rows.Cast<DataGridViewRow>().Count(r => (string)r.Cells[1].Value == "LDB"));
                summary.Append(" | HSB:" + dgvResults.Rows.Cast<DataGridViewRow>().Count(r => (string)r.Cells[1].Value == "HSB"));
                summary.Append(" | MDB:" + dgvResults.Rows.Cast<DataGridViewRow>().Count(r => (string)r.Cells[1].Value == "MDB"));
                summary.Append(" | FP:"  + dgvResults.Rows.Cast<DataGridViewRow>().Count(r => (string)r.Cells[1].Value == "FP"));
                summary.Append(" | CDB:" + dgvResults.Rows.Cast<DataGridViewRow>().Count(r => (string)r.Cells[1].Value == "CDB"));

                var sumRow = dgvResults.Rows.Add(9998, "SUMMARY", "Detections by type", summary.ToString());
                dgvResults.Rows[sumRow].DefaultCellStyle.BackColor = System.Drawing.Color.LightGray;
                dgvResults.Rows[sumRow].DefaultCellStyle.Font = new System.Drawing.Font("Arial", 8, System.Drawing.FontStyle.Italic);
            }

            // Error message nếu có
            if (!string.IsNullOrEmpty(result.ErrorMessage))
            {
                var errRow = dgvResults.Rows.Add(
                    999,
                    "ERROR",
                    result.ErrorMessage,
                    "File read error"
                );
                dgvResults.Rows[errRow].DefaultCellStyle.ForeColor = System.Drawing.Color.Red;
            }

            // Thêm MD5/SHA256 nếu có
            if (!string.IsNullOrEmpty(result.MD5))
            {
                dgvResults.Rows.Add(
                    10,
                    "MD5",
                    result.MD5,
                    ""
                );
            }

            if (!string.IsNullOrEmpty(result.SHA256))
            {
                dgvResults.Rows.Add(
                    11,
                    "SHA256",
                    result.SHA256.Substring(0, Math.Min(40, result.SHA256.Length)) + "...",
                    ""
                );
            }

            lblScanTime.Text = $"Scan time: {timeMs}ms";
        }

        private string FormatBytes(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F2} KB";
            if (bytes < 1024 * 1024 * 1024) return $"{bytes / (1024.0 * 1024):F2} MB";
            return $"{bytes / (1024.0 * 1024 * 1024):F2} GB";
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            dgvResults.Rows.Clear();
            prgProgress.Value = 0;
            lblScanTime.Text = "";
        }
    }
}
