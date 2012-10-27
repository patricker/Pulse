namespace PulseForm
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
            this.lbActiveInputProviders = new System.Windows.Forms.ListView();
            this.btnRemoveInputProvider = new System.Windows.Forms.Button();
            this.btnPreview = new System.Windows.Forms.Button();
            this.btnAddInput = new System.Windows.Forms.Button();
            this.ProviderSettings = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.cbRunOnWindowsStartup = new System.Windows.Forms.CheckBox();
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
            this.ApplyButton = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.panel1 = new System.Windows.Forms.Panel();
            this.cbCheckForNewVersions = new System.Windows.Forms.CheckBox();
            this.cbProviders = new PulseForm.ProviderComboBox();
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
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
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
            this.tabControl1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(724, 510);
            this.tabControl1.TabIndex = 0;
            // 
            // tpGeneral
            // 
            this.tpGeneral.AutoScroll = true;
            this.tpGeneral.Controls.Add(this.groupBox3);
            this.tpGeneral.Controls.Add(this.groupBox2);
            this.tpGeneral.Controls.Add(this.groupBox1);
            this.tpGeneral.Location = new System.Drawing.Point(4, 25);
            this.tpGeneral.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tpGeneral.Name = "tpGeneral";
            this.tpGeneral.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tpGeneral.Size = new System.Drawing.Size(716, 481);
            this.tpGeneral.TabIndex = 0;
            this.tpGeneral.Text = "General";
            this.tpGeneral.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.panel4);
            this.groupBox3.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox3.Location = new System.Drawing.Point(9, 361);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBox3.Size = new System.Drawing.Size(428, 103);
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
            this.panel4.Location = new System.Drawing.Point(3, 29);
            this.panel4.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(422, 72);
            this.panel4.TabIndex = 0;
            // 
            // btnClearNow
            // 
            this.btnClearNow.Location = new System.Drawing.Point(7, 36);
            this.btnClearNow.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnClearNow.Name = "btnClearNow";
            this.btnClearNow.Size = new System.Drawing.Size(116, 26);
            this.btnClearNow.TabIndex = 3;
            this.btnClearNow.Text = "Clear Now";
            this.btnClearNow.UseVisualStyleBackColor = true;
            this.btnClearNow.Click += new System.EventHandler(this.ClearNowButtonClick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(271, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(71, 17);
            this.label1.TabIndex = 2;
            this.label1.Text = "day(s) old";
            // 
            // nudTempAge
            // 
            this.nudTempAge.Location = new System.Drawing.Point(208, 10);
            this.nudTempAge.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.nudTempAge.Name = "nudTempAge";
            this.nudTempAge.Size = new System.Drawing.Size(57, 23);
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
            this.cbDeleteOldFiles.Location = new System.Drawing.Point(7, 11);
            this.cbDeleteOldFiles.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cbDeleteOldFiles.Name = "cbDeleteOldFiles";
            this.cbDeleteOldFiles.Size = new System.Drawing.Size(197, 21);
            this.cbDeleteOldFiles.TabIndex = 0;
            this.cbDeleteOldFiles.Text = "Delete pictures older than ";
            this.cbDeleteOldFiles.UseVisualStyleBackColor = true;
            this.cbDeleteOldFiles.CheckedChanged += new System.EventHandler(this.CheckBoxClick);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.panel3);
            this.groupBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox2.Location = new System.Drawing.Point(7, 5);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBox2.Size = new System.Drawing.Size(431, 161);
            this.groupBox2.TabIndex = 12;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Search";
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.lbActiveInputProviders);
            this.panel3.Controls.Add(this.btnRemoveInputProvider);
            this.panel3.Controls.Add(this.btnPreview);
            this.panel3.Controls.Add(this.btnAddInput);
            this.panel3.Controls.Add(this.cbProviders);
            this.panel3.Controls.Add(this.ProviderSettings);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.panel3.Location = new System.Drawing.Point(3, 29);
            this.panel3.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(425, 130);
            this.panel3.TabIndex = 4;
            // 
            // lbActiveInputProviders
            // 
            this.lbActiveInputProviders.Location = new System.Drawing.Point(9, 30);
            this.lbActiveInputProviders.MultiSelect = false;
            this.lbActiveInputProviders.Name = "lbActiveInputProviders";
            this.lbActiveInputProviders.Size = new System.Drawing.Size(222, 91);
            this.lbActiveInputProviders.TabIndex = 14;
            this.lbActiveInputProviders.UseCompatibleStateImageBehavior = false;
            this.lbActiveInputProviders.View = System.Windows.Forms.View.SmallIcon;
            this.lbActiveInputProviders.SelectedIndexChanged += new System.EventHandler(this.lbActiveInputProviders_SelectedIndexChanged_1);
            // 
            // btnRemoveInputProvider
            // 
            this.btnRemoveInputProvider.Enabled = false;
            this.btnRemoveInputProvider.Location = new System.Drawing.Point(237, 95);
            this.btnRemoveInputProvider.Name = "btnRemoveInputProvider";
            this.btnRemoveInputProvider.Size = new System.Drawing.Size(92, 26);
            this.btnRemoveInputProvider.TabIndex = 7;
            this.btnRemoveInputProvider.Text = "Remove";
            this.btnRemoveInputProvider.UseVisualStyleBackColor = true;
            this.btnRemoveInputProvider.Click += new System.EventHandler(this.btnRemoveInputProvider_Click);
            // 
            // btnPreview
            // 
            this.btnPreview.Enabled = false;
            this.btnPreview.Location = new System.Drawing.Point(237, 65);
            this.btnPreview.Name = "btnPreview";
            this.btnPreview.Size = new System.Drawing.Size(92, 26);
            this.btnPreview.TabIndex = 6;
            this.btnPreview.Text = "Preview";
            this.btnPreview.UseVisualStyleBackColor = true;
            this.btnPreview.Click += new System.EventHandler(this.btnPreview_Click);
            // 
            // btnAddInput
            // 
            this.btnAddInput.Enabled = false;
            this.btnAddInput.Location = new System.Drawing.Point(237, 4);
            this.btnAddInput.Name = "btnAddInput";
            this.btnAddInput.Size = new System.Drawing.Size(92, 26);
            this.btnAddInput.TabIndex = 5;
            this.btnAddInput.Text = "Add";
            this.btnAddInput.UseVisualStyleBackColor = true;
            this.btnAddInput.Click += new System.EventHandler(this.btnAddInput_Click);
            // 
            // ProviderSettings
            // 
            this.ProviderSettings.Enabled = false;
            this.ProviderSettings.Location = new System.Drawing.Point(237, 34);
            this.ProviderSettings.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.ProviderSettings.Name = "ProviderSettings";
            this.ProviderSettings.Size = new System.Drawing.Size(92, 26);
            this.ProviderSettings.TabIndex = 3;
            this.ProviderSettings.Text = "Settings";
            this.ProviderSettings.UseVisualStyleBackColor = true;
            this.ProviderSettings.Click += new System.EventHandler(this.ProviderSettings_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.panel2);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(9, 170);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBox1.Size = new System.Drawing.Size(428, 187);
            this.groupBox1.TabIndex = 11;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Behavior";
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.cbCheckForNewVersions);
            this.panel2.Controls.Add(this.cbRunOnWindowsStartup);
            this.panel2.Controls.Add(this.udInterval);
            this.panel2.Controls.Add(this.cbDownloadAutomatically);
            this.panel2.Controls.Add(this.cbPrefetch);
            this.panel2.Controls.Add(this.label3);
            this.panel2.Controls.Add(this.cbAutoChangeonStartup);
            this.panel2.Controls.Add(this.cbUpdateFrequencyUnit);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.panel2.Location = new System.Drawing.Point(3, 29);
            this.panel2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(422, 156);
            this.panel2.TabIndex = 11;
            // 
            // cbRunOnWindowsStartup
            // 
            this.cbRunOnWindowsStartup.AutoSize = true;
            this.cbRunOnWindowsStartup.Location = new System.Drawing.Point(3, 104);
            this.cbRunOnWindowsStartup.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cbRunOnWindowsStartup.Name = "cbRunOnWindowsStartup";
            this.cbRunOnWindowsStartup.Size = new System.Drawing.Size(186, 21);
            this.cbRunOnWindowsStartup.TabIndex = 12;
            this.cbRunOnWindowsStartup.Text = "Run on Windows Startup";
            this.cbRunOnWindowsStartup.UseVisualStyleBackColor = true;
            this.cbRunOnWindowsStartup.CheckedChanged += new System.EventHandler(this.cbRunOnWindowsStartup_CheckedChanged);
            // 
            // udInterval
            // 
            this.udInterval.Location = new System.Drawing.Point(163, 49);
            this.udInterval.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
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
            this.udInterval.Size = new System.Drawing.Size(57, 23);
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
            this.cbDownloadAutomatically.Location = new System.Drawing.Point(3, 28);
            this.cbDownloadAutomatically.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cbDownloadAutomatically.Name = "cbDownloadAutomatically";
            this.cbDownloadAutomatically.Size = new System.Drawing.Size(187, 21);
            this.cbDownloadAutomatically.TabIndex = 5;
            this.cbDownloadAutomatically.Text = "Change Picture on Timer";
            this.cbDownloadAutomatically.UseVisualStyleBackColor = true;
            this.cbDownloadAutomatically.CheckedChanged += new System.EventHandler(this.CheckBoxClick);
            // 
            // cbPrefetch
            // 
            this.cbPrefetch.AutoSize = true;
            this.cbPrefetch.Location = new System.Drawing.Point(3, 79);
            this.cbPrefetch.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cbPrefetch.Name = "cbPrefetch";
            this.cbPrefetch.Size = new System.Drawing.Size(146, 21);
            this.cbPrefetch.TabIndex = 10;
            this.cbPrefetch.Text = "Pre Fetch Pictures";
            this.cbPrefetch.UseVisualStyleBackColor = true;
            this.cbPrefetch.CheckedChanged += new System.EventHandler(this.CheckBoxClick);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 52);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(153, 17);
            this.label3.TabIndex = 7;
            this.label3.Text = "Change Picture Every: ";
            // 
            // cbAutoChangeonStartup
            // 
            this.cbAutoChangeonStartup.AutoSize = true;
            this.cbAutoChangeonStartup.Location = new System.Drawing.Point(3, 2);
            this.cbAutoChangeonStartup.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cbAutoChangeonStartup.Name = "cbAutoChangeonStartup";
            this.cbAutoChangeonStartup.Size = new System.Drawing.Size(197, 21);
            this.cbAutoChangeonStartup.TabIndex = 6;
            this.cbAutoChangeonStartup.Text = "Change Picture on Startup";
            this.cbAutoChangeonStartup.UseVisualStyleBackColor = true;
            this.cbAutoChangeonStartup.CheckedChanged += new System.EventHandler(this.CheckBoxClick);
            // 
            // cbUpdateFrequencyUnit
            // 
            this.cbUpdateFrequencyUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbUpdateFrequencyUnit.FormattingEnabled = true;
            this.cbUpdateFrequencyUnit.Location = new System.Drawing.Point(235, 49);
            this.cbUpdateFrequencyUnit.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cbUpdateFrequencyUnit.Name = "cbUpdateFrequencyUnit";
            this.cbUpdateFrequencyUnit.Size = new System.Drawing.Size(108, 24);
            this.cbUpdateFrequencyUnit.TabIndex = 9;
            this.cbUpdateFrequencyUnit.SelectedIndexChanged += new System.EventHandler(this.ComboBoxSelectionChanged);
            // 
            // tpOutputs
            // 
            this.tpOutputs.AutoScroll = true;
            this.tpOutputs.Controls.Add(this.dgvOutputProviders);
            this.tpOutputs.Location = new System.Drawing.Point(4, 25);
            this.tpOutputs.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tpOutputs.Name = "tpOutputs";
            this.tpOutputs.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tpOutputs.Size = new System.Drawing.Size(716, 481);
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
            this.dgvOutputProviders.Location = new System.Drawing.Point(3, 2);
            this.dgvOutputProviders.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.dgvOutputProviders.MultiSelect = false;
            this.dgvOutputProviders.Name = "dgvOutputProviders";
            this.dgvOutputProviders.RowHeadersVisible = false;
            this.dgvOutputProviders.RowTemplate.Height = 28;
            this.dgvOutputProviders.Size = new System.Drawing.Size(710, 477);
            this.dgvOutputProviders.TabIndex = 0;
            this.dgvOutputProviders.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvOutputProviders_CellClick);
            this.dgvOutputProviders.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvOutputProviders_CellValueChanged);
            // 
            // oppSettings
            // 
            this.oppSettings.HeaderText = "Settings";
            this.oppSettings.Name = "oppSettings";
            this.oppSettings.Text = "Change";
            this.oppSettings.Width = 65;
            // 
            // tpAbout
            // 
            this.tpAbout.Controls.Add(this.BuildTag);
            this.tpAbout.Location = new System.Drawing.Point(4, 25);
            this.tpAbout.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tpAbout.Name = "tpAbout";
            this.tpAbout.Size = new System.Drawing.Size(716, 481);
            this.tpAbout.TabIndex = 2;
            this.tpAbout.Text = "About";
            this.tpAbout.UseVisualStyleBackColor = true;
            // 
            // BuildTag
            // 
            this.BuildTag.AutoSize = true;
            this.BuildTag.Location = new System.Drawing.Point(7, 6);
            this.BuildTag.Name = "BuildTag";
            this.BuildTag.Size = new System.Drawing.Size(0, 17);
            this.BuildTag.TabIndex = 0;
            // 
            // ApplyButton
            // 
            this.ApplyButton.Enabled = false;
            this.ApplyButton.Location = new System.Drawing.Point(272, 5);
            this.ApplyButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.ApplyButton.Name = "ApplyButton";
            this.ApplyButton.Size = new System.Drawing.Size(87, 28);
            this.ApplyButton.TabIndex = 1;
            this.ApplyButton.Text = "OK";
            this.ApplyButton.UseVisualStyleBackColor = true;
            this.ApplyButton.Click += new System.EventHandler(this.OkButtonClick);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(369, 5);
            this.button3.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(87, 28);
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
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
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
            this.splitContainer1.Size = new System.Drawing.Size(724, 559);
            this.splitContainer1.SplitterDistance = 510;
            this.splitContainer1.TabIndex = 4;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.ApplyButton);
            this.panel1.Controls.Add(this.button3);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 9);
            this.panel1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(724, 36);
            this.panel1.TabIndex = 4;
            // 
            // cbCheckForNewVersions
            // 
            this.cbCheckForNewVersions.AutoSize = true;
            this.cbCheckForNewVersions.Location = new System.Drawing.Point(3, 130);
            this.cbCheckForNewVersions.Name = "cbCheckForNewVersions";
            this.cbCheckForNewVersions.Size = new System.Drawing.Size(244, 21);
            this.cbCheckForNewVersions.TabIndex = 13;
            this.cbCheckForNewVersions.Text = "Check for new versions on startup\r\n";
            this.cbCheckForNewVersions.UseVisualStyleBackColor = true;
            // 
            // cbProviders
            // 
            this.cbProviders.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cbProviders.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbProviders.FormattingEnabled = true;
            this.cbProviders.Location = new System.Drawing.Point(9, 4);
            this.cbProviders.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cbProviders.Name = "cbProviders";
            this.cbProviders.Size = new System.Drawing.Size(222, 24);
            this.cbProviders.TabIndex = 2;
            this.cbProviders.SelectedIndexChanged += new System.EventHandler(this.cbProviders_SelectedIndexChanged);
            // 
            // frmPulseOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(724, 559);
            this.Controls.Add(this.splitContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
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
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tpGeneral;
        private System.Windows.Forms.TabPage tpOutputs;
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
        private ProviderComboBox cbProviders;
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
        private System.Windows.Forms.CheckBox cbRunOnWindowsStartup;
        private System.Windows.Forms.DataGridViewButtonColumn oppSettings;
        private System.Windows.Forms.Button btnAddInput;
        private System.Windows.Forms.Button btnPreview;
        private System.Windows.Forms.Button btnRemoveInputProvider;
        private System.Windows.Forms.ListView lbActiveInputProviders;
        private System.Windows.Forms.CheckBox cbCheckForNewVersions;
    }
}