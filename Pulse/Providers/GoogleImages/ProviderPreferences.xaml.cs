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

namespace GoogleImages
{
    /// <summary>
    /// Interaction logic for ProviderPreferences.xaml
    /// </summary>
    public partial class ProviderPreferences : Pulse.Base.ProviderConfigurationBase
    {
        private GoogleImageSearchSettings giss = null;

        public ProviderPreferences()
        {
            InitializeComponent();
        }

        public override void LoadConfiguration(string config)
        {
            //fresh config
            if (string.IsNullOrEmpty(config))
            {
                giss = new GoogleImageSearchSettings();
            }
                //deserialize
            else
            {
                giss = GoogleImageSearchSettings.LoadFromXML(config);
            }

            comboBox1.SelectedValue = giss.Color;
            txtHeight.Text = giss.ImageHeight.ToString();
            txtWidth.Text = giss.ImageWidth.ToString();
        }

        public override string SaveConfiguration()
        {
            //I should find out if we can do two-way binding on these properties to make this cleaner...
            giss.Color = comboBox1.SelectedValue != null ? comboBox1.SelectedValue.ToString() : "";
            giss.ImageWidth = Convert.ToInt32(txtWidth.Text);
            giss.ImageHeight = Convert.ToInt32(txtHeight.Text);

            var s = giss.Save();

            return s;
        }

        private void ProviderConfigurationBase_Loaded(object sender, RoutedEventArgs e)
        {
            //bind colors to colors combobox on load
            var colors = GoogleImageSearchSettings.GoogleImageColors.GetColors();
            comboBox1.ItemsSource = colors;

        }
    }
}
