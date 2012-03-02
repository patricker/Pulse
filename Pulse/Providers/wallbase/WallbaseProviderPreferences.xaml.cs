using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace wallbase
{
    /// <summary>
    /// Interaction logic for WallbaseProviderPreferences.xaml
    /// </summary>
    public partial class WallbaseProviderPreferences : Pulse.Base.ProviderConfigurationBase
    {
        WallbaseImageSearchSettings wiss = null;

        public WallbaseProviderPreferences()
        {
            InitializeComponent();
        }

        public override void LoadConfiguration(string config) {
            wiss = string.IsNullOrEmpty(config) ? new WallbaseImageSearchSettings() : WallbaseImageSearchSettings.LoadFromXML(config);

            txtUserID.Text = wiss.Username;
            txtPassword.Password = wiss.Password;

            cbArea.SelectedValue = wiss.SA;

            cbWG.IsChecked = wiss.WG;
            cbW.IsChecked = wiss.W;
            cbHR.IsChecked = wiss.HR;

            cbSFW.IsChecked = wiss.SFW;
            cbSketchy.IsChecked = wiss.SKETCHY;
            cbNSFW.IsChecked = wiss.NSFW;

            cbImageSizeType.SelectedValue = wiss.SO;
            cbOrderBy.SelectedValue = wiss.OB;
            cbOrderByDirection.SelectedValue = wiss.OBD;

            txtWidth.Text = wiss.ImageWidth.ToString();
            txtHeight.Text = wiss.ImageHeight.ToString();

            txtCollectionID.Text = wiss.CollectionID;

            if (wiss.Color != System.Drawing.Color.Empty)
            {
                txtColorRed.Text = wiss.Color.R.ToString();
                txtColorGreen.Text = wiss.Color.G.ToString();
                txtColorBlue.Text = wiss.Color.B.ToString();
            }
        }

        public override string SaveConfiguration()
        {
            wiss.Username = txtUserID.Text;
            wiss.Password = txtPassword.Password;

            wiss.WG = cbWG.IsChecked.Value;
            wiss.W = cbW.IsChecked.Value;
            wiss.HR = cbHR.IsChecked.Value;

            wiss.SFW = cbSFW.IsChecked.Value;
            wiss.SKETCHY = cbSketchy.IsChecked.Value;
            wiss.NSFW = cbNSFW.IsChecked.Value;

            wiss.SA = cbArea.SelectedValue.ToString();
            wiss.SO = cbImageSizeType.SelectedValue.ToString();
            wiss.OB = cbOrderBy.SelectedValue.ToString();
            wiss.OBD = cbOrderByDirection.SelectedValue.ToString();

            wiss.ImageWidth = string.IsNullOrEmpty(txtWidth.Text) ? 0 : Convert.ToInt32(txtWidth.Text);
            wiss.ImageHeight = string.IsNullOrEmpty(txtHeight.Text) ? 0 : Convert.ToInt32(txtHeight.Text);

            if (!string.IsNullOrEmpty(txtColorRed.Text) && !string.IsNullOrEmpty(txtColorGreen.Text) && !string.IsNullOrEmpty(txtColorBlue.Text))
            {
                wiss.Color = System.Drawing.Color.FromArgb(Convert.ToInt32(txtColorRed.Text), Convert.ToInt32(txtColorGreen.Text), Convert.ToInt32(txtColorBlue.Text));
            }
            else
            {
                wiss.Color = System.Drawing.Color.Empty;
            }

            wiss.CollectionID = txtCollectionID.Text;

            return wiss.Save();
        }

        private void ProviderConfigurationBase_Loaded(object sender, RoutedEventArgs e)
        {
            //load in data items for cb's
            cbArea.ItemsSource = WallbaseImageSearchSettings.SearchArea.GetSearchAreas();
            cbImageSizeType.ItemsSource = WallbaseImageSearchSettings.SizingOption.GetDirectionList();
            cbOrderBy.ItemsSource = WallbaseImageSearchSettings.OrderBy.GetOrderByList();
            cbOrderByDirection.ItemsSource = WallbaseImageSearchSettings.OrderByDirection.GetDirectionList();

        }
    }
}
