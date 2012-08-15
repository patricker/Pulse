﻿namespace PulseForm
{
    partial class frmPulseOptions
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmPulseOptions));
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tpGeneral = new System.Windows.Forms.TabPage();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.panel4 = new System.Windows.Forms.Panel();
            this.btnClearNow = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.nudTempAge = new System.Windows.Forms.NumericUpDown();
            this.cbDeleteOldFiles = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.panel3 = new System.Windows.Forms.Panel();
            this.SearchBox = new System.Windows.Forms.TextBox();
            this.cbProviders = new System.Windows.Forms.ComboBox();
            this.ProviderSettings = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.udInterval = new System.Windows.Forms.NumericUpDown();
            this.cbDownloadAutomatically = new System.Windows.Forms.CheckBox();
            this.cbPrefetch = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cbAutoChangeonStartup = new System.Windows.Forms.CheckBox();
            this.cbUpdateFrequencyUnit = new System.Windows.Forms.ComboBox();
            this.tpOutputs = new System.Windows.Forms.TabPage();
            this.dgvOutputProviders = new System.Windows.Forms.DataGridView();
            this.oppSettings = new System.Windows.Forms.DataGridViewButtonColumn();
            this.tpAbout = new System.Windows.Forms.TabPage();
            this.BuildTag = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.ApplyButton = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.panel1 = new System.Windows.Forms.Panel();
            this.tabControl1.SuspendLayout();
            this.tpGeneral.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.panel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudTempAge)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udInterval)).BeginInit();
            this.tpOutputs.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvOutputProviders)).BeginInit();
            this.tpAbout.SuspendLayout();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tpGeneral);
            this.tabControl1.Controls.Add(this.tpOutputs);
            this.tabControl1.Controls.Add(this.tpAbout);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(2);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(345, 300);
            this.tabControl1.TabIndex = 0;
            // 
            // tpGeneral
            // 
            this.tpGeneral.AutoScroll = true;
            this.tpGeneral.Controls.Add(this.groupBox3);
            this.tpGeneral.Controls.Add(this.groupBox2);
            this.tpGeneral.Controls.Add(this.groupBox1);
            this.tpGeneral.Location = new System.Drawing.Point(4, 22);
            this.tpGeneral.Margin = new System.Windows.Forms.Padding(2);
            this.tpGeneral.Name = "tpGeneral";
            this.tpGeneral.Padding = new System.Windows.Forms.Padding(2);
            this.tpGeneral.Size = new System.Drawing.Size(337, 274);
            this.tpGeneral.TabIndex = 0;
            this.tpGeneral.Text = "General";
            this.tpGeneral.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.panel4);
            this.groupBox3.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox3.Location = new System.Drawing.Point(7, 177);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox3.Size = new System.Drawing.Size(321, 84);
            this.groupBox3.TabIndex = 13;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Temporary Files";
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.btnClearNow);
            this.panel4.Controls.Add(this.label1);
            this.panel4.Controls.Add(this.nudTempAge);
            this.panel4.Controls.Add(this.cbDeleteOldFiles);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.panel4.Location = new System.Drawing.Point(2, 24);
            this.panel4.Margin = new System.Windows.Forms.Padding(2);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(317, 58);
            this.panel4.TabIndex = 0;
            // 
            // btnClearNow
            // 
            this.btnClearNow.Location = new System.Drawing.Point(5, 29);
            this.btnClearNow.Margin = new System.Windows.Forms.Padding(2);
            this.btnClearNow.Name = "btnClearNow";
            this.btnClearNow.Size = new System.Drawing.Size(87, 21);
            this.btnClearNow.TabIndex = 3;
            this.btnClearNow.Text = "Clear Now";
            this.btnClearNow.UseVisualStyleBackColor = true;
            this.btnClearNow.Click += new System.EventHandler(this.ClearNowButtonClick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(213, 10);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "day(s) ago";
            // 
            // nudTempAge
            // 
            this.nudTempAge.Location = new System.Drawing.Point(156, 8);
            this.nudTempAge.Margin = new System.Windows.Forms.Padding(2);
            this.nudTempAge.Name = "nudTempAge";
            this.nudTempAge.Size = new System.Drawing.Size(43, 20);
            this.nudTempAge.TabIndex = 1;
            this.nudTempAge.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            this.nudTempAge.ValueChanged += new System.EventHandler(this.ComboBoxSelectionChanged);
            // 
            // cbDeleteOldFiles
            // 
            this.cbDeleteOldFiles.AutoSize = true;
            this.cbDeleteOldFiles.Location = new System.Drawing.Point(5, 9);
            this.cbDeleteOldFiles.Margin = new System.Windows.Forms.Padding(2);
            this.cbDeleteOldFiles.Name = "cbDeleteOldFiles";
            this.cbDeleteOldFiles.Size = new System.Drawing.Size(150, 17);
            this.cbDeleteOldFiles.TabIndex = 0;
            this.cbDeleteOldFiles.Text = "Delete pictures older than ";
            this.cbDeleteOldFiles.UseVisualStyleBackColor = true;
            this.cbDeleteOldFiles.CheckedChanged += new System.EventHandler(this.CheckBoxClick);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.panel3);
            this.groupBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox2.Location = new System.Drawing.Point(5, 4);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox2.Size = new System.Drawing.Size(323, 53);
            this.groupBox2.TabIndex = 12;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Search";
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.SearchBox);
            this.panel3.Controls.Add(this.cbProviders);
            this.panel3.Controls.Add(this.ProviderSettings);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.panel3.Location = new System.Drawing.Point(2, 24);
            this.panel3.Margin = new System.Windows.Forms.Padding(2);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(319, 27);
            this.panel3.TabIndex = 4;
            // 
            // SearchBox
            // 
            this.SearchBox.Location = new System.Drawing.Point(4, 2);
            this.SearchBox.Margin = new System.Windows.Forms.Padding(2);
            this.SearchBox.Name = "SearchBox";
            this.SearchBox.Size = new System.Drawing.Size(108, 20);
            this.SearchBox.TabIndex = 1;
            this.SearchBox.TextChanged += new System.EventHandler(this.SearchBoxTextChanged);
            // 
            // cbProviders
            // 
            this.cbProviders.DisplayMember = "ProviderName";
            this.cbProviders.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbProviders.FormattingEnabled = true;
            this.cbProviders.Location = new System.Drawing.Point(115, 1);
            this.cbProviders.Margin = new System.Windows.Forms.Padding(2);
            this.cbProviders.Name = "cbProviders";
            this.cbProviders.Size = new System.Drawing.Size(131, 21);
            this.cbProviders.TabIndex = 2;
            // 
            // ProviderSettings
            // 
            this.ProviderSettings.Location = new System.Drawing.Point(248, 1);
            this.ProviderSettings.Margin = new System.Windows.Forms.Padding(2);
            this.ProviderSettings.Name = "ProviderSettings";
            this.ProviderSettings.Size = new System.Drawing.Size(69, 21);
            this.ProviderSettings.TabIndex = 3;
            this.ProviderSettings.Text = "Settings";
            this.ProviderSettings.UseVisualStyleBackColor = true;
            this.ProviderSettings.Click += new System.EventHandler(this.ProviderSettings_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.panel2);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(7, 61);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox1.Size = new System.Drawing.Size(321, 112);
            this.groupBox1.TabIndex = 11;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Behavior";
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.udInterval);
            this.panel2.Controls.Add(this.cbDownloadAutomatically);
            this.panel2.Controls.Add(this.cbPrefetch);
            this.panel2.Controls.Add(this.label3);
            this.panel2.Controls.Add(this.cbAutoChangeonStartup);
            this.panel2.Controls.Add(this.cbUpdateFrequencyUnit);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.panel2.Location = new System.Drawing.Point(2, 24);
            this.panel2.Margin = new System.Windows.Forms.Padding(2);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(317, 86);
            this.panel2.TabIndex = 11;
            // 
            // udInterval
            // 
            this.udInterval.Location = new System.Drawing.Point(122, 40);
            this.udInterval.Margin = new System.Windows.Forms.Padding(2);
            this.udInterval.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
            this.udInterval.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.udInterval.Name = "udInterval";
            this.udInterval.Size = new System.Drawing.Size(43, 20);
            this.udInterval.TabIndex = 11;
            this.udInterval.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.udInterval.ValueChanged += new System.EventHandler(this.ComboBoxSelectionChanged);
            // 
            // cbDownloadAutomatically
            // 
            this.cbDownloadAutomatically.AutoSize = true;
            this.cbDownloadAutomatically.Location = new System.Drawing.Point(2, 23);
            this.cbDownloadAutomatically.Margin = new System.Windows.Forms.Padding(2);
            this.cbDownloadAutomatically.Name = "cbDownloadAutomatically";
            this.cbDownloadAutomatically.Size = new System.Drawing.Size(143, 17);
            this.cbDownloadAutomatically.TabIndex = 5;
            this.cbDownloadAutomatically.Text = "Change Picture on Timer";
            this.cbDownloadAutomatically.UseVisualStyleBackColor = true;
            this.cbDownloadAutomatically.CheckedChanged += new System.EventHandler(this.CheckBoxClick);
            // 
            // cbPrefetch
            // 
            this.cbPrefetch.AutoSize = true;
            this.cbPrefetch.Location = new System.Drawing.Point(2, 64);
            this.cbPrefetch.Margin = new System.Windows.Forms.Padding(2);
            this.cbPrefetch.Name = "cbPrefetch";
            this.cbPrefetch.Size = new System.Drawing.Size(113, 17);
            this.cbPrefetch.TabIndex = 10;
            this.cbPrefetch.Text = "Pre Fetch Pictures";
            this.cbPrefetch.UseVisualStyleBackColor = true;
            this.cbPrefetch.CheckedChanged += new System.EventHandler(this.CheckBoxClick);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(2, 42);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(116, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Change Picture Every: ";
            // 
            // cbAutoChangeonStartup
            // 
            this.cbAutoChangeonStartup.AutoSize = true;
            this.cbAutoChangeonStartup.Location = new System.Drawing.Point(2, 2);
            this.cbAutoChangeonStartup.Margin = new System.Windows.Forms.Padding(2);
            this.cbAutoChangeonStartup.Name = "cbAutoChangeonStartup";
            this.cbAutoChangeonStartup.Size = new System.Drawing.Size(151, 17);
            this.cbAutoChangeonStartup.TabIndex = 6;
            this.cbAutoChangeonStartup.Text = "Change Picture on Startup";
            this.cbAutoChangeonStartup.UseVisualStyleBackColor = true;
            this.cbAutoChangeonStartup.CheckedChanged += new System.EventHandler(this.CheckBoxClick);
            // 
            // cbUpdateFrequencyUnit
            // 
            this.cbUpdateFrequencyUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbUpdateFrequencyUnit.FormattingEnabled = true;
            this.cbUpdateFrequencyUnit.Location = new System.Drawing.Point(176, 40);
            this.cbUpdateFrequencyUnit.Margin = new System.Windows.Forms.Padding(2);
            this.cbUpdateFrequencyUnit.Name = "cbUpdateFrequencyUnit";
            this.cbUpdateFrequencyUnit.Size = new System.Drawing.Size(82, 21);
            this.cbUpdateFrequencyUnit.TabIndex = 9;
            this.cbUpdateFrequencyUnit.SelectedIndexChanged += new System.EventHandler(this.ComboBoxSelectionChanged);
            // 
            // tpOutputs
            // 
            this.tpOutputs.AutoScroll = true;
            this.tpOutputs.Controls.Add(this.dgvOutputProviders);
            this.tpOutputs.Location = new System.Drawing.Point(4, 22);
            this.tpOutputs.Margin = new System.Windows.Forms.Padding(2);
            this.tpOutputs.Name = "tpOutputs";
            this.tpOutputs.Padding = new System.Windows.Forms.Padding(2);
            this.tpOutputs.Size = new System.Drawing.Size(337, 273);
            this.tpOutputs.TabIndex = 1;
            this.tpOutputs.Text = "Outputs";
            this.tpOutputs.UseVisualStyleBackColor = true;
            // 
            // dgvOutputProviders
            // 
            this.dgvOutputProviders.AllowUserToAddRows = false;
            this.dgvOutputProviders.AllowUserToDeleteRows = false;
            this.dgvOutputProviders.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dgvOutputProviders.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvOutputProviders.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.oppSettings});
            this.dgvOutputProviders.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvOutputProviders.Location = new System.Drawing.Point(2, 2);
            this.dgvOutputProviders.Margin = new System.Windows.Forms.Padding(2);
            this.dgvOutputProviders.MultiSelect = false;
            this.dgvOutputProviders.Name = "dgvOutputProviders";
            this.dgvOutputProviders.RowHeadersVisible = false;
            this.dgvOutputProviders.RowTemplate.Height = 28;
            this.dgvOutputProviders.Size = new System.Drawing.Size(333, 270);
            this.dgvOutputProviders.TabIndex = 0;
            this.dgvOutputProviders.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvOutputProviders_CellClick);
            this.dgvOutputProviders.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvOutputProviders_CellValueChanged);
            // 
            // oppSettings
            // 
            this.oppSettings.HeaderText = "Settings";
            this.oppSettings.Name = "oppSettings";
            this.oppSettings.Width = 51;
            // 
            // tpAbout
            // 
            this.tpAbout.Controls.Add(this.BuildTag);
            this.tpAbout.Location = new System.Drawing.Point(4, 22);
            this.tpAbout.Margin = new System.Windows.Forms.Padding(2);
            this.tpAbout.Name = "tpAbout";
            this.tpAbout.Size = new System.Drawing.Size(337, 273);
            this.tpAbout.TabIndex = 2;
            this.tpAbout.Text = "About";
            this.tpAbout.UseVisualStyleBackColor = true;
            // 
            // BuildTag
            // 
            this.BuildTag.AutoSize = true;
            this.BuildTag.Location = new System.Drawing.Point(5, 5);
            this.BuildTag.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.BuildTag.Name = "BuildTag";
            this.BuildTag.Size = new System.Drawing.Size(0, 13);
            this.BuildTag.TabIndex = 0;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(91, 4);
            this.button1.Margin = new System.Windows.Forms.Padding(2);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(65, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "OK";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.OkButtonClick);
            // 
            // ApplyButton
            // 
            this.ApplyButton.Enabled = false;
            this.ApplyButton.Location = new System.Drawing.Point(160, 4);
            this.ApplyButton.Margin = new System.Windows.Forms.Padding(2);
            this.ApplyButton.Name = "ApplyButton";
            this.ApplyButton.Size = new System.Drawing.Size(65, 23);
            this.ApplyButton.TabIndex = 2;
            this.ApplyButton.Text = "Apply";
            this.ApplyButton.UseVisualStyleBackColor = true;
            this.ApplyButton.Click += new System.EventHandler(this.ApplyButtonClick);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(277, 4);
            this.button3.Margin = new System.Windows.Forms.Padding(2);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(65, 23);
            this.button3.TabIndex = 3;
            this.button3.Text = "Cancel";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.CancelButtonClick);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(2);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tabControl1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.panel1);
            this.splitContainer1.Panel2MinSize = 45;
            this.splitContainer1.Size = new System.Drawing.Size(345, 348);
            this.splitContainer1.SplitterDistance = 300;
            this.splitContainer1.SplitterWidth = 3;
            this.splitContainer1.TabIndex = 4;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.button1);
            this.panel1.Controls.Add(this.ApplyButton);
            this.panel1.Controls.Add(this.button3);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 16);
            this.panel1.Margin = new System.Windows.Forms.Padding(2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(345, 29);
            this.panel1.TabIndex = 4;
            // 
            // frmPulseOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(345, 348);
            this.Controls.Add(this.splitContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "frmPulseOptions";
            this.Text = "Pulse Options";
            this.Load += new System.EventHandler(this.frmPulseOptions_Load);
            this.tabControl1.ResumeLayout(false);
            this.tpGeneral.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudTempAge)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udInterval)).EndInit();
            this.tpOutputs.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvOutputProviders)).EndInit();
            this.tpAbout.ResumeLayout(false);
            this.tpAbout.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tpGeneral;
        private System.Windows.Forms.TabPage tpOutputs;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button ApplyButton;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.TabPage tpAbout;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.CheckBox cbPrefetch;
        private System.Windows.Forms.ComboBox cbUpdateFrequencyUnit;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox cbAutoChangeonStartup;
        private System.Windows.Forms.CheckBox cbDownloadAutomatically;
        private System.Windows.Forms.Button ProviderSettings;
        private System.Windows.Forms.ComboBox cbProviders;
        private System.Windows.Forms.TextBox SearchBox;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Button btnClearNow;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown nudTempAge;
        private System.Windows.Forms.CheckBox cbDeleteOldFiles;
        private System.Windows.Forms.NumericUpDown udInterval;
        private System.Windows.Forms.Label BuildTag;
        private System.Windows.Forms.DataGridView dgvOutputProviders;
        private System.Windows.Forms.DataGridViewButtonColumn oppSettings;
    }
}