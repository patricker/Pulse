using System;using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace wallbase
{
    public partial class WallbaseProviderPrefs : UserControl, Pulse.Base.IProviderConfigurationEditor
    {

        WallbaseImageSearchSettings wiss = null;

        public WallbaseProviderPrefs()
        {
            InitializeComponent();

            cbArea.DataSource = WallbaseImageSearchSettings.SearchArea.GetSearchAreas();
            cbImageSizeType.DataSource = WallbaseImageSearchSettings.SizingOption.GetDirectionList();
            cbOrderBy.DataSource = WallbaseImageSearchSettings.OrderBy.GetOrderByList();
            cbOrderByDirection.DataSource = WallbaseImageSearchSettings.OrderByDirection.GetDirectionList();
            cbTopRange.DataSource = WallbaseImageSearchSettings.TopTimeSpan.GetTimespanList();
            cbAspectRatio.DataSource = WallbaseImageSearchSettings.AspectRatio.GetAspectRatioList();

            // Show/hide TopRange based on OrderBy
            cbOrderBy.SelectedIndexChanged += (s, e) => UpdateTopRangeVisibility();
        }

        private void UpdateTopRangeVisibility()
        {
            try
            {
                string ob = cbOrderBy.SelectedValue?.ToString();
                bool isToplist = ob == "toplist";
                cbTopRange.Visible = isToplist;
                label13.Visible = isToplist;
                if (isToplist && cbTopRange.SelectedValue == null)
                    cbTopRange.SelectedIndex = 3; // 1M default
            }
            catch { }
        }

        public void LoadConfiguration(string config)
        {
            try
            {   if(!string.IsNullOrEmpty(config))
                    wiss = WallbaseImageSearchSettings.LoadFromXML(config);
            }
            catch { }

            if (wiss == null)
                wiss = new WallbaseImageSearchSettings();

            // New API: ApiUsername + ApiKey, fallback to legacy Username/Password
            txtUserID.Text = !string.IsNullOrEmpty(wiss.ApiUsername) ? wiss.ApiUsername : wiss.Username;
            txtPassword.Text = !string.IsNullOrEmpty(wiss.ApiKey) ? wiss.ApiKey : wiss.Password;

            txtSearch.Text = wiss.Query;

            try { cbArea.SelectedValue = wiss.SA; } catch { cbArea.SelectedIndex = 0; }

            cbWG.Checked = wiss.WG;
            cbW.Checked = wiss.W;
            cbHR.Checked = wiss.HR;

            cbSFW.Checked = wiss.SFW;
            cbSketchy.Checked = wiss.SKETCHY;
            cbNSFW.Checked = wiss.NSFW;

            try { cbImageSizeType.SelectedValue = wiss.SO; } catch { cbImageSizeType.SelectedIndex = 0; }
            try { cbOrderBy.SelectedValue = wiss.OB; } catch { cbOrderBy.SelectedIndex = 0; }
            try { cbOrderByDirection.SelectedValue = wiss.OBD; } catch { cbOrderByDirection.SelectedIndex = 0; }
            try { cbTopRange.SelectedValue = string.IsNullOrEmpty(wiss.TopRange) ? "1M" : wiss.TopRange; } catch { cbTopRange.SelectedIndex = 3; }

            txtWidth.Text = wiss.ImageWidth > 0 ? wiss.ImageWidth.ToString() : "";
            txtHeight.Text = wiss.ImageHeight > 0 ? wiss.ImageHeight.ToString() : "";
            try { cbAspectRatio.SelectedValue = wiss.AR; } catch { }

            txtCollectionID.Text = wiss.CollectionID;
            txtFavoritesID.Text = wiss.FavoriteID;

            UpdateTopRangeVisibility();

            if (wiss.Color != System.Drawing.Color.Empty)
            {
                pnlColor.BackColor = wiss.Color;
                cdPicker.Color = wiss.Color;
            }

            // FIX: Unhide color search (was Visible=false in Designer, blocking designer persona)
            try
            {
                pnlColor.Visible = true;
                label8.Visible = true;
                lbPickColor.Visible = true;
                lbClearColor.Visible = true;
                // Make color panel 44x44 minimum for a11y target size
                pnlColor.Size = new System.Drawing.Size(44, 24);
                pnlColor.BorderStyle = BorderStyle.FixedSingle;
                pnlColor.Cursor = Cursors.Hand;
                pnlColor.Click += (s, e) => lbPickColor_LinkClicked(s, new LinkLabelLinkClickedEventArgs(new LinkLabel.Link()));
                // Accessible name for screen readers
                pnlColor.AccessibleName = "Selected color filter, click to pick";
                label8.AccessibleName = "Color filter";
                lbPickColor.AccessibleName = "Pick color";
                lbClearColor.AccessibleName = "Clear color";
            }
            catch { }

            // Update labels to reflect new API key auth + eye toggle for security
            try
            {
                label2.Text = "User:";
                label3.Text = "API Key:";
                label4.Text = "API Key optional. Needed for NSFW. Get key from wallhaven.cc/settings (45 req/min)";
                label4.AutoSize = false;
                label4.Height = 38;
                // Default masked, with eye toggle
                txtPassword.UseSystemPasswordChar = true;
                txtPassword.Width = 160;
                txtUserID.Width = 180;

                // Add eye toggle checkbox if not already added
                var eyeCb = this.Controls.Find("cbShowApiKey", true).FirstOrDefault() as CheckBox;
                if (eyeCb == null)
                {
                    eyeCb = new CheckBox()
                    {
                        Name = "cbShowApiKey",
                        Text = "Show",
                        AutoSize = true,
                        Location = new System.Drawing.Point(txtPassword.Right + 5, txtPassword.Top + 2),
                        TabIndex = 100,
                        AccessibleName = "Show API key"
                    };
                    eyeCb.CheckedChanged += (s, e) =>
                    {
                        txtPassword.UseSystemPasswordChar = !eyeCb.Checked;
                    };
                    // Find parent tab to add to
                    tpAuthenticate.Controls.Add(eyeCb);
                }

                // Add Get Key link if not exists
                var getKeyLink = this.Controls.Find("lbGetKey", true).FirstOrDefault() as LinkLabel;
                if (getKeyLink == null)
                {
                    getKeyLink = new LinkLabel()
                    {
                        Name = "lbGetKey",
                        Text = "Get Key...",
                        AutoSize = true,
                        Location = new System.Drawing.Point(txtPassword.Left, txtPassword.Bottom + 5),
                        TabIndex = 101,
                        AccessibleName = "Get Wallhaven API key opens browser"
                    };
                    getKeyLink.LinkClicked += (s, e) =>
                    {
                        try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("https://wallhaven.cc/settings") { UseShellExecute = true }); } catch { }
                    };
                    tpAuthenticate.Controls.Add(getKeyLink);
                }
            }
            catch { }
        }

        public string SaveConfiguration()
        {
            wiss.ApiUsername = txtUserID.Text;
            wiss.ApiKey = txtPassword.Text;
            // Keep legacy for backward compat
            wiss.Username = txtUserID.Text;
            wiss.Password = txtPassword.Text;

            wiss.Query = txtSearch.Text;

            wiss.WG = cbWG.Checked;
            wiss.W = cbW.Checked;
            wiss.HR = cbHR.Checked;

            wiss.SFW = cbSFW.Checked;
            wiss.SKETCHY = cbSketchy.Checked;
            wiss.NSFW = cbNSFW.Checked;

            wiss.SA = cbArea.SelectedValue != null ? cbArea.SelectedValue.ToString() : "search";
            wiss.SO = cbImageSizeType.SelectedValue != null ? cbImageSizeType.SelectedValue.ToString() : "gteq";
            wiss.OB = cbOrderBy.SelectedValue==null?"relevance":cbOrderBy.SelectedValue.ToString();
            wiss.OBD = cbOrderByDirection.SelectedValue==null?"desc":cbOrderByDirection.SelectedValue.ToString();
            wiss.TopRange = cbTopRange.SelectedValue==null?"1M":cbTopRange.SelectedValue.ToString();

            int w = 0, h = 0;
            int.TryParse(txtWidth.Text, out w);
            int.TryParse(txtHeight.Text, out h);
            wiss.ImageWidth = w;
            wiss.ImageHeight = h;
            wiss.AR = cbAspectRatio.SelectedValue==null?"":cbAspectRatio.SelectedValue.ToString();
            
            wiss.CollectionID = txtCollectionID.Text;
            wiss.FavoriteID = txtFavoritesID.Text;

            return wiss.Save();
        }

        private void lbPickColor_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (cdPicker.ShowDialog() == DialogResult.OK)
            {
                pnlColor.BackColor = cdPicker.Color;
                wiss.Color = cdPicker.Color;
            }
        }

        private void lbClearColor_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            pnlColor.BackColor = SystemColors.Control;
            wiss.Color = System.Drawing.Color.Empty;

        }

        private void WallbaseProviderPrefs_Load(object sender, EventArgs e)
        {
            
        }

        public bool IsOK { get; set; }

        public void HostMe(object parent)
        {
            Control c = parent as Control;

            c.Controls.Add(this);
        }

        private void cbArea_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbArea.SelectedValue == null) return;

            string val = cbArea.SelectedValue.ToString();

            if (val == "search")
                tabControl1.SelectedTab = tpSearch;
            else if (val == "toplist")
                tabControl1.SelectedTab = tpSearch;
            else if (val == "user/favorites")
                tabControl1.SelectedTab = tpFavorites;
            else if (val == "user/collection")
                tabControl1.SelectedTab = tpCollections;
        }
    }
}
