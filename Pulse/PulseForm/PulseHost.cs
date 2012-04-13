using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using Pulse.Base;
using System.IO;
using System.Diagnostics;

namespace PulseForm
{
    public partial class frmPulseHost : Form
    {
        public static PictureManager PictureManager = new PictureManager();
        public static IInputProvider CurrentProvider = null;

        private System.Timers.Timer timer;

        public frmPulseHost()
        {
            InitializeComponent();
        }


        private void frmPulseHost_Load(object sender, EventArgs e)
        {
            //App startup, load settings
            Log.Logger.Write("Pulse starting up", Log.LoggerLevels.Information);

            //Hook global exception handler for the app domain
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            if (Log.Logger.LoggerLevel == Log.LoggerLevels.Verbose)
            {
                Log.Logger.Write(string.Format("Settings loaded, settings are: {0}", Environment.NewLine + Settings.CurrentSettings.Save()), Log.LoggerLevels.Verbose);
            }

            //System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo(Settings.CurrentSettings.Language);
            //System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo(Settings.CurrentSettings.Language);

            //Log.Logger.Write(string.Format("Current Culture: {0}, UI Culture: {1}",
            //    Thread.CurrentThread.CurrentCulture.ToString(),
            //    Thread.CurrentThread.CurrentUICulture.ToString()),
            //    Log.LoggerLevels.Information);

            Log.Logger.Write(string.Format("Clear old pics flag set to '{0}'", Settings.CurrentSettings.ClearOldPics.ToString()), Log.LoggerLevels.Verbose);
            //clear out old pictures on startup after 3 days
            // this should be on a timer job for people who never turn off their computers
            if (Settings.CurrentSettings.ClearOldPics && Directory.Exists(Settings.CurrentSettings.CachePath))
            {
                var oFiles = Directory.GetFiles(Settings.CurrentSettings.CachePath).Where(x => DateTime.Now.Subtract(File.GetCreationTime(x)).TotalDays >= Settings.CurrentSettings.ClearInterval);

                Log.Logger.Write(string.Format("Deleting {0} expired pics", Settings.CurrentSettings.ClearOldPics.ToString()), Log.LoggerLevels.Information);

                try
                {
                    foreach (var f in oFiles)
                    {
                        Log.Logger.Write(string.Format("Deleting '{0}'", f), Log.LoggerLevels.Verbose);
                        File.Delete(f);
                    }
                }
                catch (Exception ex)
                {
                    Log.Logger.Write(string.Format("Error cleaning out old pictures. Exception details: {0}", ex.ToString()), Log.LoggerLevels.Errors);
                }
            }

            PictureManager.PictureDownloaded += PmanagerPictureDownloaded;
            PictureManager.PictureDownloading += PictureManager_PictureDownloading;

            timer = new System.Timers.Timer();

            TimeSpan ts;

            //calculate timer elapse frequency
            switch (Settings.CurrentSettings.IntervalUnit)
            {
                case Settings.IntervalUnits.Days:
                    ts = new TimeSpan(Settings.CurrentSettings.RefreshInterval, 0, 0, 0);
                    break;
                case Settings.IntervalUnits.Hours:
                    ts = new TimeSpan(0, Settings.CurrentSettings.RefreshInterval, 0, 0);
                    break;
                case Settings.IntervalUnits.Minutes:
                    ts = new TimeSpan(0, 0, Settings.CurrentSettings.RefreshInterval, 0);
                    break;
                case Settings.IntervalUnits.Seconds:
                    ts = new TimeSpan(0, 0, 0, Settings.CurrentSettings.RefreshInterval);
                    break;
                default:
                    ts = new TimeSpan(0, 1, 0, 0);
                    break;
            }


            timer.Interval = ts.TotalMilliseconds; // Settings.CurrentSettings.RefreshInterval * 60 * 1000;
            timer.Elapsed += TimerTick;

            if (Settings.CurrentSettings.DownloadAutomatically)
                timer.Start();

            //load provider from settings
            if (ProviderManager.Instance.Providers.ContainsKey(Settings.CurrentSettings.Provider))
            {
                CurrentProvider = (IInputProvider)ProviderManager.Instance.InitializeProvider(Settings.CurrentSettings.Provider);
            }

            if (CurrentProvider == null)
            {
                MessageBox.Show("A provider is not currently selected.  Please choose a wallpaper provider from the options menu.");
            }
            else
            {
                Log.Logger.Write(string.Format("Autodownload wallpaper on startup is: '{0}'", Settings.CurrentSettings.DownloadOnAppStartup.ToString()),
                    Log.LoggerLevels.Information);

                //get next photo on startup if requested
                if (Settings.CurrentSettings.DownloadOnAppStartup)
                {
                    Log.Logger.Write("DownloadNextPicture called because 'Settings.CurrentSettings.DownloadOnAppStartup' == true", Log.LoggerLevels.Verbose);

                    DownloadNextPicture();
                }
            }
        }

        void TimerTick(object sender, EventArgs e)
        {
            Log.Logger.Write("DownloadNextPicture called from 'TimerTick'",
                Log.LoggerLevels.Verbose);

            //stop the timer
            timer.Stop();
            
            try
            {
                DownloadNextPicture();
            }
            catch (Exception ex)
            {
                Log.Logger.Write("TimerTick Errored: " + ex.ToString(),
                    Log.LoggerLevels.Errors);
            }
            finally
            {
                timer.Start();
            }
        }

        void SkipToNextPicture()
        {
            DownloadNextPicture();
            //Stop and restart the timer so that the next picture won't happen to soon
            if (Settings.CurrentSettings.DownloadAutomatically)
            {
                timer.Stop();
                timer.Start();
            }
        }

        void DownloadNextPicture()
        {
            var ps = new PictureSearch()
            {
                SearchString = Settings.CurrentSettings.Search,
                PreFetch = Settings.CurrentSettings.PreFetch,
                SaveFolder = Settings.CurrentSettings.CachePath,
                MaxPictureCount = Settings.CurrentSettings.MaxPictureDownloadCount,
                SearchProvider = CurrentProvider,
                BannedURLs = Settings.CurrentSettings.BannedImages,
                ProviderSearchSettings = Settings.CurrentSettings.ProviderSettings.ContainsKey(Settings.CurrentSettings.Provider) ? Settings.CurrentSettings.ProviderSettings[Settings.CurrentSettings.Provider].ProviderConfig : string.Empty
            };

            PictureManager.GetPicture(ps);
        }

        void PmanagerPictureDownloaded(Picture pic)
        {
            if (pic != null)
            {
                //enable image banning
                banImageToolStripMenuItem.Enabled = true;

                //call all activated output providers in order
                foreach (var op in Settings.CurrentSettings.ProviderSettings.Values.Where(x => x.Active).OrderBy(x => x.ExecutionOrder))
                {
                    if (!typeof(IOutputProvider).IsAssignableFrom(op.Instance.GetType())) continue;
                    //wrap each in a try/catch block so we avoid killing pulse on error
                    try
                    {
                        ((IOutputProvider)op.Instance).ProcessPicture(pic);
                    }
                    catch (Exception ex)
                    {
                        Log.Logger.Write(string.Format("Error processing output plugin '{0}'.  Error: {1}", op.ProviderName, ex.ToString()), Log.LoggerLevels.Errors);
                    }
                }
            }

            //var stream = GetResourceStream(new Uri("/Pulse;component/Resources/icon_small.ico", UriKind.Relative)).Stream;
            //trayIcon.Icon = new Icon(stream);
            //stream.Close();
        }

        void PictureManager_PictureDownloading()
        {
            //var stream = GetResourceStream(new Uri("/Pulse;component/Resources/icon_downloading.ico", UriKind.Relative)).Stream;
            //trayIcon.Icon = new Icon(stream);
            //stream.Close();
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            Log.Logger.Write("Unhandled exception! Error Details: " + args.ExceptionObject.ToString(), Log.LoggerLevels.Errors);

            MessageBox.Show("Pulse has encountered an exception and will exit.  The exception has been logged to the Pulse Log file.  Please post on the Issue Tracker page located on the Pulse Website @ http://pulse.codeplex.com/.  Exception details: " + ((Exception)args.ExceptionObject).Message, 
                "Pulse Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            frmPulseOptions po = new frmPulseOptions();
            po.Show();
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void openCacheFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(Settings.CurrentSettings.CachePath)) Directory.CreateDirectory(Settings.CurrentSettings.CachePath);
            //launch directory
            Process.Start("explorer.exe", Settings.CurrentSettings.CachePath);
        }

        private void banImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (PictureManager.CurrentPicture != null)
            {
                if (MessageBox.Show(string.Format("Ban '{0}'?", PictureManager.CurrentPicture.Url), "Image Ban", MessageBoxButtons.YesNo) != System.Windows.Forms.DialogResult.Yes)
                {
                    return;
                }

                //add the url to the ban list
                Settings.CurrentSettings.BannedImages.Add(PictureManager.CurrentPicture.Url);
                //save the settings file
                Settings.CurrentSettings.Save(Settings.Path + "\\settings.conf");

                try
                {
                    //delete the file from the local disk
                    File.Delete(PictureManager.CurrentPicture.LocalPath);
                }
                catch (Exception ex)
                {
                    Log.Logger.Write(string.Format("Error deleting banned pictures. Exception details: {0}", ex.ToString()), Log.LoggerLevels.Errors);
                }

                //skip to next photo
                SkipToNextPicture();
            }
        }

        private void nextPictureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SkipToNextPicture();
        }
    }
}
