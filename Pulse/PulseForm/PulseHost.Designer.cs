namespace PulseForm
{
    partial class frmPulseHost
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
            this.components = new System.ComponentModel.Container();
            this.cmsTrayIcon = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.nextPictureToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.banImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openCacheFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.niPulse = new System.Windows.Forms.NotifyIcon(this.components);
            this.cmsTrayIcon.SuspendLayout();
            this.SuspendLayout();
            // 
            // cmsTrayIcon
            // 
            this.cmsTrayIcon.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.nextPictureToolStripMenuItem,
            this.banImageToolStripMenuItem,
            this.openCacheFolderToolStripMenuItem,
            this.toolStripMenuItem1,
            this.toolStripSeparator1,
            this.closeToolStripMenuItem});
            this.cmsTrayIcon.Name = "cmsTrayIcon";
            this.cmsTrayIcon.Size = new System.Drawing.Size(236, 182);
            // 
            // nextPictureToolStripMenuItem
            // 
            this.nextPictureToolStripMenuItem.Image = global::PulseForm.Properties.Resources._112_DownArrowLong_Green_24x24_72;
            this.nextPictureToolStripMenuItem.Name = "nextPictureToolStripMenuItem";
            this.nextPictureToolStripMenuItem.Size = new System.Drawing.Size(235, 30);
            this.nextPictureToolStripMenuItem.Text = "Next Picture";
            this.nextPictureToolStripMenuItem.Click += new System.EventHandler(this.nextPictureToolStripMenuItem_Click);
            // 
            // banImageToolStripMenuItem
            // 
            this.banImageToolStripMenuItem.Name = "banImageToolStripMenuItem";
            this.banImageToolStripMenuItem.Size = new System.Drawing.Size(235, 30);
            this.banImageToolStripMenuItem.Text = "Ban Image";
            this.banImageToolStripMenuItem.Click += new System.EventHandler(this.banImageToolStripMenuItem_Click);
            // 
            // openCacheFolderToolStripMenuItem
            // 
            this.openCacheFolderToolStripMenuItem.Image = global::PulseForm.Properties.Resources.InsertPictureHS;
            this.openCacheFolderToolStripMenuItem.Name = "openCacheFolderToolStripMenuItem";
            this.openCacheFolderToolStripMenuItem.Size = new System.Drawing.Size(235, 30);
            this.openCacheFolderToolStripMenuItem.Text = "Open Cache Folder";
            this.openCacheFolderToolStripMenuItem.Click += new System.EventHandler(this.openCacheFolderToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Image = global::PulseForm.Properties.Resources._327_Options_16x16_72;
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(235, 30);
            this.toolStripMenuItem1.Text = "Options";
            this.toolStripMenuItem1.Click += new System.EventHandler(this.toolStripMenuItem1_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(232, 6);
            // 
            // closeToolStripMenuItem
            // 
            this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            this.closeToolStripMenuItem.Size = new System.Drawing.Size(235, 30);
            this.closeToolStripMenuItem.Text = "Close";
            this.closeToolStripMenuItem.Click += new System.EventHandler(this.closeToolStripMenuItem_Click);
            // 
            // niPulse
            // 
            this.niPulse.ContextMenuStrip = this.cmsTrayIcon;
            this.niPulse.Text = "Pulse";
            // 
            // frmPulseHost
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(361, 58);
            this.ContextMenuStrip = this.cmsTrayIcon;
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.MaximizeBox = false;
            this.Name = "frmPulseHost";
            this.ShowInTaskbar = false;
            this.Text = "Pulse Host";
            this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
            this.Load += new System.EventHandler(this.frmPulseHost_Load);
            this.cmsTrayIcon.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openCacheFolderToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem nextPictureToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem banImageToolStripMenuItem;
        internal System.Windows.Forms.ContextMenuStrip cmsTrayIcon;
        public System.Windows.Forms.NotifyIcon niPulse;
    }
}

