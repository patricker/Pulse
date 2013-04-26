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
            this.tpAuthenticate = new System.Windows.Forms.TabPage();
            this.tpSearch = new System.Windows.Forms.TabPage();
            this.label12 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.lbClearColor = new System.Windows.Forms.LinkLabel();
            this.lbPickColor = new System.Windows.Forms.LinkLabel();
            this.pnlColor = new System.Windows.Forms.Panel();
            this.label8 = new System.Windows.Forms.Label();
            this.cbOrderByDirection = new System.Windows.Forms.ComboBox();
            this.cbOrderBy = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.tpCollections = new System.Windows.Forms.TabPage();
            this.txtCollectionID = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.tpFavorites = new System.Windows.Forms.TabPage();
            this.txtFavoritesID = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.cdPicker = new System.Windows.Forms.ColorDialog();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tpAuthenticate.SuspendLayout();
            this.tpSearch.SuspendLayout();
            this.tpCollections.SuspendLayout();
            this.tpFavorites.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 11);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(88, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "Image Area: ";
            // 
            // cbArea
            // 
            this.cbArea.DisplayMember = "Name";
            this.cbArea.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbArea.FormattingEnabled = true;
            this.cbArea.Location = new System.Drawing.Point(98, 11);
            this.cbArea.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cbArea.Name = "cbArea";
            this.cbArea.Size = new System.Drawing.Size(143, 24);
            this.cbArea.TabIndex = 1;
            this.cbArea.ValueMember = "Value";
            this.cbArea.SelectedIndexChanged += new System.EventHandler(this.cbArea_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 47);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(51, 17);
            this.label2.TabIndex = 2;
            this.label2.Text = "Login: ";
            // 
            // txtUserID
            // 
            this.txtUserID.Location = new System.Drawing.Point(58, 47);
            this.txtUserID.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtUserID.Name = "txtUserID";
            this.txtUserID.Size = new System.Drawing.Size(89, 22);
            this.txtUserID.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 76);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(47, 17);
            this.label3.TabIndex = 4;
            this.label3.Text = "Pass: ";
            // 
            // txtPassword
            // 
            this.txtPassword.Location = new System.Drawing.Point(58, 73);
            this.txtPassword.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.Size = new System.Drawing.Size(89, 22);
            this.txtPassword.TabIndex = 5;
            this.txtPassword.UseSystemPasswordChar = true;
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(5, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(342, 34);
            this.label4.TabIndex = 6;
            this.label4.Text = "Authentication is optional.  Authentication enables NSFW and Favorites.";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cbHR);
            this.groupBox1.Controls.Add(this.cbW);
            this.groupBox1.Controls.Add(this.cbWG);
            this.groupBox1.Location = new System.Drawing.Point(0, 43);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBox1.Size = new System.Drawing.Size(161, 49);
            this.groupBox1.TabIndex = 7;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Categories";
            // 
            // cbHR
            // 
            this.cbHR.AutoSize = true;
            this.cbHR.Location = new System.Drawing.Point(104, 22);
            this.cbHR.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cbHR.Name = "cbHR";
            this.cbHR.Size = new System.Drawing.Size(50, 21);
            this.cbHR.TabIndex = 9;
            this.cbHR.Text = "HR";
            this.cbHR.UseVisualStyleBackColor = true;
            // 
            // cbW
            // 
            this.cbW.AutoSize = true;
            this.cbW.Location = new System.Drawing.Point(60, 22);
            this.cbW.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cbW.Name = "cbW";
            this.cbW.Size = new System.Drawing.Size(43, 21);
            this.cbW.TabIndex = 10;
            this.cbW.Text = "W";
            this.cbW.UseVisualStyleBackColor = true;
            // 
            // cbWG
            // 
            this.cbWG.AutoSize = true;
            this.cbWG.Location = new System.Drawing.Point(7, 22);
            this.cbWG.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cbWG.Name = "cbWG";
            this.cbWG.Size = new System.Drawing.Size(54, 21);
            this.cbWG.TabIndex = 11;
            this.cbWG.Text = "WG";
            this.cbWG.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.cbNSFW);
            this.groupBox2.Controls.Add(this.cbSketchy);
            this.groupBox2.Controls.Add(this.cbSFW);
            this.groupBox2.Location = new System.Drawing.Point(166, 43);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBox2.Size = new System.Drawing.Size(255, 49);
            this.groupBox2.TabIndex = 8;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Purity";
            // 
            // cbNSFW
            // 
            this.cbNSFW.AutoSize = true;
            this.cbNSFW.Location = new System.Drawing.Point(176, 22);
            this.cbNSFW.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cbNSFW.Name = "cbNSFW";
            this.cbNSFW.Size = new System.Drawing.Size(70, 21);
            this.cbNSFW.TabIndex = 2;
            this.cbNSFW.Text = "NSFW";
            this.cbNSFW.UseVisualStyleBackColor = true;
            // 
            // cbSketchy
            // 
            this.cbSketchy.AutoSize = true;
            this.cbSketchy.Location = new System.Drawing.Point(75, 22);
            this.cbSketchy.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cbSketchy.Name = "cbSketchy";
            this.cbSketchy.Size = new System.Drawing.Size(94, 21);
            this.cbSketchy.TabIndex = 1;
            this.cbSketchy.Text = "SKETCHY";
            this.cbSketchy.UseVisualStyleBackColor = true;
            // 
            // cbSFW
            // 
            this.cbSFW.AutoSize = true;
            this.cbSFW.Location = new System.Drawing.Point(7, 22);
            this.cbSFW.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cbSFW.Name = "cbSFW";
            this.cbSFW.Size = new System.Drawing.Size(60, 21);
            this.cbSFW.TabIndex = 0;
            this.cbSFW.Text = "SFW";
            this.cbSFW.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.txtHeight);
            this.groupBox3.Controls.Add(this.label6);
            this.groupBox3.Controls.Add(this.txtWidth);
            this.groupBox3.Controls.Add(this.cbImageSizeType);
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Location = new System.Drawing.Point(4, 97);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBox3.Size = new System.Drawing.Size(417, 50);
            this.groupBox3.TabIndex = 9;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Resolution";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(294, 19);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(14, 17);
            this.label6.TabIndex = 4;
            this.label6.Text = "x";
            // 
            // txtHeight
            // 
            this.txtHeight.Location = new System.Drawing.Point(307, 17);
            this.txtHeight.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtHeight.Name = "txtHeight";
            this.txtHeight.Size = new System.Drawing.Size(73, 22);
            this.txtHeight.TabIndex = 3;
            // 
            // txtWidth
            // 
            this.txtWidth.Location = new System.Drawing.Point(221, 17);
            this.txtWidth.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtWidth.Name = "txtWidth";
            this.txtWidth.Size = new System.Drawing.Size(73, 22);
            this.txtWidth.TabIndex = 2;
            // 
            // cbImageSizeType
            // 
            this.cbImageSizeType.DisplayMember = "Name";
            this.cbImageSizeType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbImageSizeType.FormattingEnabled = true;
            this.cbImageSizeType.Location = new System.Drawing.Point(108, 17);
            this.cbImageSizeType.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cbImageSizeType.Name = "cbImageSizeType";
            this.cbImageSizeType.Size = new System.Drawing.Size(108, 24);
            this.cbImageSizeType.TabIndex = 1;
            this.cbImageSizeType.ValueMember = "Value";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(18, 19);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(85, 17);
            this.label5.TabIndex = 0;
            this.label5.Text = "Image Size: ";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tpAuthenticate);
            this.tabControl1.Controls.Add(this.tpSearch);
            this.tabControl1.Controls.Add(this.tpCollections);
            this.tabControl1.Controls.Add(this.tpFavorites);
            this.tabControl1.Location = new System.Drawing.Point(4, 151);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(421, 168);
            this.tabControl1.TabIndex = 10;
            // 
            // tpAuthenticate
            // 
            this.tpAuthenticate.Controls.Add(this.label4);
            this.tpAuthenticate.Controls.Add(this.label2);
            this.tpAuthenticate.Controls.Add(this.txtUserID);
            this.tpAuthenticate.Controls.Add(this.label3);
            this.tpAuthenticate.Controls.Add(this.txtPassword);
            this.tpAuthenticate.Location = new System.Drawing.Point(4, 25);
            this.tpAuthenticate.Name = "tpAuthenticate";
            this.tpAuthenticate.Size = new System.Drawing.Size(413, 139);
            this.tpAuthenticate.TabIndex = 3;
            this.tpAuthenticate.Text = "Authentication";
            this.tpAuthenticate.UseVisualStyleBackColor = true;
            // 
            // tpSearch
            // 
            this.tpSearch.Controls.Add(this.label12);
            this.tpSearch.Controls.Add(this.label11);
            this.tpSearch.Controls.Add(this.txtSearch);
            this.tpSearch.Controls.Add(this.lbClearColor);
            this.tpSearch.Controls.Add(this.lbPickColor);
            this.tpSearch.Controls.Add(this.pnlColor);
            this.tpSearch.Controls.Add(this.label8);
            this.tpSearch.Controls.Add(this.cbOrderByDirection);
            this.tpSearch.Controls.Add(this.cbOrderBy);
            this.tpSearch.Controls.Add(this.label7);
            this.tpSearch.Location = new System.Drawing.Point(4, 25);
            this.tpSearch.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tpSearch.Name = "tpSearch";
            this.tpSearch.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tpSearch.Size = new System.Drawing.Size(413, 139);
            this.tpSearch.TabIndex = 0;
            this.tpSearch.Text = "Search";
            this.tpSearch.UseVisualStyleBackColor = true;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.Location = new System.Drawing.Point(37, 50);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(179, 17);
            this.label12.TabIndex = 13;
            this.label12.Text = "Search Query OR Color";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(6, 70);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(55, 17);
            this.label11.TabIndex = 12;
            this.label11.Text = "Query: ";
            // 
            // txtSearch
            // 
            this.txtSearch.Location = new System.Drawing.Point(78, 70);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(192, 22);
            this.txtSearch.TabIndex = 11;
            // 
            // lbClearColor
            // 
            this.lbClearColor.AutoSize = true;
            this.lbClearColor.Location = new System.Drawing.Point(192, 109);
            this.lbClearColor.Name = "lbClearColor";
            this.lbClearColor.Size = new System.Drawing.Size(78, 17);
            this.lbClearColor.TabIndex = 6;
            this.lbClearColor.TabStop = true;
            this.lbClearColor.Text = "Clear Color";
            this.lbClearColor.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lbClearColor_LinkClicked);
            // 
            // lbPickColor
            // 
            this.lbPickColor.AutoSize = true;
            this.lbPickColor.Location = new System.Drawing.Point(112, 109);
            this.lbPickColor.Name = "lbPickColor";
            this.lbPickColor.Size = new System.Drawing.Size(71, 17);
            this.lbPickColor.TabIndex = 5;
            this.lbPickColor.TabStop = true;
            this.lbPickColor.Text = "Pick Color";
            this.lbPickColor.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lbPickColor_LinkClicked);
            // 
            // pnlColor
            // 
            this.pnlColor.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pnlColor.Location = new System.Drawing.Point(78, 102);
            this.pnlColor.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pnlColor.Name = "pnlColor";
            this.pnlColor.Size = new System.Drawing.Size(29, 23);
            this.pnlColor.TabIndex = 4;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(6, 109);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(49, 17);
            this.label8.TabIndex = 3;
            this.label8.Text = "Color: ";
            // 
            // cbOrderByDirection
            // 
            this.cbOrderByDirection.DisplayMember = "Name";
            this.cbOrderByDirection.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbOrderByDirection.FormattingEnabled = true;
            this.cbOrderByDirection.Location = new System.Drawing.Point(197, 6);
            this.cbOrderByDirection.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cbOrderByDirection.Name = "cbOrderByDirection";
            this.cbOrderByDirection.Size = new System.Drawing.Size(108, 24);
            this.cbOrderByDirection.TabIndex = 2;
            this.cbOrderByDirection.ValueMember = "Value";
            // 
            // cbOrderBy
            // 
            this.cbOrderBy.DisplayMember = "Name";
            this.cbOrderBy.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbOrderBy.FormattingEnabled = true;
            this.cbOrderBy.Location = new System.Drawing.Point(79, 6);
            this.cbOrderBy.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cbOrderBy.Name = "cbOrderBy";
            this.cbOrderBy.Size = new System.Drawing.Size(108, 24);
            this.cbOrderBy.TabIndex = 1;
            this.cbOrderBy.ValueMember = "Value";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(3, 6);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(73, 17);
            this.label7.TabIndex = 0;
            this.label7.Text = "Order By: ";
            // 
            // tpCollections
            // 
            this.tpCollections.Controls.Add(this.txtCollectionID);
            this.tpCollections.Controls.Add(this.label9);
            this.tpCollections.Location = new System.Drawing.Point(4, 25);
            this.tpCollections.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tpCollections.Name = "tpCollections";
            this.tpCollections.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tpCollections.Size = new System.Drawing.Size(413, 139);
            this.tpCollections.TabIndex = 1;
            this.tpCollections.Text = "Collections";
            this.tpCollections.UseVisualStyleBackColor = true;
            // 
            // txtCollectionID
            // 
            this.txtCollectionID.Location = new System.Drawing.Point(100, 4);
            this.txtCollectionID.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtCollectionID.Name = "txtCollectionID";
            this.txtCollectionID.Size = new System.Drawing.Size(133, 22);
            this.txtCollectionID.TabIndex = 1;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(7, 6);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(94, 17);
            this.label9.TabIndex = 0;
            this.label9.Text = "Collection ID: ";
            // 
            // tpFavorites
            // 
            this.tpFavorites.Controls.Add(this.txtFavoritesID);
            this.tpFavorites.Controls.Add(this.label10);
            this.tpFavorites.Location = new System.Drawing.Point(4, 25);
            this.tpFavorites.Margin = new System.Windows.Forms.Padding(4);
            this.tpFavorites.Name = "tpFavorites";
            this.tpFavorites.Padding = new System.Windows.Forms.Padding(4);
            this.tpFavorites.Size = new System.Drawing.Size(413, 139);
            this.tpFavorites.TabIndex = 2;
            this.tpFavorites.Text = "Favorites";
            this.tpFavorites.UseVisualStyleBackColor = true;
            // 
            // txtFavoritesID
            // 
            this.txtFavoritesID.Location = new System.Drawing.Point(100, 1);
            this.txtFavoritesID.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtFavoritesID.Name = "txtFavoritesID";
            this.txtFavoritesID.Size = new System.Drawing.Size(133, 22);
            this.txtFavoritesID.TabIndex = 3;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(7, 4);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(91, 17);
            this.label10.TabIndex = 2;
            this.label10.Text = "Favorites ID: ";
            // 
            // cdPicker
            // 
            this.cdPicker.AnyColor = true;
            this.cdPicker.FullOpen = true;
            // 
            // WallbaseProviderPrefs
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.cbArea);
            this.Controls.Add(this.label1);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "WallbaseProviderPrefs";
            this.Size = new System.Drawing.Size(486, 321);
            this.Load += new System.EventHandler(this.WallbaseProviderPrefs_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tpAuthenticate.ResumeLayout(false);
            this.tpAuthenticate.PerformLayout();
            this.tpSearch.ResumeLayout(false);
            this.tpSearch.PerformLayout();
            this.tpCollections.ResumeLayout(false);
            this.tpCollections.PerformLayout();
            this.tpFavorites.ResumeLayout(false);
            this.tpFavorites.PerformLayout();
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
        private System.Windows.Forms.TabPage tpSearch;
        private System.Windows.Forms.ComboBox cbOrderByDirection;
        private System.Windows.Forms.ComboBox cbOrderBy;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TabPage tpCollections;
        private System.Windows.Forms.Panel pnlColor;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ColorDialog cdPicker;
        private System.Windows.Forms.LinkLabel lbClearColor;
        private System.Windows.Forms.LinkLabel lbPickColor;
        private System.Windows.Forms.TextBox txtCollectionID;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TabPage tpFavorites;
        private System.Windows.Forms.TextBox txtFavoritesID;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.TabPage tpAuthenticate;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label11;
    }
}
