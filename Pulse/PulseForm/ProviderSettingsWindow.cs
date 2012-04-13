using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Pulse.Base;

namespace PulseForm
{
    public partial class ProviderSettingsWindow : Form
    {
        private ProviderConfigurationBase _uc;

        public ProviderSettingsWindow()
        {
            InitializeComponent();
        }

        public void LoadSettings(ProviderConfigurationBase uc, string providerName)
        {
            _uc = uc;

            this.Text = "Provider Settings for " + providerName;
            scHost.Panel1.Controls.Add(uc);
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            _uc.IsOK = true;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            _uc.IsOK = false;
            this.Close();
        }
    }
}
