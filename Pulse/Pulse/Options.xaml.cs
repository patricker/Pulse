using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;
using Pulse.Base;

namespace Pulse
{
    /// <summary>
    /// Interaction logic for Options.xaml
    /// </summary>
    public partial class Options : Window
    {
        public event EventHandler UpdateSettings;
        private readonly List<string> langCodes = new List<string>();
        private string _tempProviderConfig = string.Empty;

        public Options()
        {
            InitializeComponent();
        }

        private void WindowSourceInitialized(object sender, EventArgs e)
        {
            var handle = new WindowInteropHelper(this).Handle;

            var margins = new WinAPI.Margins { cyTopHeight = 34 };

            HwndSource.FromHwnd(handle).CompositionTarget.BackgroundColor = Color.FromArgb(0, 0, 0, 0);

            Dwm.ExtendGlassFrame(handle, ref margins);

            var fileInfo = new FileInfo(Assembly.GetExecutingAssembly().Location);
            BuildTag.Text = Assembly.GetExecutingAssembly().GetName().Version + ".beta." + fileInfo.LastWriteTimeUtc.ToString("yyMMdd-HHmm");

            if (!string.IsNullOrEmpty(App.Settings.Search))
                SearchBox.Text = App.Settings.Search;

            AutoDownloadCheckBox.IsChecked = App.Settings.DownloadAutomatically;
            if (App.Settings.RefreshInterval != 1)
                RefreshIntervalSlider.Value = App.Settings.RefreshInterval;
            else
                RefreshIntervalSlider.Value = 0;
            PreFetchCheckBox.IsChecked = App.Settings.PreFetch;
            

            LanguageComboBox.Items.Add(new ComboBoxItem() { Content = CultureInfo.GetCultureInfo("en-US").NativeName });
            langCodes.Add("en-US");
            var langs = from x in Directory.GetDirectories(App.Path) where x.Contains("-") select System.IO.Path.GetFileNameWithoutExtension(x);
            foreach (var l in langs)
            {
                try
                {
                    var c = CultureInfo.GetCultureInfo(l);
                    langCodes.Add(c.Name);
                    LanguageComboBox.Items.Add(new ComboBoxItem() { Content = c.NativeName });
                }
                catch { }
            }

            LanguageComboBox.Text = CultureInfo.GetCultureInfo(App.Settings.Language).NativeName;

            ClearCacheCheckBox.IsChecked = App.Settings.ClearOldPics;
            ClearIntervalSlider.Value = App.Settings.ClearInterval;

            FilterBox.Text = App.Settings.Filter;

            if (Environment.OSVersion.Version.Major < 6 || Environment.OSVersion.Version.Minor < 1)
                ChangeLogonBgCheckBox.Visibility = System.Windows.Visibility.Collapsed;
            ChangeLogonBgCheckBox.IsChecked = App.Settings.ChangeLogonBg;

            if (App.Settings.Language != "ru-RU")
            {
                FilterPanel.Visibility = System.Windows.Visibility.Collapsed;
            }


            if (App.PictureManager != null && App.ProviderManager.Providers != null)
            {
                foreach (var p in App.ProviderManager.Providers)
                {
                    ProvidersBox.Items.Add(p.Key);
                }
                ProvidersBox.SelectedValue = App.Settings.Provider;

                //handle settings button enable/disable and loading string from config if it exists
                HandleProviderSettingsEnableAndLoad();
            }

            ApplyButton.IsEnabled = false;
        }

        private void OkButtonClick(object sender, RoutedEventArgs e)
        {
            if (ApplyButton.IsEnabled)
            {
                ApplySettings();
                ApplyButton.IsEnabled = false;
            }
            this.Close();
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ApplyButtonClick(object sender, RoutedEventArgs e)
        {
            ApplySettings();
            ApplyButton.IsEnabled = false;
        }

        private void ApplySettings()
        {
            //if (App.Settings.Search != SearchBox.Text)
            //    App.Translator.Result = string.Empty;
            //check if the search box text == options search box text (keyword(s)), if so use empty string
            // on some sites searching with no query is fine
            App.Settings.Search = SearchBox.Text == Properties.Resources.OptionsSearchBox ? "":SearchBox.Text;
            App.Settings.DownloadAutomatically = (bool)AutoDownloadCheckBox.IsChecked;
            App.Settings.ClearOldPics = (bool)ClearCacheCheckBox.IsChecked;
            App.Settings.ClearInterval = (int)ClearIntervalSlider.Value;
            App.Settings.Filter = FilterBox.Text;
            if (!string.IsNullOrEmpty(ProvidersBox.Text))
                App.Settings.Provider = ProvidersBox.Text;

            //set pre-fetch flag
            App.Settings.PreFetch = (bool)PreFetchCheckBox.IsChecked;

            //save provider config if it exists
            if (!string.IsNullOrEmpty(_tempProviderConfig))
            {
                App.Settings.ProviderSettings[App.Settings.Provider] = _tempProviderConfig;
            }

            if (RefreshIntervalSlider.Value > 0)
                App.Settings.RefreshInterval = RefreshIntervalSlider.Value;
            else
                App.Settings.RefreshInterval = 1;

            if (LanguageComboBox.SelectedIndex >= 0)
                App.Settings.Language = langCodes[LanguageComboBox.SelectedIndex];

            if (App.Settings.ChangeLogonBg != (bool)ChangeLogonBgCheckBox.IsChecked)
            {
                App.Settings.ChangeLogonBg = (bool)ChangeLogonBgCheckBox.IsChecked;
                //HKLM\Software\Microsoft\Windows\CurrentVersion\Authentication\LogonUI\Background
                if (App.Settings.ChangeLogonBg)
                {
                    var p = new ProcessStartInfo { Verb = "runas", FileName = Assembly.GetExecutingAssembly().Location, Arguments = "/enableoembg" };
                    Process.Start(p);
                }
                else
                {
                    var p = new ProcessStartInfo { Verb = "runas", FileName = Assembly.GetExecutingAssembly().Location, Arguments = "/disableoembg" };
                    Process.Start(p);
                }
            }

            if (App.PictureManager != null && App.ProviderManager.Providers != null)
            {
                foreach (KeyValuePair<string, Type> p in App.ProviderManager.Providers)
                {
                    if (p.Key == App.Settings.Provider && App.CurrentProvider.GetType() != p.Value)
                    {
                        App.CurrentProvider = App.ProviderManager.InitializeProvider(p.Key);
                    }
                }
            }

            App.Settings.Save(App.Path + "\\settings.conf");

            if (UpdateSettings != null)
            {
                UpdateSettings(null, EventArgs.Empty);
            }
        }

        private void SearchBoxIsKeyboardFocusedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == false)
            {
                if (string.IsNullOrEmpty(SearchBox.Text))
                {
                    SearchBox.Text = Properties.Resources.OptionsSearchBox;
                    SearchBox.FontStyle = FontStyles.Italic;
                    SearchBox.Foreground = Brushes.Gray;
                }
            }
            else
            {
                if (SearchBox.Text == Properties.Resources.OptionsSearchBox)
                {
                    SearchBox.Text = "";
                    SearchBox.FontStyle = FontStyles.Normal;
                    SearchBox.Foreground = Brushes.Black;
                }
            }
        }

        private void SearchBoxKeyUp(object sender, KeyEventArgs e)
        {
            //What is this for?  SO that you can hit enter while in options and have it run the search??  Seems silly to me.
            //if (e.Key == Key.Enter && SearchBox.Text != Properties.Resources.OptionsSearchBox && SearchBox.Text.Length > 2)
            //{
            //    App.PictureManager.GetPicture(new PictureSearch() { ProviderSearchSettings = SearchBox.Text, PreFetch=false, MaxPictureCount=200, SearchProvider=App.CurrentProvider });
            //}
        }

        private void SearchBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            if (SearchBox.Text != Properties.Resources.OptionsSearchBox)
            {
                SearchBox.FontStyle = FontStyles.Normal;
                SearchBox.Foreground = Brushes.Black;
            }

            ApplyButton.IsEnabled = true;
        }

        private void CheckBoxClick(object sender, RoutedEventArgs e)
        {
            ApplyButton.IsEnabled = true;
        }

        private void RefreshIntervalSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ApplyButton.IsEnabled = true;
            if (RefreshIntervalSlider.Value < 60)
            {
                RefreshIntervalValueTextBlock.Text = RefreshIntervalSlider.Value + " " + Properties.Resources.OptionsIntervalMinutes;
            }
            else if (RefreshIntervalSlider.Value == 60)
            {
                RefreshIntervalValueTextBlock.Text = 1 + " " + Properties.Resources.OptionsIntervalHours;
            }
            else
            {
                RefreshIntervalValueTextBlock.Text = string.Format("{0} {1} {2} {3}", Math.Truncate(RefreshIntervalSlider.Value / 60), Properties.Resources.OptionsIntervalHours,
                    Math.Abs(Math.IEEERemainder(RefreshIntervalSlider.Value, 60)), Properties.Resources.OptionsIntervalMinutes);
            }
            if (RefreshIntervalSlider.Value == 0)
                RefreshIntervalValueTextBlock.Text = "1 " + Properties.Resources.OptionsIntervalMinutes;
        }

        private void ClearIntervalSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ApplyButton.IsEnabled = true;

            if (ClearIntervalSlider.Value == 1)
                ClearIntervalTextBlock.Text = string.Format(Properties.Resources.OptionsClearInterval, ClearIntervalSlider.Value + " " + Properties.Resources.OptionsDay);
            else
                ClearIntervalTextBlock.Text = string.Format(Properties.Resources.OptionsClearInterval, ClearIntervalSlider.Value + " " + Properties.Resources.OptionsDays);
        }

        private void ComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyButton.IsEnabled = true;
        }

        private void ProviderSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxSelectionChanged(sender, e);

            HandleProviderSettingsEnableAndLoad();
        }

        private void HandleProviderSettingsEnableAndLoad()
        {
            //enable or disable settings button depending on settings availability
            ProviderSettings.IsEnabled = (ProvidersBox.SelectedValue != null) && App.ProviderManager.HasConfigurationWindow(ProvidersBox.SelectedValue.ToString()) != null;

            //load provider config from settings if it exists
            if (ProvidersBox.SelectedValue != null && App.Settings.ProviderSettings.ContainsKey(ProvidersBox.SelectedValue.ToString()))
            {
                _tempProviderConfig = App.Settings.ProviderSettings[ProvidersBox.SelectedValue.ToString()];
            }
            else { _tempProviderConfig = string.Empty; }
        }

        private void ClearNowButtonClick(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(App.Path + "\\Cache"))
            {
                foreach (var f in Directory.GetFiles(App.Path + "\\Cache"))
                    File.Delete(f);
            }
        }

        private void ProviderSettings_Click(object sender, RoutedEventArgs e)
        {
            //get the usercontrol instance from Provider Manager
            var initSettings = App.ProviderManager.InitializeConfigurationWindow(ProvidersBox.SelectedValue.ToString());
            if (initSettings == null) return;

            //dialog window which will house the user control
            ProviderSettingsWindow psw = new ProviderSettingsWindow();

            //load the usercontrol into the window
            psw.LoadSettings(initSettings, ProvidersBox.SelectedValue.ToString());

            //load the configuration info into the user control
            initSettings.LoadConfiguration(_tempProviderConfig);

            //show the dialog box
            psw.ShowDialog();
            
            //if user clicked OK then call save to keep a copy of settings in memory
            if (initSettings.IsOK)
            {
                _tempProviderConfig = initSettings.SaveConfiguration();
                //activate apply option
                ApplyButton.IsEnabled = true;
            }
        }
    }
}
