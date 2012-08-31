using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.IO;
using System.Threading;

namespace Pulse.Base
{
    public class PulseRunner : IDisposable
    {
        public Picture CurrentPicture { get; set; }

        public PictureManager PictureManager = new PictureManager();
        public IInputProvider CurrentProvider = null;

        private DownloadManager DownloadManager = DownloadManager.Current;

        private System.Timers.Timer wallpaperChangerTimer;
        private System.Timers.Timer clearOldWallpapersTimer;

        public void Initialize()
        {
            //App startup, load settings
            Log.Logger.Write("Pulse starting up", Log.LoggerLevels.Information);

            //check if verbose before logging the settings so we only do the work of 
            // serializing them if we actually have to.
            if (Log.Logger.LoggerLevel == Log.LoggerLevels.Verbose)
            {
                Log.Logger.Write(string.Format("Settings loaded, settings are: {0}", Environment.NewLine + Settings.CurrentSettings.Save()), Log.LoggerLevels.Verbose);
            }

            Log.Logger.Write(string.Format("Clear old pics flag set to '{0}'", Settings.CurrentSettings.ClearOldPics.ToString()), Log.LoggerLevels.Verbose);

            if (Settings.CurrentSettings.ClearOldPics)
            {
                ClearOldWallpapers();
            }           

            wallpaperChangerTimer = new System.Timers.Timer();
            clearOldWallpapersTimer = new System.Timers.Timer();

            SetTimers();
            wallpaperChangerTimer.Elapsed += WallpaperChangerTimerTick;
            clearOldWallpapersTimer.Elapsed += clearOldWallpapersTimer_Elapsed;

            //load provider from settings
            if (ProviderManager.Instance.Providers.ContainsKey(Settings.CurrentSettings.Provider))
            {
                CurrentProvider = (IInputProvider)ProviderManager.Instance.InitializeProvider(Settings.CurrentSettings.Provider);
            }

            //if we have a provider and we are setup for automatic picture download/change on startup then do so
            if(CurrentProvider != null)
            {
                Log.Logger.Write(string.Format("Autodownload wallpaper on startup is: '{0}'", Settings.CurrentSettings.DownloadOnAppStartup.ToString()),
                    Log.LoggerLevels.Information);

                //get next photo on startup if requested
                if (Settings.CurrentSettings.DownloadOnAppStartup)
                {
                    Log.Logger.Write("DownloadNextPicture called because 'Settings.CurrentSettings.DownloadOnAppStartup' == true", Log.LoggerLevels.Verbose);

                    SkipToNextPicture();
                }
            }
        }

        public void UpdateFromConfiguration()
        {
            SetTimers();
        }

        public void ClearOldWallpapers()
        {
            //if our cache folder does not exist then don't do it
            if (!Directory.Exists(Settings.CurrentSettings.CachePath))
                return;

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

        public void SkipToNextPicture()
        {
            wallpaperChangerTimer.Stop();

            ThreadStart tstarter = () => {
                DownloadNextPicture();

                //restart the timer if appropriate
                if (Settings.CurrentSettings.ChangeOnTimer)
                {
                    wallpaperChangerTimer.Start();
                }
            };

            Thread t = new Thread(tstarter);

            t.Start();
        }

        public void BanPicture()
        {
            //add the url to the ban list
            Settings.CurrentSettings.BannedImages.Add(CurrentPicture.Url);
            //save the settings file
            Settings.CurrentSettings.Save(Settings.AppPath + "\\settings.conf");

            try
            {
                //delete the file from the local disk
                File.Delete(CurrentPicture.LocalPath);
            }
            catch (Exception ex)
            {
                Log.Logger.Write(string.Format("Error deleting banned pictures. Exception details: {0}", ex.ToString()), Log.LoggerLevels.Errors);
            }

            //skip to next photo
            SkipToNextPicture();        
        }

        protected void DownloadNextPicture()
        {
            var ps = new PictureSearch()
            {
                SearchString = Settings.CurrentSettings.Search,
                SaveFolder = Settings.CurrentSettings.CachePath,
                MaxPictureCount = Settings.CurrentSettings.MaxPictureDownloadCount,
                SearchProvider = CurrentProvider,
                BannedURLs = Settings.CurrentSettings.BannedImages,
                ProviderSearchSettings = Settings.CurrentSettings.ProviderSettings.ContainsKey(Settings.CurrentSettings.Provider) ? Settings.CurrentSettings.ProviderSettings[Settings.CurrentSettings.Provider].ProviderConfig : string.Empty
            };

            //Clear the download Queue
            DownloadManager.ClearQueue();

            //get new pictures
            PictureList pl = PictureManager.GetPictureList(ps);

            //process downloaded picture list
            ProcessDownloadedPicture(pl);

            //if prefetch is enabled, validate that all pictures have been downloaded
            if (Settings.CurrentSettings.PreFetch) DownloadManager.PreFetchFiles(pl);
        }

        protected void SetTimers()
        {
            wallpaperChangerTimer.Stop();
            clearOldWallpapersTimer.Stop();

            wallpaperChangerTimer.Interval =
                GetTimerTickTime(
                    Settings.CurrentSettings.IntervalUnit,
                    Settings.CurrentSettings.RefreshInterval
                        ).TotalMilliseconds;

            clearOldWallpapersTimer.Interval = TimeSpan.FromDays(Settings.CurrentSettings.ClearInterval).TotalMilliseconds;

            if(Settings.CurrentSettings.ClearOldPics)
                clearOldWallpapersTimer.Start();

            if (Settings.CurrentSettings.ChangeOnTimer)
                wallpaperChangerTimer.Start();
        }

        protected TimeSpan GetTimerTickTime(Settings.IntervalUnits unit, int amount)
        {
            TimeSpan ts;

            //calculate timer elapse frequency
            switch (unit)
            {
                case Settings.IntervalUnits.Days:
                    ts = new TimeSpan(amount, 0, 0, 0);
                    break;
                case Settings.IntervalUnits.Hours:
                    ts = new TimeSpan(0, amount, 0, 0);
                    break;
                case Settings.IntervalUnits.Minutes:
                    ts = new TimeSpan(0, 0, amount, 0);
                    break;
                case Settings.IntervalUnits.Seconds:
                    ts = new TimeSpan(0, 0, 0, amount);
                    break;
                default:
                    ts = new TimeSpan(0, 1, 0, 0);
                    break;
            }

            return ts;
        }

        private void WallpaperChangerTimerTick(object sender, EventArgs e)
        {
            Log.Logger.Write("DownloadNextPicture called from 'TimerTick'",
                Log.LoggerLevels.Verbose);

            //stop the timer
            wallpaperChangerTimer.Stop();

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
                wallpaperChangerTimer.Start();
            }
        }

        private void clearOldWallpapersTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            ClearOldWallpapers();
        }

        private void ProcessDownloadedPicture(PictureList pl)
        {
            if (pl != null && pl.Pictures.Any())
            {
                ////enable image banning
                //banImageToolStripMenuItem.Enabled = true;

                //get the picture downloading wrapper, but don't queue it up for download yet.  We need to hook into it first.
                PictureDownload pic = DownloadManager.GetPicture(pl, CurrentPicture, false);
                //set for max priority
                pic.Priority = 1;

                //call all activated output providers in order
                foreach (var op in Settings.CurrentSettings.ProviderSettings.Values.Where(x => x.Active).OrderBy(x => x.ExecutionOrder))
                {
                    if (!typeof(IOutputProvider).IsAssignableFrom(op.Instance.GetType())) continue;
                    //wrap each in a try/catch block so we avoid killing pulse on error
                    try
                    {
                        IOutputProvider ip = ((IOutputProvider)op.Instance);
                        string config = op.ProviderConfig;

                        if (ip.Mode == OutputProviderMode.Single)
                        {
                            pic.PictureDownloaded += new PictureDownload.PictureDownloadEvent(
                                    delegate(PictureDownload t) {
                                        ip.ProcessPicture(pic.Picture, config);
                                    });
                        }
                        else if (ip.Mode == OutputProviderMode.Multiple)
                            ip.ProcessPictures(pl, config);
                    }
                    catch (Exception ex)
                    {
                        Log.Logger.Write(string.Format("Error processing output plugin '{0}'.  Error: {1}", op.ProviderName, ex.ToString()), Log.LoggerLevels.Errors);
                    }
                }

                //queue picture up for download
                DownloadManager.QueuePicture(pic);
            }
        }

        public void Dispose()
        {
            wallpaperChangerTimer.Dispose();
            clearOldWallpapersTimer.Dispose();
        }
    }
}
