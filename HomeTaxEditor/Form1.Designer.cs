namespace HomeTaxEditor;

partial class Form1
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
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
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.panelSidebar = new System.Windows.Forms.Panel();
        this.lblDescription = new System.Windows.Forms.Label();
        this.btnStartProcess = new System.Windows.Forms.Button();
        this.btnStop = new System.Windows.Forms.Button();
        this.progressBar = new System.Windows.Forms.ProgressBar();
        this.lblDevMode = new System.Windows.Forms.Label();
        this.panelDevTools = new System.Windows.Forms.Panel();
        this.chkDetailedLog = new System.Windows.Forms.CheckBox();
        this.btnDevTools = new System.Windows.Forms.Button();
        this.btnTestDOM = new System.Windows.Forms.Button();
        this.btnExtractHTML = new System.Windows.Forms.Button();
        this.panelMain = new System.Windows.Forms.Panel();
        this.webView = new Microsoft.Web.WebView2.WinForms.WebView2();
        this.tabControl = new System.Windows.Forms.TabControl();
        this.tabLog = new System.Windows.Forms.TabPage();
        this.txtLog = new System.Windows.Forms.TextBox();
        this.tabChanges = new System.Windows.Forms.TabPage();
        this.btnDownloadExcel = new System.Windows.Forms.Button();
        this.dataGridChanges = new System.Windows.Forms.DataGridView();
        this.panelSidebar.SuspendLayout();
        this.panelDevTools.SuspendLayout();
        this.panelMain.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.webView)).BeginInit();
        this.tabControl.SuspendLayout();
        this.tabLog.SuspendLayout();
        this.tabChanges.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.dataGridChanges)).BeginInit();
        this.SuspendLayout();
        //
        // panelSidebar
        //
        this.panelSidebar.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
        this.panelSidebar.Controls.Add(this.lblDescription);
        this.panelSidebar.Controls.Add(this.btnStartProcess);
        this.panelSidebar.Controls.Add(this.btnStop);
        this.panelSidebar.Controls.Add(this.progressBar);
        this.panelSidebar.Controls.Add(this.lblDevMode);
        this.panelSidebar.Controls.Add(this.panelDevTools);
        this.panelSidebar.Dock = System.Windows.Forms.DockStyle.Left;
        this.panelSidebar.Location = new System.Drawing.Point(0, 0);
        this.panelSidebar.Name = "panelSidebar";
        this.panelSidebar.Padding = new System.Windows.Forms.Padding(10);
        this.panelSidebar.Size = new System.Drawing.Size(280, 700);
        this.panelSidebar.TabIndex = 0;
        //
        // lblDescription
        //
        this.lblDescription.Font = new System.Drawing.Font("Segoe UI", 10F);
        this.lblDescription.Location = new System.Drawing.Point(15, 15);
        this.lblDescription.Name = "lblDescription";
        this.lblDescription.Size = new System.Drawing.Size(250, 60);
        this.lblDescription.TabIndex = 0;
        this.lblDescription.Text = "홈택스 로그인 후,\n적용할 엑셀을 업로드 해주세요";
        this.lblDescription.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        //
        // btnStartProcess
        //
        this.btnStartProcess.Location = new System.Drawing.Point(15, 95);
        this.btnStartProcess.Name = "btnStartProcess";
        this.btnStartProcess.Size = new System.Drawing.Size(250, 50);
        this.btnStartProcess.TabIndex = 1;
        this.btnStartProcess.Text = "엑셀 업로드 및 시작";
        this.btnStartProcess.UseVisualStyleBackColor = true;
        this.btnStartProcess.Click += new System.EventHandler(this.btnStartProcess_Click);
        //
        // btnStop
        //
        this.btnStop.Enabled = false;
        this.btnStop.Location = new System.Drawing.Point(15, 155);
        this.btnStop.Name = "btnStop";
        this.btnStop.Size = new System.Drawing.Size(250, 50);
        this.btnStop.TabIndex = 2;
        this.btnStop.Text = "중단";
        this.btnStop.UseVisualStyleBackColor = true;
        this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
        //
        // progressBar
        //
        this.progressBar.Location = new System.Drawing.Point(15, 220);
        this.progressBar.Name = "progressBar";
        this.progressBar.Size = new System.Drawing.Size(250, 25);
        this.progressBar.TabIndex = 3;
        //
        // lblDevMode
        //
        this.lblDevMode.AutoSize = true;
        this.lblDevMode.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        this.lblDevMode.ForeColor = System.Drawing.Color.Red;
        this.lblDevMode.Location = new System.Drawing.Point(15, 260);
        this.lblDevMode.Name = "lblDevMode";
        this.lblDevMode.Size = new System.Drawing.Size(104, 20);
        this.lblDevMode.TabIndex = 4;
        this.lblDevMode.Text = "[개발자 모드]";
        this.lblDevMode.Visible = false;
        //
        // panelDevTools
        //
        this.panelDevTools.Controls.Add(this.chkDetailedLog);
        this.panelDevTools.Controls.Add(this.btnDevTools);
        this.panelDevTools.Controls.Add(this.btnTestDOM);
        this.panelDevTools.Controls.Add(this.btnExtractHTML);
        this.panelDevTools.Location = new System.Drawing.Point(10, 290);
        this.panelDevTools.Name = "panelDevTools";
        this.panelDevTools.Size = new System.Drawing.Size(260, 200);
        this.panelDevTools.TabIndex = 5;
        this.panelDevTools.Visible = false;
        //
        // chkDetailedLog
        //
        this.chkDetailedLog.AutoSize = true;
        this.chkDetailedLog.Location = new System.Drawing.Point(5, 155);
        this.chkDetailedLog.Name = "chkDetailedLog";
        this.chkDetailedLog.Size = new System.Drawing.Size(104, 24);
        this.chkDetailedLog.TabIndex = 3;
        this.chkDetailedLog.Text = "상세 로그";
        this.chkDetailedLog.UseVisualStyleBackColor = true;
        //
        // btnDevTools
        //
        this.btnDevTools.Location = new System.Drawing.Point(5, 105);
        this.btnDevTools.Name = "btnDevTools";
        this.btnDevTools.Size = new System.Drawing.Size(220, 40);
        this.btnDevTools.TabIndex = 2;
        this.btnDevTools.Text = "개발자 도구 열기";
        this.btnDevTools.UseVisualStyleBackColor = true;
        this.btnDevTools.Click += new System.EventHandler(this.btnDevTools_Click);
        //
        // btnTestDOM
        //
        this.btnTestDOM.Location = new System.Drawing.Point(5, 55);
        this.btnTestDOM.Name = "btnTestDOM";
        this.btnTestDOM.Size = new System.Drawing.Size(220, 40);
        this.btnTestDOM.TabIndex = 1;
        this.btnTestDOM.Text = "DOM 테스트 실행";
        this.btnTestDOM.UseVisualStyleBackColor = true;
        this.btnTestDOM.Click += new System.EventHandler(this.btnTestDOM_Click);
        //
        // btnExtractHTML
        //
        this.btnExtractHTML.Location = new System.Drawing.Point(5, 5);
        this.btnExtractHTML.Name = "btnExtractHTML";
        this.btnExtractHTML.Size = new System.Drawing.Size(220, 40);
        this.btnExtractHTML.TabIndex = 0;
        this.btnExtractHTML.Text = "HTML 추출";
        this.btnExtractHTML.UseVisualStyleBackColor = true;
        this.btnExtractHTML.Click += new System.EventHandler(this.btnExtractHTML_Click);
        //
        // panelMain
        //
        this.panelMain.Controls.Add(this.webView);
        this.panelMain.Controls.Add(this.tabControl);
        this.panelMain.Dock = System.Windows.Forms.DockStyle.Fill;
        this.panelMain.Location = new System.Drawing.Point(280, 0);
        this.panelMain.Name = "panelMain";
        this.panelMain.Size = new System.Drawing.Size(920, 700);
        this.panelMain.TabIndex = 1;
        //
        // webView
        //
        this.webView.AllowExternalDrop = true;
        this.webView.CreationProperties = null;
        this.webView.DefaultBackgroundColor = System.Drawing.Color.White;
        this.webView.Dock = System.Windows.Forms.DockStyle.Fill;
        this.webView.Location = new System.Drawing.Point(0, 0);
        this.webView.Name = "webView";
        this.webView.Size = new System.Drawing.Size(950, 500);
        this.webView.TabIndex = 0;
        this.webView.ZoomFactor = 1D;
        //
        // tabControl
        //
        this.tabControl.Controls.Add(this.tabLog);
        this.tabControl.Controls.Add(this.tabChanges);
        this.tabControl.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.tabControl.Location = new System.Drawing.Point(0, 500);
        this.tabControl.Name = "tabControl";
        this.tabControl.SelectedIndex = 0;
        this.tabControl.Size = new System.Drawing.Size(950, 200);
        this.tabControl.TabIndex = 1;
        //
        // tabLog
        //
        this.tabLog.Controls.Add(this.txtLog);
        this.tabLog.Location = new System.Drawing.Point(4, 29);
        this.tabLog.Name = "tabLog";
        this.tabLog.Padding = new System.Windows.Forms.Padding(3);
        this.tabLog.Size = new System.Drawing.Size(942, 167);
        this.tabLog.TabIndex = 0;
        this.tabLog.Text = "로그";
        this.tabLog.UseVisualStyleBackColor = true;
        //
        // txtLog
        //
        this.txtLog.Dock = System.Windows.Forms.DockStyle.Fill;
        this.txtLog.Location = new System.Drawing.Point(3, 3);
        this.txtLog.Multiline = true;
        this.txtLog.Name = "txtLog";
        this.txtLog.ReadOnly = true;
        this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
        this.txtLog.Size = new System.Drawing.Size(936, 161);
        this.txtLog.TabIndex = 0;
        //
        // tabChanges
        //
        this.tabChanges.Controls.Add(this.btnDownloadExcel);
        this.tabChanges.Controls.Add(this.dataGridChanges);
        this.tabChanges.Location = new System.Drawing.Point(4, 29);
        this.tabChanges.Name = "tabChanges";
        this.tabChanges.Padding = new System.Windows.Forms.Padding(3);
        this.tabChanges.Size = new System.Drawing.Size(942, 167);
        this.tabChanges.TabIndex = 1;
        this.tabChanges.Text = "변경 내역";
        this.tabChanges.UseVisualStyleBackColor = true;
        //
        // btnDownloadExcel
        //
        this.btnDownloadExcel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        this.btnDownloadExcel.Location = new System.Drawing.Point(790, 130);
        this.btnDownloadExcel.Name = "btnDownloadExcel";
        this.btnDownloadExcel.Size = new System.Drawing.Size(140, 30);
        this.btnDownloadExcel.TabIndex = 1;
        this.btnDownloadExcel.Text = "Excel로 다운로드";
        this.btnDownloadExcel.UseVisualStyleBackColor = true;
        this.btnDownloadExcel.Click += new System.EventHandler(this.btnDownloadExcel_Click);
        //
        // dataGridChanges
        //
        this.dataGridChanges.AllowUserToAddRows = false;
        this.dataGridChanges.AllowUserToDeleteRows = false;
        this.dataGridChanges.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
        this.dataGridChanges.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        this.dataGridChanges.Location = new System.Drawing.Point(3, 3);
        this.dataGridChanges.Name = "dataGridChanges";
        this.dataGridChanges.ReadOnly = true;
        this.dataGridChanges.RowHeadersWidth = 51;
        this.dataGridChanges.Size = new System.Drawing.Size(936, 120);
        this.dataGridChanges.TabIndex = 0;
        //
        // Form1
        //
        this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(1200, 700);
        this.Controls.Add(this.panelMain);
        this.Controls.Add(this.panelSidebar);
        this.Name = "Form1";
        this.Text = "홈택스 자동 공제 반영 도구";
        this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
        this.Load += new System.EventHandler(this.Form1_Load);
        this.panelSidebar.ResumeLayout(false);
        this.panelSidebar.PerformLayout();
        this.panelDevTools.ResumeLayout(false);
        this.panelDevTools.PerformLayout();
        this.panelMain.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.webView)).EndInit();
        this.tabControl.ResumeLayout(false);
        this.tabLog.ResumeLayout(false);
        this.tabLog.PerformLayout();
        this.tabChanges.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.dataGridChanges)).EndInit();
        this.ResumeLayout(false);
    }

    #endregion

    private System.Windows.Forms.Panel panelSidebar;
    private System.Windows.Forms.Panel panelMain;
    private Microsoft.Web.WebView2.WinForms.WebView2 webView;
    private System.Windows.Forms.Label lblDescription;
    private System.Windows.Forms.Button btnStartProcess;
    private System.Windows.Forms.Button btnStop;
    private System.Windows.Forms.ProgressBar progressBar;
    private System.Windows.Forms.TabControl tabControl;
    private System.Windows.Forms.TabPage tabLog;
    private System.Windows.Forms.TextBox txtLog;
    private System.Windows.Forms.TabPage tabChanges;
    private System.Windows.Forms.DataGridView dataGridChanges;
    private System.Windows.Forms.Button btnDownloadExcel;
    private System.Windows.Forms.Panel panelDevTools;
    private System.Windows.Forms.Button btnExtractHTML;
    private System.Windows.Forms.Button btnTestDOM;
    private System.Windows.Forms.Button btnDevTools;
    private System.Windows.Forms.CheckBox chkDetailedLog;
    private System.Windows.Forms.Label lblDevMode;
}
