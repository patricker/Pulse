using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Pulse.Base;

namespace PulseMassDownloader
{
    public partial class Form1 : Form
    {
        private IProviderConfigurationEditor _current;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            providerComboBox1.DataSource = (from c in ProviderManager.Instance.GetProvidersByType<IInputProvider>()
                                            select c.Key).ToList();

            txtDownloadPath.Text = System.Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

        }

        private void providerComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var initSettings = ProviderManager.Instance.InitializeConfigurationWindow(providerComboBox1.SelectedItem.ToString());

            if (initSettings == null) return;

            HostConfigurationWindow(initSettings);
        }

        private void HostConfigurationWindow(IProviderConfigurationEditor initSettings)
        {
            panel1.Controls.Clear();
            _current = initSettings;

            //load empty settings
            _current.LoadConfiguration("");

            initSettings.HostMe(panel1);
        }

        private void btnFolderBrowser_Click(object sender, EventArgs e)
        {
            fbd.SelectedPath = txtDownloadPath.Text;

            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtDownloadPath.Text = fbd.SelectedPath;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            PictureSearch ps = new PictureSearch();
            ps.SearchProvider = new ActiveProviderInfo(providerComboBox1.SelectedItem.ToString()) { Active = true, ProviderConfig = _current.SaveConfiguration() };
            ps.MaxPictureCount = 100;

            ps.SaveFolder = txtDownloadPath.Text;

            IInputProvider p = ps.SearchProvider.Instance as IInputProvider;
            var pl = p.GetPictures(ps);

            PictureBatch pb = new PictureBatch();
            pb.AllPictures.Add(pl);

            DownloadManager.Current.SaveFolder = txtDownloadPath.Text;
            DownloadManager.Current.PreFetchFiles(pb);
        }
    }
}
