using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace ClamAV_Engine
{
    partial class Form2
    {
        private IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private Label lblDbStatus;
        private Label lblDbFolder;
        private TextBox txtDbPath;
        private Label lblDbPath;
        private Label lblSignatureTypes;
        private Button btnBrowseDb;
        private Button btnLoadDb;
        private Button btnBrowse;
        private Button btnScan;
        private Button btnClear;
        private Label lblFileName;
        private Label lblFileInfo;
        private Label lblScanTime;
        private ProgressBar prgProgress;
        private DataGridView dgvResults;
        private Panel dbLoadPanel;
        private Panel dbStatusPanel;
        private Panel scanPanel;
        private Panel resultPanel;
        private Panel typesPanel;
        private CheckBox chkHDB;
        private CheckBox chkNDB;
        private CheckBox chkHSB;
        private CheckBox chkMDB;
        private CheckBox chkLDU;
        private CheckBox chkLDB;
        private CheckBox chkFP;
        private CheckBox chkCDB;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem exitToolStripMenuItem;

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dbLoadPanel = new System.Windows.Forms.Panel();
            this.txtDbPath = new System.Windows.Forms.TextBox();
            this.lblDbPath = new System.Windows.Forms.Label();
            this.lblSignatureTypes = new System.Windows.Forms.Label();
            this.btnBrowseDb = new System.Windows.Forms.Button();
            this.lblDbFolder = new System.Windows.Forms.Label();
            this.btnLoadDb = new System.Windows.Forms.Button();
            this.typesPanel = new System.Windows.Forms.Panel();
            this.chkHDB = new System.Windows.Forms.CheckBox();
            this.chkNDB = new System.Windows.Forms.CheckBox();
            this.chkHSB = new System.Windows.Forms.CheckBox();
            this.chkMDB = new System.Windows.Forms.CheckBox();
            this.chkLDU = new System.Windows.Forms.CheckBox();
            this.chkLDB = new System.Windows.Forms.CheckBox();
            this.chkFP = new System.Windows.Forms.CheckBox();
            this.chkCDB = new System.Windows.Forms.CheckBox();
            this.dbStatusPanel = new System.Windows.Forms.Panel();
            this.lblDbStatus = new System.Windows.Forms.Label();
            this.scanPanel = new System.Windows.Forms.Panel();
            this.lblFileInfo = new System.Windows.Forms.Label();
            this.lblScanTime = new System.Windows.Forms.Label();
            this.prgProgress = new System.Windows.Forms.ProgressBar();
            this.lblFileName = new System.Windows.Forms.Label();
            this.btnClear = new System.Windows.Forms.Button();
            this.btnScan = new System.Windows.Forms.Button();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.resultPanel = new System.Windows.Forms.Panel();
            this.dgvResults = new System.Windows.Forms.DataGridView();
            this.colIndex = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colInfo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.menuStrip1.SuspendLayout();
            this.dbLoadPanel.SuspendLayout();
            this.typesPanel.SuspendLayout();
            this.dbStatusPanel.SuspendLayout();
            this.scanPanel.SuspendLayout();
            this.resultPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvResults)).BeginInit();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1100, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(92, 22);
            this.exitToolStripMenuItem.Text = "E&xit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // lblSignatureTypes
            // 
            this.lblSignatureTypes.AutoSize = true;
            this.lblSignatureTypes.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold);
            this.lblSignatureTypes.Location = new System.Drawing.Point(8, 38);
            this.lblSignatureTypes.Name = "lblSignatureTypes";
            this.lblSignatureTypes.Size = new System.Drawing.Size(100, 15);
            this.lblSignatureTypes.TabIndex = 5;
            this.lblSignatureTypes.Text = "Signature Types:";
            // 
            // typesPanel
            // 
            this.typesPanel.Location = new System.Drawing.Point(110, 43);
            this.typesPanel.Name = "typesPanel";
            this.typesPanel.Size = new System.Drawing.Size(980, 30);
            this.typesPanel.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.typesPanel.Controls.Add(this.chkHDB);
            this.typesPanel.Controls.Add(this.chkNDB);
            this.typesPanel.Controls.Add(this.chkHSB);
            this.typesPanel.Controls.Add(this.chkMDB);
            this.typesPanel.Controls.Add(this.chkLDU);
            this.typesPanel.Controls.Add(this.chkLDB);
            this.typesPanel.Controls.Add(this.chkFP);
            this.typesPanel.Controls.Add(this.chkCDB);
            // 
            // chkHDB
            // 
            this.chkHDB.Text = "☑ HDB";
            this.chkHDB.Checked = true;
            this.chkHDB.Location = new System.Drawing.Point(10, 5);
            this.chkHDB.Size = new System.Drawing.Size(70, 20);
            this.chkHDB.AutoSize = true;
            // 
            // chkNDB
            // 
            this.chkNDB.Text = "☑ NDB";
            this.chkNDB.Checked = true;
            this.chkNDB.Location = new System.Drawing.Point(100, 5);
            this.chkNDB.Size = new System.Drawing.Size(70, 20);
            this.chkNDB.AutoSize = true;
            // 
            // chkHSB
            // 
            this.chkHSB.Text = "☑ HSB";
            this.chkHSB.Checked = true;
            this.chkHSB.Location = new System.Drawing.Point(190, 5);
            this.chkHSB.Size = new System.Drawing.Size(70, 20);
            this.chkHSB.AutoSize = true;
            // 
            // chkMDB
            // 
            this.chkMDB.Text = "☑ MDB";
            this.chkMDB.Checked = true;
            this.chkMDB.Location = new System.Drawing.Point(280, 5);
            this.chkMDB.Size = new System.Drawing.Size(70, 20);
            this.chkMDB.AutoSize = true;
            // 
            // chkLDU
            // 
            this.chkLDU.Text = "☑ LDU";
            this.chkLDU.Checked = true;
            this.chkLDU.Location = new System.Drawing.Point(370, 5);
            this.chkLDU.Size = new System.Drawing.Size(70, 20);
            this.chkLDU.AutoSize = true;
            // 
            // chkLDB
            // 
            this.chkLDB.Text = "☑ LDB";
            this.chkLDB.Checked = true;
            this.chkLDB.Location = new System.Drawing.Point(460, 5);
            this.chkLDB.Size = new System.Drawing.Size(70, 20);
            this.chkLDB.AutoSize = true;
            // 
            // chkFP
            // 
            this.chkFP.Text = "☑ FP";
            this.chkFP.Checked = true;
            this.chkFP.Location = new System.Drawing.Point(550, 5);
            this.chkFP.Size = new System.Drawing.Size(60, 20);
            this.chkFP.AutoSize = true;
            // 
            // chkCDB
            // 
            this.chkCDB.Text = "☐ CDB";
            this.chkCDB.Checked = false;
            this.chkCDB.Location = new System.Drawing.Point(630, 5);
            this.chkCDB.Size = new System.Drawing.Size(70, 20);
            this.chkCDB.AutoSize = true;
            // 
            // dbLoadPanel
            // 
            this.dbLoadPanel.BackColor = System.Drawing.Color.AliceBlue;
            this.dbLoadPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.dbLoadPanel.Controls.Add(this.typesPanel);
            this.dbLoadPanel.Controls.Add(this.lblSignatureTypes);
            this.dbLoadPanel.Controls.Add(this.btnLoadDb);
            this.dbLoadPanel.Controls.Add(this.lblDbFolder);
            this.dbLoadPanel.Controls.Add(this.btnBrowseDb);
            this.dbLoadPanel.Controls.Add(this.txtDbPath);
            this.dbLoadPanel.Controls.Add(this.lblDbPath);
            this.dbLoadPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.dbLoadPanel.Location = new System.Drawing.Point(0, 24);
            this.dbLoadPanel.Name = "dbLoadPanel";
            this.dbLoadPanel.Padding = new System.Windows.Forms.Padding(5);
            this.dbLoadPanel.Size = new System.Drawing.Size(1100, 105);
            this.dbLoadPanel.TabIndex = 1;
            // 
            // txtDbPath
            // 
            this.txtDbPath.Font = new System.Drawing.Font("Arial", 9F);
            this.txtDbPath.Location = new System.Drawing.Point(100, 12);
            this.txtDbPath.Name = "txtDbPath";
            this.txtDbPath.Size = new System.Drawing.Size(850, 21);
            this.txtDbPath.TabIndex = 3;
            this.txtDbPath.Text = "D:\\Data\\Tailieu\\Projects\\C#\\ClamAV_Engine\\ClamAV_Engine\\bin\\Debug\\clamdb\\daily\\";
            // 
            // btnBrowseDb
            // 
            this.btnBrowseDb.Font = new System.Drawing.Font("Arial", 9F);
            this.btnBrowseDb.Location = new System.Drawing.Point(960, 10);
            this.btnBrowseDb.Name = "btnBrowseDb";
            this.btnBrowseDb.Size = new System.Drawing.Size(130, 25);
            this.btnBrowseDb.TabIndex = 4;
            this.btnBrowseDb.Text = "Browse...";
            this.btnBrowseDb.UseVisualStyleBackColor = true;
            this.btnBrowseDb.Click += new System.EventHandler(this.BrowseDatabase_Click);
            // 
            // lblDbPath
            // 
            this.lblDbPath.AutoSize = true;
            this.lblDbPath.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold);
            this.lblDbPath.Location = new System.Drawing.Point(8, 15);
            this.lblDbPath.Name = "lblDbPath";
            this.lblDbPath.Size = new System.Drawing.Size(85, 15);
            this.lblDbPath.TabIndex = 2;
            this.lblDbPath.Text = "Database Path:";
            // 
            // lblDbFolder
            // 
            this.lblDbFolder.AutoSize = true;
            this.lblDbFolder.Font = new System.Drawing.Font("Arial", 8F);
            this.lblDbFolder.Location = new System.Drawing.Point(140, 52);
            this.lblDbFolder.Name = "lblDbFolder";
            this.lblDbFolder.Size = new System.Drawing.Size(95, 14);
            this.lblDbFolder.TabIndex = 1;
            this.lblDbFolder.Text = "No folder selected";
            // 
            // btnLoadDb
            // 
            this.btnLoadDb.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold);
            this.btnLoadDb.Location = new System.Drawing.Point(8, 43);
            this.btnLoadDb.Name = "btnLoadDb";
            this.btnLoadDb.Size = new System.Drawing.Size(95, 30);
            this.btnLoadDb.TabIndex = 0;
            this.btnLoadDb.Text = "Load DB";
            this.btnLoadDb.UseVisualStyleBackColor = true;
            this.btnLoadDb.Click += new System.EventHandler(this.LoadDatabase_Click);
            // 
            // dbStatusPanel
            // 
            this.dbStatusPanel.Controls.Add(this.lblDbStatus);
            this.dbStatusPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.dbStatusPanel.Location = new System.Drawing.Point(0, 104);
            this.dbStatusPanel.Name = "dbStatusPanel";
            this.dbStatusPanel.Padding = new System.Windows.Forms.Padding(5);
            this.dbStatusPanel.Size = new System.Drawing.Size(1100, 50);
            this.dbStatusPanel.TabIndex = 2;
            // 
            // lblDbStatus
            // 
            this.lblDbStatus.AutoSize = true;
            this.lblDbStatus.Font = new System.Drawing.Font("Arial", 9F);
            this.lblDbStatus.ForeColor = System.Drawing.Color.Red;
            this.lblDbStatus.Location = new System.Drawing.Point(5, 5);
            this.lblDbStatus.Name = "lblDbStatus";
            this.lblDbStatus.Size = new System.Drawing.Size(119, 15);
            this.lblDbStatus.TabIndex = 0;
            this.lblDbStatus.Text = "No database loaded";
            // 
            // scanPanel
            // 
            this.scanPanel.BackColor = System.Drawing.Color.Honeydew;
            this.scanPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.scanPanel.Controls.Add(this.lblFileInfo);
            this.scanPanel.Controls.Add(this.lblScanTime);
            this.scanPanel.Controls.Add(this.prgProgress);
            this.scanPanel.Controls.Add(this.lblFileName);
            this.scanPanel.Controls.Add(this.btnClear);
            this.scanPanel.Controls.Add(this.btnScan);
            this.scanPanel.Controls.Add(this.btnBrowse);
            this.scanPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.scanPanel.Location = new System.Drawing.Point(0, 154);
            this.scanPanel.Name = "scanPanel";
            this.scanPanel.Padding = new System.Windows.Forms.Padding(5);
            this.scanPanel.Size = new System.Drawing.Size(1100, 90);
            this.scanPanel.TabIndex = 3;
            // 
            // lblFileInfo
            // 
            this.lblFileInfo.AutoSize = true;
            this.lblFileInfo.Font = new System.Drawing.Font("Arial", 8F);
            this.lblFileInfo.Location = new System.Drawing.Point(5, 62);
            this.lblFileInfo.Name = "lblFileInfo";
            this.lblFileInfo.Size = new System.Drawing.Size(19, 14);
            this.lblFileInfo.TabIndex = 6;
            this.lblFileInfo.Text = "---";
            // 
            // lblScanTime
            // 
            this.lblScanTime.AutoSize = true;
            this.lblScanTime.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            this.lblScanTime.Location = new System.Drawing.Point(420, 42);
            this.lblScanTime.Name = "lblScanTime";
            this.lblScanTime.Size = new System.Drawing.Size(0, 14);
            this.lblScanTime.TabIndex = 5;
            // 
            // prgProgress
            // 
            this.prgProgress.Location = new System.Drawing.Point(70, 40);
            this.prgProgress.Name = "prgProgress";
            this.prgProgress.Size = new System.Drawing.Size(340, 15);
            this.prgProgress.TabIndex = 4;
            // 
            // lblFileName
            // 
            this.lblFileName.AutoSize = true;
            this.lblFileName.Font = new System.Drawing.Font("Arial", 8F);
            this.lblFileName.Location = new System.Drawing.Point(290, 15);
            this.lblFileName.Name = "lblFileName";
            this.lblFileName.Size = new System.Drawing.Size(81, 14);
            this.lblFileName.TabIndex = 3;
            this.lblFileName.Text = "No file selected";
            // 
            // btnClear
            // 
            this.btnClear.Location = new System.Drawing.Point(195, 10);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(90, 25);
            this.btnClear.TabIndex = 2;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // btnScan
            // 
            this.btnScan.Enabled = false;
            this.btnScan.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold);
            this.btnScan.Location = new System.Drawing.Point(100, 10);
            this.btnScan.Name = "btnScan";
            this.btnScan.Size = new System.Drawing.Size(90, 25);
            this.btnScan.TabIndex = 1;
            this.btnScan.Text = "Scan";
            this.btnScan.UseVisualStyleBackColor = true;
            this.btnScan.Click += new System.EventHandler(this.StartScan_Click);
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(5, 10);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(90, 25);
            this.btnBrowse.TabIndex = 0;
            this.btnBrowse.Text = "Browse";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.BrowseFile_Click);
            // 
            // resultPanel
            // 
            this.resultPanel.Controls.Add(this.dgvResults);
            this.resultPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.resultPanel.Location = new System.Drawing.Point(0, 244);
            this.resultPanel.Name = "resultPanel";
            this.resultPanel.Padding = new System.Windows.Forms.Padding(5);
            this.resultPanel.Size = new System.Drawing.Size(1100, 506);
            this.resultPanel.TabIndex = 4;
            // 
            // dgvResults
            // 
            this.dgvResults.AllowUserToAddRows = false;
            this.dgvResults.BackgroundColor = System.Drawing.Color.White;
            this.dgvResults.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.dgvResults.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvResults.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colIndex,
            this.colType,
            this.colName,
            this.colInfo});
            this.dgvResults.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvResults.Location = new System.Drawing.Point(5, 5);
            this.dgvResults.MultiSelect = false;
            this.dgvResults.Name = "dgvResults";
            this.dgvResults.ReadOnly = true;
            this.dgvResults.Size = new System.Drawing.Size(1090, 526);
            this.dgvResults.TabIndex = 0;
            // 
            // colIndex
            // 
            this.colIndex.HeaderText = "No.";
            this.colIndex.Name = "colIndex";
            this.colIndex.ReadOnly = true;
            this.colIndex.Width = 50;
            // 
            // colType
            // 
            this.colType.HeaderText = "Type";
            this.colType.Name = "colType";
            this.colType.ReadOnly = true;
            this.colType.Width = 80;
            // 
            // colName
            // 
            this.colName.HeaderText = "Detection Name";
            this.colName.Name = "colName";
            this.colName.ReadOnly = true;
            this.colName.Width = 300;
            // 
            // colInfo
            // 
            this.colInfo.HeaderText = "Info";
            this.colInfo.Name = "colInfo";
            this.colInfo.ReadOnly = true;
            this.colInfo.Width = 400;
            // 
            // Form2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1100, 750);
            this.Controls.Add(this.resultPanel);
            this.Controls.Add(this.scanPanel);
            this.Controls.Add(this.dbStatusPanel);
            this.Controls.Add(this.dbLoadPanel);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form2";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ClamAV AC Scanner";
            this.Load += new System.EventHandler(this.Form2_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.dbLoadPanel.ResumeLayout(false);
            this.dbLoadPanel.PerformLayout();
            this.dbStatusPanel.ResumeLayout(false);
            this.dbStatusPanel.PerformLayout();
            this.scanPanel.ResumeLayout(false);
            this.scanPanel.PerformLayout();
            this.resultPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvResults)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DataGridViewTextBoxColumn colIndex;
        private DataGridViewTextBoxColumn colType;
        private DataGridViewTextBoxColumn colName;
        private DataGridViewTextBoxColumn colInfo;
    }
}
