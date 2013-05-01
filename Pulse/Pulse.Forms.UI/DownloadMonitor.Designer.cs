namespace Pulse.Forms.UI
{
    partial class DownloadMonitor
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.btnClearQueue = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.flpDownloads = new System.Windows.Forms.FlowLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.btnClose);
            this.splitContainer1.Panel1.Controls.Add(this.btnClearQueue);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.flpDownloads);
            this.splitContainer1.Size = new System.Drawing.Size(881, 505);
            this.splitContainer1.SplitterDistance = 43;
            this.splitContainer1.TabIndex = 1;
            // 
            // btnClearQueue
            // 
            this.btnClearQueue.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnClearQueue.Image = global::Pulse.Forms.UI.Properties.Resources.Clearwindowcontent_6304;
            this.btnClearQueue.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnClearQueue.Location = new System.Drawing.Point(0, 0);
            this.btnClearQueue.Name = "btnClearQueue";
            this.btnClearQueue.Size = new System.Drawing.Size(150, 43);
            this.btnClearQueue.TabIndex = 0;
            this.btnClearQueue.Text = "Clear Queue";
            this.btnClearQueue.UseVisualStyleBackColor = true;
            this.btnClearQueue.Click += new System.EventHandler(this.btnClearQueue_Click);
            // 
            // btnClose
            // 
            this.btnClose.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnClose.Location = new System.Drawing.Point(731, 0);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(150, 43);
            this.btnClose.TabIndex = 1;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // flpDownloads
            // 
            this.flpDownloads.AutoScroll = true;
            this.flpDownloads.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpDownloads.Location = new System.Drawing.Point(0, 0);
            this.flpDownloads.Name = "flpDownloads";
            this.flpDownloads.Size = new System.Drawing.Size(881, 458);
            this.flpDownloads.TabIndex = 0;
            // 
            // DownloadMonitor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(881, 505);
            this.Controls.Add(this.splitContainer1);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "DownloadMonitor";
            this.Text = "Download Monitor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DownloadMonitor_FormClosing);
            this.Load += new System.EventHandler(this.DownloadMonitor_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button btnClearQueue;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.FlowLayoutPanel flpDownloads;

    }
}