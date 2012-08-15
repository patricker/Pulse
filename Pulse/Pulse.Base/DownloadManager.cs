using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Threading;
using Pulse.Base;

namespace Pulse.Base
{
    public class DownloadManager
    {
        public static DownloadManager Current
        {
            get { return _current; }
        }

        private static DownloadManager _current = new DownloadManager();

        private System.Timers.Timer _queuePollingTimer = new System.Timers.Timer(1000);
        private int _maxConcurrentDownloads = 5;

        private readonly Random rnd = new Random();

        public List<PictureDownload> DownloadQueue { get; set; }
        
        public string SaveFolder { get; private set; }

        public DownloadManager() : this(Settings.CurrentSettings.CachePath) { 
           
        }

        public DownloadManager(string saveFolder)
        {
            this.SaveFolder = saveFolder;
            DownloadQueue = new List<PictureDownload>();

            //validate that the output directory exists
            if (!Directory.Exists(SaveFolder))
                Directory.CreateDirectory(SaveFolder);

            //setup the timer
            _queuePollingTimer.Elapsed += new System.Timers.ElapsedEventHandler(_queuePollingTimer_Elapsed);
            _queuePollingTimer.Enabled = true;
        }

        private void _queuePollingTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            _queuePollingTimer.Stop();
            try
            {
                //check for any open spots in the queue
                var count = (from c in DownloadQueue
                             where c.Status == PictureDownload.DownloadStatus.Downloading
                             select c).Count();

                //start up the difference
                var toStart = (from c in DownloadQueue
                               where c.Status == PictureDownload.DownloadStatus.Stopped
                               orderby c.Priority ascending
                               select c).Take(_maxConcurrentDownloads - count);

                foreach (PictureDownload pd in toStart)
                {
                    pd.StartDownload();
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Write(string.Format("Error starting queued downloads. Exception details: {0}", ex.ToString()), Log.LoggerLevels.Errors);
            }
            finally { _queuePollingTimer.Start(); }
        }

        /// <summary>
        /// Retrieves a random picture from the picture list
        /// </summary>
        /// <param name="pl">Picture list from which to retrieve pictures</param>
        /// <param name="saveFolder">Location where to save the picture</param>
        /// <param name="currentPicture">(optional) the current picture, to avoid repeates.  Pass null if not needed or this is the first picture.</param>
        public PictureDownload GetPicture(PictureList pl, Picture currentPicture, bool queueForDownload)
        {
            Picture pic = null;

            if (pl == null || pl.Pictures.Count == 0) return null;

            //pick the next picture at random
            // only "non-random" bit is that we make sure that the next random picture isn't the same as our current one
            var index = 0;
            do
            {
                index = rnd.Next(pl.Pictures.Count);
            } while (currentPicture != null && currentPicture.Url == pl.Pictures[index].Url);

            pic = pl.Pictures[index];
            //download current picture first
            PictureDownload pd = GetPicture(pic, queueForDownload);

            return pd;
        }

        public PictureDownload GetPicture(Picture pic, bool queueForDownload)
        {
            //check if the requested image exists, if it does then return
            //if image already has a local path then use it (just what we need for local provider where images are not stored in cache).
            var picturePath = string.IsNullOrEmpty(pic.LocalPath) ? pic.CalculateLocalPath(SaveFolder) : pic.LocalPath;
            pic.LocalPath = picturePath;


            if (pic.IsGood)
            {
                //if the wallpaper image already exists, and passes our 0 size check then fire the event
                return new PictureDownload(pic);
            }

            var fi = new FileInfo(picturePath);

            //for files that do not exist "Delete" does nothing per MSDN docs
            try { fi.Delete(); }
            catch (Exception ex)
            {
                Log.Logger.Write(string.Format("Error deleting 0 byte file '{0}' while prepareing to redownload it. Exception details: {1}", fi.FullName, ex.ToString()), Log.LoggerLevels.Errors);
            }

            PictureDownload pd = new PictureDownload(pic);

            if (queueForDownload)
            {
                //add file to Queue
                DownloadQueue.Add(pd);
            }

            return pd;
        }

        public void PreFetchFiles(PictureList pl)
        {
            if (pl == null || pl.Pictures.Count == 0) return;

            foreach (Picture pic in pl.Pictures)
            {
                GetPicture(pic, true);
            }
        }
    }
}
