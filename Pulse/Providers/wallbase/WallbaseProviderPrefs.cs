using System;using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace wallhaven
{
    public partial class WallbaseProviderPrefs : UserControl, Pulse.Base.IProviderConfigurationEditor
    {

        WallhavenImageSearchSettings wiss = null;

        public WallbaseProviderPrefs()
        {
            InitializeComponent();

            cbArea.DataSource = WallhavenImageSearchSettings.SearchArea.GetSearchAreas();
            cbImageSizeType.DataSource = WallhavenImageSearchSettings.SizingOption.GetSizingList();
            cbColor.DataSource = WallhavenImageSearchSettings.ColorList.GetColors();
            cbOrderBy.DataSource = WallhavenImageSearchSettings.OrderBy.GetOrderByList();
            cbOrderByDirection.DataSource = WallhavenImageSearchSettings.OrderByDirection.GetDirectionList();
            cbTopRange.DataSource = WallhavenImageSearchSettings.TopTimeSpan.GetTimespanList();
            cbAspectRatio.DataSource = WallhavenImageSearchSettings.AspectRatio.GetAspectRatioList();
        }

        public void LoadConfiguration(string config)
        {
            try
            {   if(!string.IsNullOrEmpty(config))
                    wiss = WallhavenImageSearchSettings.LoadFromXML(config);
            }
            catch { }

            if (wiss == null)
                wiss = new WallhavenImageSearchSettings();

            txtAPIKey.Text = wiss.APIKey;

            txtSearch.Text = wiss.Query;

            cbArea.SelectedValue = wiss.SearchType;

            cbTopRange.SelectedValue = wiss.TopRange;

            cbWG.Checked = wiss.General;
            cbW.Checked = wiss.Anime;
            cbHR.Checked = wiss.People;

            cbSFW.Checked = wiss.SFW;
            cbSketchy.Checked = wiss.SKETCHY;
            cbNSFW.Checked = wiss.NSFW;

            cbImageSizeType.SelectedValue = wiss.SO;
            cbOrderBy.SelectedValue = wiss.OB;
            cbOrderByDirection.SelectedValue = wiss.OBD;

            txtWidth.Text = wiss.ImageWidth.ToString();
            txtHeight.Text = wiss.ImageHeight.ToString();
            cbAspectRatio.SelectedValue = wiss.AR;

            txtCollectionID.Text = wiss.CollectionID;
            txtFavoritesID.Text = wiss.FavoriteID;

            cbColor.SelectedText = wiss.Color;
        }

        public string SaveConfiguration()
        {
            wiss.APIKey = txtAPIKey.Text;

            wiss.Query = txtSearch.Text;

            wiss.TopRange = cbTopRange.SelectedValue.ToString();

            wiss.General = cbWG.Checked;
            wiss.Anime = cbW.Checked;
            wiss.People = cbHR.Checked;

            wiss.SFW = cbSFW.Checked;
            wiss.SKETCHY = cbSketchy.Checked;
            wiss.NSFW = cbNSFW.Checked;

            wiss.SearchType = cbArea.SelectedValue.ToString();
            wiss.SO = cbImageSizeType.SelectedValue.ToString();
            wiss.OB = cbOrderBy.SelectedValue==null?"":cbOrderBy.SelectedValue.ToString();
            wiss.OBD = cbOrderByDirection.SelectedValue==null?"":cbOrderByDirection.SelectedValue.ToString();

            wiss.ImageWidth = string.IsNullOrEmpty(txtWidth.Text) ? 0 : Convert.ToInt32(txtWidth.Text);
            wiss.ImageHeight = string.IsNullOrEmpty(txtHeight.Text) ? 0 : Convert.ToInt32(txtHeight.Text);
            wiss.AR = cbAspectRatio.SelectedValue==null?"":cbAspectRatio.SelectedValue.ToString();
            
            wiss.CollectionID = txtCollectionID.Text;
            wiss.FavoriteID = txtFavoritesID.Text;

            return wiss.Save();
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
            else if (val == "favorites")
                tabControl1.SelectedTab = tpFavorites;
            else if (val == "collection")
                tabControl1.SelectedTab = tpCollections;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start("https://wallhaven.cc/settings/account");
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to open Wallhaven Account Settings");
            }
        }

        private void cbColor_DrawItem(object sender, DrawItemEventArgs e)
        {
            // Get the item text    
            string hex = ((ComboBox)sender).Items[e.Index].ToString();

            if(hex == "NONE")
            {
                using (Pen red = new Pen(Color.Red, 3))
                {
                    e.Graphics.DrawLine(red,
                        new Point(e.Bounds.Left, e.Bounds.Top),
                        new Point(e.Bounds.Right, e.Bounds.Bottom));
                }
            } else {
                using (Brush pen = new SolidBrush(ColorTranslator.FromHtml("#" + hex)))
                {
                    e.Graphics.FillRectangle(pen, e.Bounds);
                }
            }
        }
    }
}
