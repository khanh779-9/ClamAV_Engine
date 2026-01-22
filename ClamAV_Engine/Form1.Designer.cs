namespace ClamAV_Engine
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lblDbStatus = new System.Windows.Forms.Label();
            this.btnLoadDb = new System.Windows.Forms.Button();
            this.btnBrowseDb = new System.Windows.Forms.Button();
            this.txtDbPath = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.lblProgress = new System.Windows.Forms.Label();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.btnScan = new System.Windows.Forms.Button();
            this.btnBrowseFolder = new System.Windows.Forms.Button();
            this.btnBrowseFile = new System.Windows.Forms.Button();
            this.txtScanPath = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPageResults = new System.Windows.Forms.TabPage();
            this.lvResults = new System.Windows.Forms.ListView();
            this.colFile = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colStatus = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colThreat = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tabPageSignatures = new System.Windows.Forms.TabPage();
            this.lvSignatures = new System.Windows.Forms.ListView();
            this.colSigName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colSigType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colSigTarget = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colSigDatabase = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colSigHash = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colSigFileSize = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colSigOfficial = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.btnClearLog = new System.Windows.Forms.Button();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.col_KeySig = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPageResults.SuspendLayout();
            this.tabPageSignatures.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.lblDbStatus);
            this.groupBox1.Controls.Add(this.btnLoadDb);
            this.groupBox1.Controls.Add(this.btnBrowseDb);
            this.groupBox1.Controls.Add(this.txtDbPath);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(1160, 80);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Cơ sở dữ liệu Virus (ClamAV Database)";
            // 
            // lblDbStatus
            // 
            this.lblDbStatus.AutoSize = true;
            this.lblDbStatus.ForeColor = System.Drawing.Color.Gray;
            this.lblDbStatus.Location = new System.Drawing.Point(100, 55);
            this.lblDbStatus.Name = "lblDbStatus";
            this.lblDbStatus.Size = new System.Drawing.Size(93, 13);
            this.lblDbStatus.TabIndex = 4;
            this.lblDbStatus.Text = "Chưa tải database";
            // 
            // btnLoadDb
            // 
            this.btnLoadDb.Location = new System.Drawing.Point(1060, 26);
            this.btnLoadDb.Name = "btnLoadDb";
            this.btnLoadDb.Size = new System.Drawing.Size(85, 23);
            this.btnLoadDb.TabIndex = 3;
            this.btnLoadDb.Text = "Tải Database";
            this.btnLoadDb.UseVisualStyleBackColor = true;
            this.btnLoadDb.Click += new System.EventHandler(this.btnLoadDb_Click);
            // 
            // btnBrowseDb
            // 
            this.btnBrowseDb.Location = new System.Drawing.Point(970, 26);
            this.btnBrowseDb.Name = "btnBrowseDb";
            this.btnBrowseDb.Size = new System.Drawing.Size(85, 23);
            this.btnBrowseDb.TabIndex = 2;
            this.btnBrowseDb.Text = "Chọn thư mục";
            this.btnBrowseDb.UseVisualStyleBackColor = true;
            this.btnBrowseDb.Click += new System.EventHandler(this.btnBrowseDb_Click);
            // 
            // txtDbPath
            // 
            this.txtDbPath.Location = new System.Drawing.Point(100, 28);
            this.txtDbPath.Name = "txtDbPath";
            this.txtDbPath.Size = new System.Drawing.Size(865, 20);
            this.txtDbPath.TabIndex = 1;
            this.txtDbPath.Text = "D:\\Data\\Tailieu\\Projects\\C#\\ClamAV_Engine\\ClamAV_Engine\\bin\\Debug\\clamdb\\daily\\";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 31);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(81, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Đường dẫn DB:";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.lblProgress);
            this.groupBox2.Controls.Add(this.progressBar);
            this.groupBox2.Controls.Add(this.btnScan);
            this.groupBox2.Controls.Add(this.btnBrowseFolder);
            this.groupBox2.Controls.Add(this.btnBrowseFile);
            this.groupBox2.Controls.Add(this.txtScanPath);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Location = new System.Drawing.Point(12, 98);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(1160, 100);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Quét File/Thư mục";
            // 
            // lblProgress
            // 
            this.lblProgress.AutoSize = true;
            this.lblProgress.Location = new System.Drawing.Point(100, 78);
            this.lblProgress.Name = "lblProgress";
            this.lblProgress.Size = new System.Drawing.Size(0, 13);
            this.lblProgress.TabIndex = 8;
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(100, 55);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(865, 20);
            this.progressBar.TabIndex = 7;
            // 
            // btnScan
            // 
            this.btnScan.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnScan.Location = new System.Drawing.Point(970, 55);
            this.btnScan.Name = "btnScan";
            this.btnScan.Size = new System.Drawing.Size(175, 30);
            this.btnScan.TabIndex = 6;
            this.btnScan.Text = "Bắt đầu quét";
            this.btnScan.UseVisualStyleBackColor = true;
            this.btnScan.Click += new System.EventHandler(this.btnScan_Click);
            // 
            // btnBrowseFolder
            // 
            this.btnBrowseFolder.Location = new System.Drawing.Point(1060, 26);
            this.btnBrowseFolder.Name = "btnBrowseFolder";
            this.btnBrowseFolder.Size = new System.Drawing.Size(85, 23);
            this.btnBrowseFolder.TabIndex = 5;
            this.btnBrowseFolder.Text = "Chọn thư mục";
            this.btnBrowseFolder.UseVisualStyleBackColor = true;
            this.btnBrowseFolder.Click += new System.EventHandler(this.btnBrowseFolder_Click);
            // 
            // btnBrowseFile
            // 
            this.btnBrowseFile.Location = new System.Drawing.Point(970, 26);
            this.btnBrowseFile.Name = "btnBrowseFile";
            this.btnBrowseFile.Size = new System.Drawing.Size(85, 23);
            this.btnBrowseFile.TabIndex = 4;
            this.btnBrowseFile.Text = "Chọn file";
            this.btnBrowseFile.UseVisualStyleBackColor = true;
            this.btnBrowseFile.Click += new System.EventHandler(this.btnBrowseFile_Click);
            // 
            // txtScanPath
            // 
            this.txtScanPath.Location = new System.Drawing.Point(100, 28);
            this.txtScanPath.Name = "txtScanPath";
            this.txtScanPath.Size = new System.Drawing.Size(865, 20);
            this.txtScanPath.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 31);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(73, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "File/Thư mục:";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPageResults);
            this.tabControl1.Controls.Add(this.tabPageSignatures);
            this.tabControl1.Location = new System.Drawing.Point(12, 204);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1160, 280);
            this.tabControl1.TabIndex = 2;
            // 
            // tabPageResults
            // 
            this.tabPageResults.Controls.Add(this.lvResults);
            this.tabPageResults.Location = new System.Drawing.Point(4, 22);
            this.tabPageResults.Name = "tabPageResults";
            this.tabPageResults.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageResults.Size = new System.Drawing.Size(1152, 254);
            this.tabPageResults.TabIndex = 0;
            this.tabPageResults.Text = "Kết quả quét";
            this.tabPageResults.UseVisualStyleBackColor = true;
            // 
            // lvResults
            // 
            this.lvResults.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colFile,
            this.colStatus,
            this.colThreat});
            this.lvResults.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvResults.FullRowSelect = true;
            this.lvResults.GridLines = true;
            this.lvResults.HideSelection = false;
            this.lvResults.Location = new System.Drawing.Point(3, 3);
            this.lvResults.Name = "lvResults";
            this.lvResults.Size = new System.Drawing.Size(1146, 248);
            this.lvResults.TabIndex = 0;
            this.lvResults.UseCompatibleStateImageBehavior = false;
            this.lvResults.View = System.Windows.Forms.View.Details;
            // 
            // colFile
            // 
            this.colFile.Text = "File";
            this.colFile.Width = 500;
            // 
            // colStatus
            // 
            this.colStatus.Text = "Trạng thái";
            this.colStatus.Width = 150;
            // 
            // colThreat
            // 
            this.colThreat.Text = "Mối đe dọa";
            this.colThreat.Width = 400;
            // 
            // tabPageSignatures
            // 
            this.tabPageSignatures.Controls.Add(this.lvSignatures);
            this.tabPageSignatures.Location = new System.Drawing.Point(4, 22);
            this.tabPageSignatures.Name = "tabPageSignatures";
            this.tabPageSignatures.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageSignatures.Size = new System.Drawing.Size(1152, 254);
            this.tabPageSignatures.TabIndex = 1;
            this.tabPageSignatures.Text = "Signatures đã tải";
            this.tabPageSignatures.UseVisualStyleBackColor = true;
            // 
            // lvSignatures
            // 
            this.lvSignatures.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.col_KeySig,
            this.colSigName,
            this.colSigType,
            this.colSigTarget,
            this.colSigDatabase,
            this.colSigHash,
            this.colSigFileSize,
            this.colSigOfficial});
            this.lvSignatures.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvSignatures.FullRowSelect = true;
            this.lvSignatures.GridLines = true;
            this.lvSignatures.HideSelection = false;
            this.lvSignatures.Location = new System.Drawing.Point(3, 3);
            this.lvSignatures.Name = "lvSignatures";
            this.lvSignatures.Size = new System.Drawing.Size(1146, 248);
            this.lvSignatures.TabIndex = 0;
            this.lvSignatures.UseCompatibleStateImageBehavior = false;
            this.lvSignatures.View = System.Windows.Forms.View.Details;
            // 
            // colSigName
            // 
            this.colSigName.Text = "Signature Name";
            this.colSigName.Width = 220;
            // 
            // colSigType
            // 
            this.colSigType.DisplayIndex = 1;
            this.colSigType.Text = "Type";
            this.colSigType.Width = 80;
            // 
            // colSigTarget
            // 
            this.colSigTarget.DisplayIndex = 2;
            this.colSigTarget.Text = "Target";
            this.colSigTarget.Width = 100;
            // 
            // colSigDatabase
            // 
            this.colSigDatabase.DisplayIndex = 3;
            this.colSigDatabase.Text = "Database";
            this.colSigDatabase.Width = 100;
            // 
            // colSigHash
            // 
            this.colSigHash.Text = "Hash/ Pattern";
            this.colSigHash.Width = 280;
            // 
            // colSigFileSize
            // 
            this.colSigFileSize.DisplayIndex = 5;
            this.colSigFileSize.Text = "File Size";
            this.colSigFileSize.Width = 100;
            // 
            // colSigOfficial
            // 
            this.colSigOfficial.DisplayIndex = 6;
            this.colSigOfficial.Text = "Official";
            this.colSigOfficial.Width = 80;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.btnClearLog);
            this.groupBox4.Controls.Add(this.txtLog);
            this.groupBox4.Location = new System.Drawing.Point(12, 490);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(1160, 180);
            this.groupBox4.TabIndex = 3;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Log";
            // 
            // btnClearLog
            // 
            this.btnClearLog.Location = new System.Drawing.Point(1060, 19);
            this.btnClearLog.Name = "btnClearLog";
            this.btnClearLog.Size = new System.Drawing.Size(85, 23);
            this.btnClearLog.TabIndex = 1;
            this.btnClearLog.Text = "Xóa Log";
            this.btnClearLog.UseVisualStyleBackColor = true;
            this.btnClearLog.Click += new System.EventHandler(this.btnClearLog_Click);
            // 
            // txtLog
            // 
            this.txtLog.BackColor = System.Drawing.Color.Black;
            this.txtLog.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtLog.ForeColor = System.Drawing.Color.Lime;
            this.txtLog.Location = new System.Drawing.Point(10, 19);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(1044, 150);
            this.txtLog.TabIndex = 0;
            // 
            // col_KeySig
            // 
            this.col_KeySig.Text = "Signatue Key";
            this.col_KeySig.Width = 100;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1184, 681);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ClamAV Engine Scanner";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPageResults.ResumeLayout(false);
            this.tabPageSignatures.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label lblDbStatus;
        private System.Windows.Forms.Button btnLoadDb;
        private System.Windows.Forms.Button btnBrowseDb;
        private System.Windows.Forms.TextBox txtDbPath;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label lblProgress;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Button btnScan;
        private System.Windows.Forms.Button btnBrowseFolder;
        private System.Windows.Forms.Button btnBrowseFile;
        private System.Windows.Forms.TextBox txtScanPath;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPageResults;
        private System.Windows.Forms.ListView lvResults;
        private System.Windows.Forms.ColumnHeader colFile;
        private System.Windows.Forms.ColumnHeader colStatus;
        private System.Windows.Forms.ColumnHeader colThreat;
        private System.Windows.Forms.TabPage tabPageSignatures;
        private System.Windows.Forms.ListView lvSignatures;
        private System.Windows.Forms.ColumnHeader colSigName;
        private System.Windows.Forms.ColumnHeader colSigType;
        private System.Windows.Forms.ColumnHeader colSigTarget;
        private System.Windows.Forms.ColumnHeader colSigDatabase;
        private System.Windows.Forms.ColumnHeader colSigHash;
        private System.Windows.Forms.ColumnHeader colSigFileSize;
        private System.Windows.Forms.ColumnHeader colSigOfficial;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Button btnClearLog;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.ColumnHeader col_KeySig;
    }
}

