using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Pulse.Base;
using System.IO;
using System.Globalization;
using System.Collections.ObjectModel;
using System.Reflection;

namespace PulseForm
{
    public partial class frmPulseOptions : Form
    {
        public List<ActiveProviderInfo> OutputProviderInfos
        {
            get { return _OutputProviderInfos; }
        }

        public event EventHandler UpdateSettings;
        private readonly List<string> langCodes = new List<string>();
        private string _tempProviderConfig = string.Empty;
        List<ActiveProviderInfo> _OutputProviderInfos = new List<ActiveProviderInfo>();

        public frmPulseOptions()
        {
            InitializeComponent();
            cbUpdateFrequencyUnit.DataSource = Enum.GetValues(typeof(Settings.IntervalUnits));

        }

        private void frmPulseOptions_Load(object sender, EventArgs e)
        {
            var fileInfo = new FileInfo(Assembly.GetExecutingAssembly().Location);
            BuildTag.Text = Assembly.GetExecutingAssembly().GetName().Version + ".beta." + fileInfo.LastWriteTimeUtc.ToString("yyMMdd-HHmm");

            if (!string.IsNullOrEmpty(Settings.CurrentSettings.Search))
                SearchBox.Text = Settings.CurrentSettings.Search;

            cbDownloadAutomatically.Checked = Settings.CurrentSettings.DownloadAutomatically;
            cbAutoChangeonStartup.Checked = Settings.CurrentSettings.DownloadOnAppStartup;
            udInterval.Value = Settings.CurrentSettings.RefreshInterval;
            
            cbPrefetch.Checked = Settings.CurrentSettings.PreFetch;


            //LanguageComboBox.Items.Add(new ComboBoxItem() { Content = CultureInfo.GetCultureInfo("en-US").NativeName });
            //langCodes.Add("en-US");
            //var langs = from x in Directory.GetDirectories(Settings.Path) where x.Contains("-") select System.IO.Path.GetFileNameWithoutExtension(x);
            //foreach (var l in langs)
            //{
            //    try
            //    {
            //        var c = CultureInfo.GetCultureInfo(l);
            //        langCodes.Add(c.Name);
            //        LanguageComboBox.Items.Add(new ComboBoxItem() { Content = c.NativeName });
            //    }
            //    catch { }
            //}

            //LanguageComboBox.Text = CultureInfo.GetCultureInfo(Settings.CurrentSettings.Language).NativeName;

            cbDeleteOldFiles.Checked = Settings.CurrentSettings.ClearOldPics;
            nudTempAge.Value = Settings.CurrentSettings.ClearInterval;


            //input providers
            var inputProviders = ProviderManager.Instance.GetProvidersByType<IInputProvider>();
            if (inputProviders.Count > 0)
            {
                foreach (var p in inputProviders)
                {
                    cbProviders.Items.Add(p.Key);
                }
                cbProviders.SelectedItem = Settings.CurrentSettings.Provider;

                //handle settings button enable/disable and loading string from config if it exists
                HandleProviderSettingsEnableAndLoad();
            }

            //output providers
            foreach (var op in ProviderManager.Instance.GetProvidersByType<IOutputProvider>())
            {
                if (Settings.CurrentSettings.ProviderSettings.ContainsKey(op.Key))
                {
                    var ep = Settings.CurrentSettings.ProviderSettings[op.Key];

                    _OutputProviderInfos.Add(new ActiveProviderInfo()
                    {
                        Active = ep.Active,
                        AsyncOK = ep.AsyncOK,
                        ExecutionOrder = ep.ExecutionOrder,
                        ProviderConfig = ep.ProviderConfig,
                        ProviderName = ep.ProviderName
                    });
                }
                else
                {
                    _OutputProviderInfos.Add(new ActiveProviderInfo()
                    {
                        Active = false,
                        AsyncOK = false,
                        ExecutionOrder = 0,
                        ProviderConfig = string.Empty,
                        ProviderName = op.Key
                    });
                }
            }

            //sort
            SortOutputProviders();

            dgvOutputProviders.DataSource = OutputProviderInfos;

            ApplyButton.Enabled = false;
        }

        private void SortOutputProviders()
        {
            _OutputProviderInfos = new List<ActiveProviderInfo>(
                _OutputProviderInfos.OrderBy(x => x.Active)
                .ThenBy(x => x.ExecutionOrder)
                .ThenBy(x => x.ProviderName));
        }

        private void OkButtonClick(object sender, EventArgs e)
        {
            if (ApplyButton.Enabled)
            {
                ApplySettings();
                ApplyButton.Enabled = false;
            }
            this.Close();
        }

        private void CancelButtonClick(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ApplyButtonClick(object sender, EventArgs e)
        {
            ApplySettings();
            ApplyButton.Enabled = false;
        }

        private void ApplySettings()
        {
            //if (Settings.CurrentSettings.Search != SearchBox.Text)
            //    App.Translator.Result = string.Empty;
            //check if the search box text == options search box text (keyword(s)), if so use empty string
            // on some sites searching with no query is fine
            
            Settings.CurrentSettings.Search = SearchBox.Text;
            Settings.CurrentSettings.DownloadAutomatically = cbDownloadAutomatically.Checked;
            Settings.CurrentSettings.DownloadOnAppStartup = cbDownloadAutomatically.Checked;
            Settings.CurrentSettings.ClearOldPics = cbDeleteOldFiles.Checked;
            Settings.CurrentSettings.ClearInterval = Convert.ToInt32(nudTempAge.Value);

            //set pre-fetch flag
            Settings.CurrentSettings.PreFetch = cbPrefetch.Checked;


            Settings.CurrentSettings.RefreshInterval = (int)udInterval.Value;

            //if (LanguageComboBox.SelectedIndex >= 0)
            //    Settings.CurrentSettings.Language = langCodes[LanguageComboBox.SelectedIndex];

            //Input provider settings (only if provider changed)
            if (!string.IsNullOrEmpty(cbProviders.Text) && Settings.CurrentSettings.Provider != cbProviders.Text)
            {
                //deactivate the old provider
                Settings.CurrentSettings.ProviderSettings.Remove(Settings.CurrentSettings.Provider);

                //set new provider
                Settings.CurrentSettings.Provider = cbProviders.Text;

                //initialize the new provider
                frmPulseHost.Runner.CurrentProvider = (IInputProvider)ProviderManager.Instance.InitializeProvider(Settings.CurrentSettings.Provider);
            }

            //save provider config if it exists
            if (!string.IsNullOrEmpty(_tempProviderConfig))
            {
                if (!Settings.CurrentSettings.ProviderSettings.ContainsKey(Settings.CurrentSettings.Provider))
                {
                    Settings.CurrentSettings.ProviderSettings[Settings.CurrentSettings.Provider] = new ActiveProviderInfo()
                    {
                        Active = true,
                        AsyncOK = false,
                        ExecutionOrder = 0,
                        ProviderConfig = _tempProviderConfig,
                        ProviderName = Settings.CurrentSettings.Provider
                    };
                }
                else
                {
                    Settings.CurrentSettings.ProviderSettings[Settings.CurrentSettings.Provider].ProviderConfig = _tempProviderConfig;
                }
            }

            //save output providers
            foreach (ActiveProviderInfo api in OutputProviderInfos)
            {
                if (Settings.CurrentSettings.ProviderSettings.ContainsKey(api.ProviderName))
                {
                    //check if the existing item is different then the previous
                    if (Settings.CurrentSettings.ProviderSettings[api.ProviderName].GetComparisonHashCode() != api.GetComparisonHashCode())
                    {
                        //since they are different check if we deactivated or just changed a setting
                        if (api.Active)
                        {
                            Settings.CurrentSettings.ProviderSettings[api.ProviderName] = api;
                        }
                        else
                        {
                            //validate that active state has changed, if it has then deactivate
                            if (Settings.CurrentSettings.ProviderSettings[api.ProviderName].Active != api.Active)
                            {
                                Settings.CurrentSettings.ProviderSettings[api.ProviderName].Instance.Deactivate(null);
                            }

                            Settings.CurrentSettings.ProviderSettings.Remove(api.ProviderName);
                        }
                    }
                }
                else if (api.Active)
                {
                    Settings.CurrentSettings.ProviderSettings.Add(api.ProviderName, api);

                    //activate new provider
                    api.Instance.Activate(null);
                }
            }

            //save config file
            Settings.CurrentSettings.Save(Settings.AppPath + "\\settings.conf");

            if (UpdateSettings != null)
            {
                UpdateSettings(null, EventArgs.Empty);
            }
        }

        private void OutputProvidersCheckBox_Click(object sender, EventArgs e)
        {
            ApplyButton.Enabled = true;
        }

        private void SearchBoxTextChanged(object sender, EventArgs e)
        {
            ApplyButton.Enabled = true;
        }

        private void CheckBoxClick(object sender, EventArgs e)
        {
            ApplyButton.Enabled = true;
        }

        private void ComboBoxSelectionChanged(object sender, EventArgs e)
        {
            ApplyButton.Enabled = true;
        }

        private void ProviderSelectionChanged(object sender, EventArgs e)
        {
            ComboBoxSelectionChanged(sender, e);

            HandleProviderSettingsEnableAndLoad();
        }

        private void HandleProviderSettingsEnableAndLoad()
        {
            //enable or disable settings button depending on settings availability
            ProviderSettings.Enabled = (cbProviders.SelectedItem != null) && ProviderManager.Instance.HasConfigurationWindow(cbProviders.SelectedItem.ToString()) != null;

            //load provider config from settings if it exists
            if (cbProviders.SelectedItem != null && Settings.CurrentSettings.ProviderSettings.ContainsKey(cbProviders.SelectedItem.ToString()))
            {
                _tempProviderConfig = Settings.CurrentSettings.ProviderSettings[cbProviders.SelectedItem.ToString()].ProviderConfig;
            }
            else { _tempProviderConfig = string.Empty; }
        }

        private void ClearNowButtonClick(object sender, EventArgs e)
        {
            if (Directory.Exists(Settings.AppPath + "\\Cache"))
            {
                foreach (var f in Directory.GetFiles(Settings.AppPath + "\\Cache"))
                {
                    try
                    {
                        File.Delete(f);
                    }
                    catch { }
                }
            }

            MessageBox.Show("All cached items have been cleared.");
        }

        private void ProviderSettings_Click(object sender, EventArgs e)
        {
            //get the usercontrol instance from Provider Manager
            var initSettings = ProviderManager.Instance.InitializeConfigurationWindow(cbProviders.SelectedItem.ToString());
            if (initSettings == null) return;

            //dialog window which will house the user control
            ProviderSettingsWindow psw = new ProviderSettingsWindow();

            //load the usercontrol into the window
            psw.LoadSettings(initSettings, cbProviders.SelectedItem.ToString());

            //load the configuration info into the user control
            initSettings.LoadConfiguration(_tempProviderConfig);

            //show the dialog box
            psw.ShowDialog();

            //if user clicked OK then call save to keep a copy of settings in memory
            if (initSettings.IsOK)
            {
                _tempProviderConfig = initSettings.SaveConfiguration();
                //activate apply option
                ApplyButton.Enabled = true;
            }
        }

        private void dgvOutputProviders_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            ApplyButton.Enabled = true;
        }
    }
}
