namespace wallbase
{
    partial class WallbaseProviderPrefs
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.cbArea = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtUserID = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cbHR = new System.Windows.Forms.CheckBox();
            this.cbW = new System.Windows.Forms.CheckBox();
            this.cbWG = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.cbNSFW = new System.Windows.Forms.CheckBox();
            this.cbSketchy = new System.Windows.Forms.CheckBox();
            this.cbSFW = new System.Windows.Forms.CheckBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txtHeight = new System.Windows.Forms.TextBox();
            this.txtWidth = new System.Windows.Forms.TextBox();
            this.cbImageSizeType = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.lbClearColor = new System.Windows.Forms.LinkLabel();
            this.lbPickColor = new System.Windows.Forms.LinkLabel();
            this.pnlColor = new System.Windows.Forms.Panel();
            this.label8 = new System.Windows.Forms.Label();
            this.cbOrderByDirection = new System.Windows.Forms.ComboBox();
            this.cbOrderBy = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.txtCollectionID = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.cdPicker = new System.Windows.Forms.ColorDialog();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 67);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(100, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "Image Area: ";
            // 
            // cbArea
            // 
            this.cbArea.DisplayMember = "Name";
            this.cbArea.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbArea.FormattingEnabled = true;
            this.cbArea.Location = new System.Drawing.Point(114, 67);
            this.cbArea.Name = "cbArea";
            this.cbArea.Size = new System.Drawing.Size(160, 28);
            this.cbArea.TabIndex = 1;
            this.cbArea.ValueMember = "Value";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 27);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(56, 20);
            this.label2.TabIndex = 2;
            this.label2.Text = "Login: ";
            // 
            // txtUserID
            // 
            this.txtUserID.Location = new System.Drawing.Point(65, 27);
            this.txtUserID.Name = "txtUserID";
            this.txtUserID.Size = new System.Drawing.Size(100, 26);
            this.txtUserID.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(171, 27);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(52, 20);
            this.label3.TabIndex = 4;
            this.label3.Text = "Pass: ";
            // 
            // txtPassword
            // 
            this.txtPassword.Location = new System.Drawing.Point(229, 27);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.Size = new System.Drawing.Size(100, 26);
            this.txtPassword.TabIndex = 5;
            this.txtPassword.UseSystemPasswordChar = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(4, 4);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(384, 20);
            this.label4.TabIndex = 6;
            this.label4.Text = "Authentication is optional but enables some features.";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cbHR);
            this.groupBox1.Controls.Add(this.cbW);
            this.groupBox1.Controls.Add(this.cbWG);
            this.groupBox1.Location = new System.Drawing.Point(3, 107);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(182, 62);
            this.groupBox1.TabIndex = 7;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Categories";
            // 
            // cbHR
            // 
            this.cbHR.AutoSize = true;
            this.cbHR.Location = new System.Drawing.Point(117, 27);
            this.cbHR.Name = "cbHR";
            this.cbHR.Size = new System.Drawing.Size(59, 24);
            this.cbHR.TabIndex = 9;
            this.cbHR.Text = "HR";
            this.cbHR.UseVisualStyleBackColor = true;
            // 
            // cbW
            // 
            this.cbW.AutoSize = true;
            this.cbW.Location = new System.Drawing.Point(68, 27);
            this.cbW.Name = "cbW";
            this.cbW.Size = new System.Drawing.Size(50, 24);
            this.cbW.TabIndex = 10;
            this.cbW.Text = "W";
            this.cbW.UseVisualStyleBackColor = true;
            // 
            // cbWG
            // 
            this.cbWG.AutoSize = true;
            this.cbWG.Location = new System.Drawing.Point(7, 27);
            this.cbWG.Name = "cbWG";
            this.cbWG.Size = new System.Drawing.Size(63, 24);
            this.cbWG.TabIndex = 11;
            this.cbWG.Text = "WG";
            this.cbWG.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.cbNSFW);
            this.groupBox2.Controls.Add(this.cbSketchy);
            this.groupBox2.Controls.Add(this.cbSFW);
            this.groupBox2.Location = new System.Drawing.Point(191, 107);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(286, 62);
            this.groupBox2.TabIndex = 8;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Purity";
            // 
            // cbNSFW
            // 
            this.cbNSFW.AutoSize = true;
            this.cbNSFW.Enabled = false;
            this.cbNSFW.Location = new System.Drawing.Point(198, 27);
            this.cbNSFW.Name = "cbNSFW";
            this.cbNSFW.Size = new System.Drawing.Size(82, 24);
            this.cbNSFW.TabIndex = 2;
            this.cbNSFW.Text = "NSFW";
            this.cbNSFW.UseVisualStyleBackColor = true;
            // 
            // cbSketchy
            // 
            this.cbSketchy.AutoSize = true;
            this.cbSketchy.Location = new System.Drawing.Point(84, 27);
            this.cbSketchy.Name = "cbSketchy";
            this.cbSketchy.Size = new System.Drawing.Size(110, 24);
            this.cbSketchy.TabIndex = 1;
            this.cbSketchy.Text = "SKETCHY";
            this.cbSketchy.UseVisualStyleBackColor = true;
            // 
            // cbSFW
            // 
            this.cbSFW.AutoSize = true;
            this.cbSFW.Location = new System.Drawing.Point(7, 27);
            this.cbSFW.Name = "cbSFW";
            this.cbSFW.Size = new System.Drawing.Size(71, 24);
            this.cbSFW.TabIndex = 0;
            this.cbSFW.Text = "SFW";
            this.cbSFW.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.label6);
            this.groupBox3.Controls.Add(this.txtHeight);
            this.groupBox3.Controls.Add(this.txtWidth);
            this.groupBox3.Controls.Add(this.cbImageSizeType);
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Location = new System.Drawing.Point(8, 176);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(469, 63);
            this.groupBox3.TabIndex = 9;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Resolution";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(317, 22);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(16, 20);
            this.label6.TabIndex = 4;
            this.label6.Text = "x";
            // 
            // txtHeight
            // 
            this.txtHeight.Location = new System.Drawing.Point(338, 19);
            this.txtHeight.Name = "txtHeight";
            this.txtHeight.Size = new System.Drawing.Size(82, 26);
            this.txtHeight.TabIndex = 3;
            // 
            // txtWidth
            // 
            this.txtWidth.Location = new System.Drawing.Point(234, 19);
            this.txtWidth.Name = "txtWidth";
            this.txtWidth.Size = new System.Drawing.Size(82, 26);
            this.txtWidth.TabIndex = 2;
            // 
            // cbImageSizeType
            // 
            this.cbImageSizeType.DisplayMember = "Name";
            this.cbImageSizeType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbImageSizeType.FormattingEnabled = true;
            this.cbImageSizeType.Location = new System.Drawing.Point(106, 19);
            this.cbImageSizeType.Name = "cbImageSizeType";
            this.cbImageSizeType.Size = new System.Drawing.Size(121, 28);
            this.cbImageSizeType.TabIndex = 1;
            this.cbImageSizeType.ValueMember = "Value";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 22);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(97, 20);
            this.label5.TabIndex = 0;
            this.label5.Text = "Image Size: ";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(3, 245);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(474, 143);
            this.tabControl1.TabIndex = 10;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.lbClearColor);
            this.tabPage1.Controls.Add(this.lbPickColor);
            this.tabPage1.Controls.Add(this.pnlColor);
            this.tabPage1.Controls.Add(this.label8);
            this.tabPage1.Controls.Add(this.cbOrderByDirection);
            this.tabPage1.Controls.Add(this.cbOrderBy);
            this.tabPage1.Controls.Add(this.label7);
            this.tabPage1.Location = new System.Drawing.Point(4, 29);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(466, 110);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Search";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // lbClearColor
            // 
            this.lbClearColor.AutoSize = true;
            this.lbClearColor.Location = new System.Drawing.Point(218, 60);
            this.lbClearColor.Name = "lbClearColor";
            this.lbClearColor.Size = new System.Drawing.Size(87, 20);
            this.lbClearColor.TabIndex = 6;
            this.lbClearColor.TabStop = true;
            this.lbClearColor.Text = "Clear Color";
            this.lbClearColor.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lbClearColor_LinkClicked);
            // 
            // lbPickColor
            // 
            this.lbPickColor.AutoSize = true;
            this.lbPickColor.Location = new System.Drawing.Point(127, 60);
            this.lbPickColor.Name = "lbPickColor";
            this.lbPickColor.Size = new System.Drawing.Size(79, 20);
            this.lbPickColor.TabIndex = 5;
            this.lbPickColor.TabStop = true;
            this.lbPickColor.Text = "Pick Color";
            this.lbPickColor.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lbPickColor_LinkClicked);
            // 
            // pnlColor
            // 
            this.pnlColor.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pnlColor.Location = new System.Drawing.Point(88, 51);
            this.pnlColor.Name = "pnlColor";
            this.pnlColor.Size = new System.Drawing.Size(33, 29);
            this.pnlColor.TabIndex = 4;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(7, 60);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(54, 20);
            this.label8.TabIndex = 3;
            this.label8.Text = "Color: ";
            // 
            // cbOrderByDirection
            // 
            this.cbOrderByDirection.DisplayMember = "Name";
            this.cbOrderByDirection.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbOrderByDirection.FormattingEnabled = true;
            this.cbOrderByDirection.Location = new System.Drawing.Point(222, 7);
            this.cbOrderByDirection.Name = "cbOrderByDirection";
            this.cbOrderByDirection.Size = new System.Drawing.Size(121, 28);
            this.cbOrderByDirection.TabIndex = 2;
            this.cbOrderByDirection.ValueMember = "Value";
            // 
            // cbOrderBy
            // 
            this.cbOrderBy.DisplayMember = "Name";
            this.cbOrderBy.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbOrderBy.FormattingEnabled = true;
            this.cbOrderBy.Location = new System.Drawing.Point(88, 7);
            this.cbOrderBy.Name = "cbOrderBy";
            this.cbOrderBy.Size = new System.Drawing.Size(121, 28);
            this.cbOrderBy.TabIndex = 1;
            this.cbOrderBy.ValueMember = "Value";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(3, 7);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(79, 20);
            this.label7.TabIndex = 0;
            this.label7.Text = "Order By: ";
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.txtCollectionID);
            this.tabPage2.Controls.Add(this.label9);
            this.tabPage2.Location = new System.Drawing.Point(4, 29);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(441, 110);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Collections";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // txtCollectionID
            // 
            this.txtCollectionID.Location = new System.Drawing.Point(113, 4);
            this.txtCollectionID.Name = "txtCollectionID";
            this.txtCollectionID.Size = new System.Drawing.Size(149, 26);
            this.txtCollectionID.TabIndex = 1;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(7, 7);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(107, 20);
            this.label9.TabIndex = 0;
            this.label9.Text = "Collection ID: ";
            // 
            // cdPicker
            // 
            this.cdPicker.AnyColor = true;
            this.cdPicker.FullOpen = true;
            // 
            // WallbaseProviderPrefs
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtPassword);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtUserID);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cbArea);
            this.Controls.Add(this.label1);
            this.Name = "WallbaseProviderPrefs";
            this.Size = new System.Drawing.Size(480, 401);
            this.Load += new System.EventHandler(this.WallbaseProviderPrefs_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cbArea;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtUserID;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox cbHR;
        private System.Windows.Forms.CheckBox cbW;
        private System.Windows.Forms.CheckBox cbWG;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox cbNSFW;
        private System.Windows.Forms.CheckBox cbSketchy;
        private System.Windows.Forms.CheckBox cbSFW;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtHeight;
        private System.Windows.Forms.TextBox txtWidth;
        private System.Windows.Forms.ComboBox cbImageSizeType;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.ComboBox cbOrderByDirection;
        private System.Windows.Forms.ComboBox cbOrderBy;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Panel pnlColor;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ColorDialog cdPicker;
        private System.Windows.Forms.LinkLabel lbClearColor;
        private System.Windows.Forms.LinkLabel lbPickColor;
        private System.Windows.Forms.TextBox txtCollectionID;
        private System.Windows.Forms.Label label9;
    }
}
