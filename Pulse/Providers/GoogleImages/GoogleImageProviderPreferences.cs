using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GoogleImages
{
    public partial class GoogleImageProviderPreferences : UserControl, Pulse.Base.IProviderConfigurationEditor
    {
        GoogleImageSearchSettings giss;

        public GoogleImageProviderPreferences()
        {
            InitializeComponent();
            IsOK = false;
        }

        private void GoogleImageProviderPreferences_Load(object sender, EventArgs e)
        {
            //bind colors to colors combobox on load
            var colors = GoogleImageSearchSettings.GoogleImageColors.GetColors();
            comboBox1.DataSource = colors;
        }

        public void LoadConfiguration(string config)
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

        public string SaveConfiguration()
        {
            //I should find out if we can do two-way binding on these properties to make this cleaner...
            giss.Color = comboBox1.SelectedValue != null ? comboBox1.SelectedValue.ToString() : "";
            giss.ImageWidth = Convert.ToInt32(txtWidth.Text);
            giss.ImageHeight = Convert.ToInt32(txtHeight.Text);

            var s = giss.Save();

            return s;
        }

        public bool IsOK { get; set; }


        public void HostMe(object parent)
        {
            Control c = parent as Control;

            c.Controls.Add(this);
        }
    }
}
