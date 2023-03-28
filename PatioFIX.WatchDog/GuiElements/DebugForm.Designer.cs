namespace PatioFIX.WatchDog
{
    partial class DebugForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DebugForm));
            this.chkVerbose = new System.Windows.Forms.CheckBox();
            this.chkInfo = new System.Windows.Forms.CheckBox();
            this.chkWarning = new System.Windows.Forms.CheckBox();
            this.chkError = new System.Windows.Forms.CheckBox();
            this.traceListView = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
            this.toolStripSrvManual = new System.Windows.Forms.ToolStrip();
            this.btnStart = new System.Windows.Forms.ToolStripButton();
            this.btnStop = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.btnClear = new System.Windows.Forms.ToolStripButton();
            this.toolStripSrvManual.SuspendLayout();
            this.SuspendLayout();
            // 
            // chkVerbose
            // 
            this.chkVerbose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkVerbose.AutoSize = true;
            this.chkVerbose.Checked = true;
            this.chkVerbose.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkVerbose.Location = new System.Drawing.Point(227, 564);
            this.chkVerbose.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.chkVerbose.Name = "chkVerbose";
            this.chkVerbose.Size = new System.Drawing.Size(67, 19);
            this.chkVerbose.TabIndex = 5;
            this.chkVerbose.Text = "Verbose";
            this.chkVerbose.UseVisualStyleBackColor = true;
            // 
            // chkInfo
            // 
            this.chkInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkInfo.AutoSize = true;
            this.chkInfo.Checked = true;
            this.chkInfo.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkInfo.Location = new System.Drawing.Point(169, 564);
            this.chkInfo.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.chkInfo.Name = "chkInfo";
            this.chkInfo.Size = new System.Drawing.Size(47, 19);
            this.chkInfo.TabIndex = 6;
            this.chkInfo.Text = "Info";
            this.chkInfo.UseVisualStyleBackColor = true;
            // 
            // chkWarning
            // 
            this.chkWarning.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkWarning.AutoSize = true;
            this.chkWarning.Checked = true;
            this.chkWarning.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkWarning.Location = new System.Drawing.Point(79, 564);
            this.chkWarning.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.chkWarning.Name = "chkWarning";
            this.chkWarning.Size = new System.Drawing.Size(76, 19);
            this.chkWarning.TabIndex = 3;
            this.chkWarning.Text = "Warnings";
            this.chkWarning.UseVisualStyleBackColor = true;
            // 
            // chkError
            // 
            this.chkError.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkError.AutoSize = true;
            this.chkError.Checked = true;
            this.chkError.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkError.Location = new System.Drawing.Point(10, 564);
            this.chkError.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.chkError.Name = "chkError";
            this.chkError.Size = new System.Drawing.Size(56, 19);
            this.chkError.TabIndex = 4;
            this.chkError.Text = "Errors";
            this.chkError.UseVisualStyleBackColor = true;
            // 
            // traceListView
            // 
            this.traceListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.traceListView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.traceListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.traceListView.GridLines = true;
            this.traceListView.Location = new System.Drawing.Point(9, 36);
            this.traceListView.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.traceListView.MultiSelect = false;
            this.traceListView.Name = "traceListView";
            this.traceListView.Size = new System.Drawing.Size(929, 524);
            this.traceListView.TabIndex = 1;
            this.traceListView.UseCompatibleStateImageBehavior = false;
            this.traceListView.View = System.Windows.Forms.View.Details;
            this.traceListView.DoubleClick += new System.EventHandler(this.OnViewDoubleClick);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "LogLevel";
            this.columnHeader1.Width = 108;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "OwnerName";
            this.columnHeader2.Width = 150;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Message";
            this.columnHeader3.Width = 532;
            // 
            // toolStripSrvManual
            // 
            this.toolStripSrvManual.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripSrvManual.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnStart,
            this.btnStop,
            this.toolStripSeparator1,
            this.btnClear});
            this.toolStripSrvManual.Location = new System.Drawing.Point(0, 0);
            this.toolStripSrvManual.Name = "toolStripSrvManual";
            this.toolStripSrvManual.Size = new System.Drawing.Size(952, 25);
            this.toolStripSrvManual.TabIndex = 0;
            this.toolStripSrvManual.Text = "toolStrip1";
            // 
            // btnStart
            // 
            this.btnStart.Image = ((System.Drawing.Image)(resources.GetObject("btnStart.Image")));
            this.btnStart.ImageTransparentColor = System.Drawing.Color.Black;
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(51, 22);
            this.btnStart.Text = "Start";
            this.btnStart.Click += new System.EventHandler(this.OnBtnStart);
            // 
            // btnStop
            // 
            this.btnStop.Enabled = false;
            this.btnStop.Image = ((System.Drawing.Image)(resources.GetObject("btnStop.Image")));
            this.btnStop.ImageTransparentColor = System.Drawing.Color.Black;
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(51, 22);
            this.btnStop.Text = "Stop";
            this.btnStop.Click += new System.EventHandler(this.OnBtnStop);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // btnClear
            // 
            this.btnClear.Image = ((System.Drawing.Image)(resources.GetObject("btnClear.Image")));
            this.btnClear.ImageTransparentColor = System.Drawing.Color.Black;
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(54, 22);
            this.btnClear.Text = "Clear";
            this.btnClear.Click += new System.EventHandler(this.OnBtnClear);
            // 
            // DebugForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(952, 625);
            this.Controls.Add(this.chkVerbose);
            this.Controls.Add(this.chkInfo);
            this.Controls.Add(this.chkWarning);
            this.Controls.Add(this.chkError);
            this.Controls.Add(this.traceListView);
            this.Controls.Add(this.toolStripSrvManual);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "DebugForm";
            this.Text = "DebugForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OnFormClosing);
            this.Load += new System.EventHandler(this.DebugForm_Load);
            this.toolStripSrvManual.ResumeLayout(false);
            this.toolStripSrvManual.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion


        private System.Windows.Forms.ToolStrip toolStripSrvManual;
        private System.Windows.Forms.ToolStripButton btnStart;
        private System.Windows.Forms.ToolStripButton btnStop;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton btnClear;
        internal System.Windows.Forms.CheckBox chkVerbose;
        internal System.Windows.Forms.CheckBox chkInfo;
        internal System.Windows.Forms.CheckBox chkWarning;
        internal System.Windows.Forms.CheckBox chkError;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        internal System.Windows.Forms.ListView traceListView;

    }
}