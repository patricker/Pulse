﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.IO;
using System.Threading;
using System.Resources;

namespace Pulse.Base
{
    public class PulseRunner : IDisposable
    {
        public event Pulse.Base.Status.StatusChanged StatusChanged;
        
        public PictureBatch CurrentBatch { get; set; }

        public PictureManager PictureManager = new PictureManager();
        public Dictionary<Guid, ActiveProviderInfo> CurrentInputProviders = new Dictionary<Guid, ActiveProviderInfo>();

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

            wallpaperChangerTimer.Elapsed += WallpaperChangerTimerTick;
            clearOldWallpapersTimer.Elapsed += clearOldWallpapersTimer_Elapsed;

            UpdateFromConfiguration();

            //if we have a provider and we are setup for automatic picture download/change on startup then do so
            if(CurrentInputProviders.Count > 0)
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
            //setup the timers for changing wallpapers and clearing old files
            SetTimers();

            //setup list of valid & active input providers
            SetInputProviders();
        }

        public void SetNewStatus(Status status)
        {
            if (StatusChanged != null)
                StatusChanged(status);
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

        public void BanPicture(string pictureURL, string localPath)
        {
            //add the url to the ban list
            Settings.CurrentSettings.BannedImages.Add(pictureURL);
            //save the settings file
            Settings.CurrentSettings.Save(Settings.AppPath + "\\settings.conf");

            try
            {
                //delete the file from the local disk
                File.Delete(localPath);
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
            if (CurrentInputProviders.Count == 0) return;
            //create the new picture batch
            PictureBatch pb = new PictureBatch() {PreviousBatch = CurrentBatch};
            this.CurrentBatch = pb;

            //create another view of the input providers, otherwise if the list changes
            // because user changes options then it breaks :)
            foreach (KeyValuePair<Guid, ActiveProviderInfo> kvpGAPI in CurrentInputProviders.ToArray())
            {
                ActiveProviderInfo api = kvpGAPI.Value;

                var ps = new PictureSearch()
                {
                    SaveFolder = Settings.CurrentSettings.CachePath,
                    MaxPictureCount = Settings.CurrentSettings.MaxPictureDownloadCount,
                    SearchProvider = api,
                    BannedURLs = Settings.CurrentSettings.BannedImages
                };

                //get new pictures
                PictureList pl = PictureManager.GetPictureList(ps);
                
                //save to picturebatch 
                pb.AllPictures.Add(pl);
            }

            //Clear the download Queue
            DownloadManager.ClearQueue();
            
            //process downloaded picture list
            ProcessDownloadedPicture(pb);

            
            //if prefetch is enabled, validate that all pictures have been downloaded
            if (Settings.CurrentSettings.PreFetch) DownloadManager.PreFetchFiles(pb);
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

        protected void SetInputProviders()
        {
            //load providers from settings
            var validInputs = ProviderManager.Instance.GetProvidersByType<IInputProvider>();
            var toKeep = new List<Guid>();

            foreach (var op in Settings.CurrentSettings.ProviderSettings)
            {
                if (!CurrentInputProviders.ContainsKey(op.Key))
                {
                    if (validInputs.ContainsKey(op.Value.ProviderName) && op.Value.Active)
                    {
                        CurrentInputProviders.Add(op.Key, op.Value);

                        //activate provider
                        op.Value.Instance.Activate(null);
                    }
                }
                else
                {
                    //if this instance of the provider is already present then make sure settings are the same
                    if (CurrentInputProviders[op.Key].GetComparisonHashCode() !=
                        op.Value.GetComparisonHashCode())
                    {
                        //if not the same configuration the swap for new config
                        CurrentInputProviders[op.Key] = op.Value;
                    }
                }

                toKeep.Add(op.Key);
            }

            //deactivate and remove any providers that aren't being kept
            var toRemove = (from c in CurrentInputProviders
                            where !toKeep.Contains(c.Key)
                            select c).ToList();

            foreach (var c in toRemove)
            {
                c.Value.Instance.Deactivate(null);
                CurrentInputProviders.Remove(c.Key);
            }
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

        private void ProcessDownloadedPicture(PictureBatch pb)
        {
            if (pb != null && pb.AllPictures.Any())
            {
                ////get the picture downloading wrapper, but don't queue it up for download yet.  We need to hook into it first.
                //PictureDownload pic = DownloadManager.GetPicture(pl, CurrentPicture, false);
                ////set for max priority
                //pic.Priority = 1;
                ////set current picture so that we don't reuse it and so we can ban it
                //CurrentPicture = pic.Picture;

                //call all activated output providers in order
                foreach (var op in Settings.CurrentSettings.ProviderSettings.Values.Where(x => x.Active).OrderBy(x => x.ExecutionOrder))
                {
                    if (!typeof(IOutputProvider).IsAssignableFrom(op.Instance.GetType())) continue;
                    //wrap each in a try/catch block so we avoid killing pulse on error
                    try
                    {
                        IOutputProvider ip = ((IOutputProvider)op.Instance);
                        string config = op.ProviderConfig;
                        ip.ProcessPicture(pb, config);

                        //if (ip.Mode == OutputProviderMode.Single)
                        //{
                        //    pic.PictureDownloaded += new PictureDownload.PictureDownloadEvent(
                        //            delegate(PictureDownload t) {
                        //                ip.ProcessPicture(pic.Picture, config);
                        //            });
                        //}
                        //else if (ip.Mode == OutputProviderMode.Multiple)
                            
                    }
                    catch (Exception ex)
                    {
                        Log.Logger.Write(string.Format("Error processing output plugin '{0}'.  Error: {1}", op.ProviderName, ex.ToString()), Log.LoggerLevels.Errors);
                    }
                }

                ////queue picture up for download
                //DownloadManager.QueuePicture(pic);
            }
        }

        public void Dispose()
        {
            wallpaperChangerTimer.Dispose();
            clearOldWallpapersTimer.Dispose();
        }
    }
}
