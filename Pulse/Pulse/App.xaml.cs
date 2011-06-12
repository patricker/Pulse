using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using Microsoft.Win32;
using Pulse.Base;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace Pulse
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static Settings Settings;
        public static string Path = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private Options optionsWindow;
        public static PictureManager PictureManager;
        private DispatcherTimer timer;
        public static Translator Translator;
        public static List<string> FilterKeywords;
        private string translatedKeyword;

        private void ApplicationStartup(object sender, StartupEventArgs e)
        {
            if (e.Args.Contains("/enableoembg"))
            {
                EnableOEMBackground();
                Shutdown();
                return;
            }

            if (e.Args.Contains("/disableoembg"))
            {
                DisableOEMBackground();
                Shutdown();
                return;
            }

            if (e.Args.Contains("/createdirs"))
            {
                CreateDirs();
                Shutdown();
                return;
            }

            Settings = (Settings)XmlSerializable.Load(typeof(Settings), "settings.conf") ?? new Settings();

            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo(Settings.Language);
            System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo(Settings.Language);

            AddIcon();

            if (App.Settings.ClearOldPics && Directory.Exists(Path + "\\Cache"))
            {
                foreach (var f in Directory.GetFiles(Path + "\\Cache"))
                {
                    if (File.GetCreationTime(f).CompareTo(DateTime.Now.AddDays(1 - App.Settings.ClearInterval)) <= 0)
                    {
                        File.Delete(f);
                    }
                }
            }

            PictureManager = new PictureManager();
            PictureManager.PictureDownloaded += PmanagerPictureDownloaded;
            PictureManager.PictureDownloading += PictureManager_PictureDownloading;


            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMinutes(Settings.RefreshInterval);
            timer.Tick += TimerTick;

            if (Settings.DownloadAutomatically)
                timer.Start();

            Translator = new Translator();
            //Translator.Translated += TranslatorTranslated;
            WinAPI.SetProcessWorkingSetSize(System.Diagnostics.Process.GetCurrentProcess().Handle, -1, -1);

            FilterKeywords = new List<string>();
            if (!string.IsNullOrEmpty(Settings.Filter))
            {
                var s = Settings.Filter.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                if (s.Length == 0)
                    return;
                FilterKeywords.AddRange(s);
            }
        }

        private void EnableOEMBackground()
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey("Software").OpenSubKey("Microsoft").OpenSubKey("Windows")
                    .OpenSubKey("CurrentVersion").OpenSubKey("Authentication").OpenSubKey("LogonUI").OpenSubKey("Background", true))
                {
                    key.SetValue("OEMBackground", 1, RegistryValueKind.DWord);
                    key.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Pulse Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DisableOEMBackground()
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey("Software", RegistryKeyPermissionCheck.ReadWriteSubTree).OpenSubKey("Microsoft").OpenSubKey("Windows")
                    .OpenSubKey("CurrentVersion").OpenSubKey("Authentication").OpenSubKey("LogonUI").OpenSubKey("Background", true))
                {
                    key.DeleteValue("OEMBackground", false);
                    key.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Pulse Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetAccessRules()
        {
            DirectorySecurity dirSec = Directory.GetAccessControl(Environment.SystemDirectory + "\\oobe\\info\\backgrounds");
            dirSec.AddAccessRule(new FileSystemAccessRule(Environment.UserDomainName + "\\" + Environment.UserName, FileSystemRights.FullControl, AccessControlType.Allow));
            Directory.SetAccessControl(Environment.SystemDirectory + "\\oobe\\info\\backgrounds", dirSec);

        }

        private void CreateDirs()
        {
            if (!Directory.Exists(Environment.SystemDirectory + "\\oobe\\info"))
                Directory.CreateDirectory(Environment.SystemDirectory + "\\oobe\\info");
            if (!Directory.Exists(Environment.SystemDirectory + "\\oobe\\info\\backgrounds"))
                Directory.CreateDirectory(Environment.SystemDirectory + "\\oobe\\info\\backgrounds");
            SetAccessRules();
        }

        //void TranslatorTranslated(object sender, EventArgs e)
        //{
        //    App.PictureManager.GetPicture(Translator.Result);
        //}

        public static void Log(string message)
        {
            if (!Directory.Exists(App.Path + "\\Logs"))
                Directory.CreateDirectory(App.Path + "\\Logs");
            if (!File.Exists(App.Path + "\\Logs\\log.txt"))
            {
                File.WriteAllText(App.Path + "\\Logs\\log.txt", string.Empty);
            }

            try
            {
                File.AppendAllText(App.Path + "\\Logs\\log.txt",
                   DateTime.Now + " -------------- " + (char)(13) + (char)(10) + "OS: " + Environment.OSVersion.VersionString + (char)(13) + (char)(10) + message + (char)(13) + (char)(10));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Can't write log. " + ex.Message);
            }
        }

        void TimerTick(object sender, EventArgs e)
        {
            if (App.Settings.Language == "ru-RU" || App.Settings.Provider != "Rewalls")
                App.PictureManager.GetPicture(App.Settings.Search);
            else
            {
                if (string.IsNullOrEmpty(translatedKeyword))
                {
                    ThreadStart threadStarter = delegate
                                                    {
                                                        translatedKeyword = Translator.TranslateText(App.Settings.Search, "en|ru");
                                                        App.PictureManager.GetPicture(translatedKeyword);
                                                    };
                    var thread = new Thread(threadStarter);
                    thread.SetApartmentState(ApartmentState.STA);
                    thread.Start();
                }
                else
                    App.PictureManager.GetPicture(translatedKeyword);
            }
        }

        private static System.Windows.Forms.ContextMenu trayMenu;
        private static System.Windows.Forms.MenuItem closeMenuItem;
        private static System.Windows.Forms.MenuItem optionsMenuItem;
        private static System.Windows.Forms.MenuItem nextMenuItem;
        private static System.Windows.Forms.NotifyIcon trayIcon;

        public void AddIcon()
        {
            trayMenu = new System.Windows.Forms.ContextMenu();

            closeMenuItem = new System.Windows.Forms.MenuItem();
            closeMenuItem.Text = Pulse.Properties.Resources.Close;
            closeMenuItem.Click += CloseMenuItemClick;

            optionsMenuItem = new System.Windows.Forms.MenuItem();
            optionsMenuItem.Text = Pulse.Properties.Resources.Options;
            optionsMenuItem.Click += OptionsMenuItemClick;

            nextMenuItem = new System.Windows.Forms.MenuItem();
            nextMenuItem.Text = Pulse.Properties.Resources.NextPicture;
            nextMenuItem.Click += new EventHandler(NextMenuItemClick);

            trayMenu.MenuItems.Add(nextMenuItem);
            trayMenu.MenuItems.Add(optionsMenuItem);
            trayMenu.MenuItems.Add(closeMenuItem);

            trayIcon = new System.Windows.Forms.NotifyIcon();

            var stream = GetResourceStream(new Uri("/Pulse;component/Resources/icon_small.ico", UriKind.Relative)).Stream;
            trayIcon.Icon = new Icon(stream); //System.Drawing.Icon.ExtractAssociatedIcon(Path + @"\Pulse.exe");
            stream.Close();
            trayIcon.Text = "Pulse";
            trayIcon.Visible = true;
            trayIcon.ContextMenu = trayMenu;
        }

        void NextMenuItemClick(object sender, EventArgs e)
        {
            TimerTick(null, EventArgs.Empty);
            //if (App.Settings.Language == "ru-RU")
            //    App.PictureManager.GetPicture(App.Settings.Search);
            //else
            //{
            //    if (string.IsNullOrEmpty(Translator.Result))
            //        Translator.TranslateText(App.Settings.Search, "en|ru");
            //    else
            //        App.PictureManager.GetPicture(Translator.Result);
            //}
        }

        void OptionsMenuItemClick(object sender, EventArgs e)
        {
            if (optionsWindow != null && optionsWindow.IsVisible)
            {
                optionsWindow.Activate();
                return;
            }

            if (optionsWindow != null)
                optionsWindow.UpdateSettings -= OptionsWindowUpdateSettings;

            optionsWindow = new Options();
            optionsWindow.UpdateSettings += OptionsWindowUpdateSettings;
            optionsWindow.Closed += OptionsWindowClosed;

            if (App.Settings.Language == "he-IL" || App.Settings.Language == "ar-SA")
            {
                optionsWindow.FlowDirection = System.Windows.FlowDirection.RightToLeft;
            }
            else
            {
                optionsWindow.FlowDirection = System.Windows.FlowDirection.LeftToRight;
            }

            optionsWindow.ShowDialog();
        }

        void OptionsWindowClosed(object sender, EventArgs e)
        {
            optionsWindow.Closed -= OptionsWindowClosed;
            WinAPI.SetProcessWorkingSetSize(System.Diagnostics.Process.GetCurrentProcess().Handle, -1, -1);
        }

        void OptionsWindowUpdateSettings(object sender, EventArgs e)
        {
            translatedKeyword = string.Empty;
            FilterKeywords.Clear();
            if (!string.IsNullOrEmpty(Settings.Filter))
            {
                var s = Settings.Filter.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                if (s.Length == 0)
                    return;
                FilterKeywords.AddRange(s);
            }

            timer.Stop();
            timer.Interval = TimeSpan.FromMinutes(Settings.RefreshInterval);
            if (Settings.DownloadAutomatically)
                timer.Start();
        }

        void CloseMenuItemClick(object sender, EventArgs e)
        {
            trayIcon.Visible = false;
            trayIcon.Dispose();
            this.Shutdown();
        }

        void PmanagerPictureDownloaded()
        {
            if (PictureManager.CurrentPicture != null)
            {
                WinAPI.SystemParametersInfo(WinAPI.SPI_SETDESKWALLPAPER, 0, App.Path + "\\Cache\\" + PictureManager.CurrentPicture.Id + ".jpg", WinAPI.SPIF_UPDATEINIFILE).ToString();
                if (App.Settings.ChangeLogonBg)
                {
                    if (!Directory.Exists(Environment.SystemDirectory + "\\oobe\\info") || !Directory.Exists(Environment.SystemDirectory + "\\oobe\\info\\backgrounds"))
                    {
                        var p = new ProcessStartInfo { Verb = "runas", FileName = Assembly.GetExecutingAssembly().Location, Arguments = "/createdirs" };
                        var proc = Process.Start(p);
                        proc.WaitForExit();
                    }
                    var info = new FileInfo(App.Path + "\\Cache\\" + PictureManager.CurrentPicture.Id + ".jpg");
                    if (info.Length > 0)
                    {
                        File.Copy(App.Path + "\\Cache\\" + PictureManager.CurrentPicture.Id + ".jpg", Environment.SystemDirectory + "\\oobe\\info\\backgrounds\\backgroundDefault.jpg", true);
                        info = new FileInfo(Environment.SystemDirectory + "\\oobe\\info\\backgrounds\\backgroundDefault.jpg");
                        if (info.Length / 1024 > 256)
                        {
                            PictureManager.ReduceQuality(App.Path + "\\Cache\\" + PictureManager.CurrentPicture.Id + ".jpg", Environment.SystemDirectory + "\\oobe\\info\\backgrounds\\backgroundDefault.jpg", 90);
                        }
                    }
                }
            }
            var stream = GetResourceStream(new Uri("/Pulse;component/Resources/icon_small.ico", UriKind.Relative)).Stream;
            trayIcon.Icon = new Icon(stream);
            stream.Close();
        }

        void PictureManager_PictureDownloading()
        {
            var stream = GetResourceStream(new Uri("/Pulse;component/Resources/icon_downloading.ico", UriKind.Relative)).Stream;
            trayIcon.Icon = new Icon(stream);
            stream.Close();
        }

        private void ApplicationDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Log("Unhandled exception!");
            Log(e.Exception.ToString());
            MessageBox.Show(e.Exception.Message, "Pulse error", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }
    }
}
