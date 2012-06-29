using System;using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace wallbase
{
    public partial class WallbaseProviderPrefs : Pulse.Base.ProviderConfigurationBase
    {

        WallbaseImageSearchSettings wiss = null;

        public WallbaseProviderPrefs()
        {
            InitializeComponent();
        }

        public override void LoadConfiguration(string config)
        {
            try
            {   if(!string.IsNullOrEmpty(config))
                    wiss = WallbaseImageSearchSettings.LoadFromXML(config);
            }
            catch { }

            if (wiss == null)
                wiss = new WallbaseImageSearchSettings();

            txtUserID.Text = wiss.Username;
            txtPassword.Text = wiss.Password;

            cbArea.SelectedValue = wiss.SA;

            cbWG.Checked = wiss.WG;
            cbW.Checked = wiss.W;
            cbHR.Checked = wiss.HR;

            cbSFW.Checked = wiss.SFW;
            cbSketchy.Checked = wiss.SKETCHY;
            cbNSFW.Checked = wiss.NSFW;

            cbImageSizeType.SelectedValue = wiss.SO;
            cbOrderBy.SelectedValue = wiss.OB;
            cbOrderByDirection.SelectedValue = wiss.OBD;

            txtWidth.Text = wiss.ImageWidth.ToString();
            txtHeight.Text = wiss.ImageHeight.ToString();

            txtCollectionID.Text = wiss.CollectionID;

            if (wiss.Color != System.Drawing.Color.Empty)
            {
                pnlColor.BackColor = wiss.Color;
                cdPicker.Color = wiss.Color;
            }
        }

        public override string SaveConfiguration()
        {
            wiss.Username = txtUserID.Text;
            wiss.Password = txtPassword.Text;

            wiss.WG = cbWG.Checked;
            wiss.W = cbW.Checked;
            wiss.HR = cbHR.Checked;

            wiss.SFW = cbSFW.Checked;
            wiss.SKETCHY = cbSketchy.Checked;
            wiss.NSFW = cbNSFW.Checked;

            wiss.SA = cbArea.SelectedValue.ToString();
            wiss.SO = cbImageSizeType.SelectedValue.ToString();
            wiss.OB = cbOrderBy.SelectedValue.ToString();
            wiss.OBD = cbOrderByDirection.SelectedValue.ToString();

            wiss.ImageWidth = string.IsNullOrEmpty(txtWidth.Text) ? 0 : Convert.ToInt32(txtWidth.Text);
            wiss.ImageHeight = string.IsNullOrEmpty(txtHeight.Text) ? 0 : Convert.ToInt32(txtHeight.Text);

            
            wiss.CollectionID = txtCollectionID.Text;

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
            cbArea.DataSource = WallbaseImageSearchSettings.SearchArea.GetSearchAreas();
            cbImageSizeType.DataSource = WallbaseImageSearchSettings.SizingOption.GetDirectionList();
            cbOrderBy.DataSource = WallbaseImageSearchSettings.OrderBy.GetOrderByList();
            cbOrderByDirection.DataSource = WallbaseImageSearchSettings.OrderByDirection.GetDirectionList();
        }   
    }
}
