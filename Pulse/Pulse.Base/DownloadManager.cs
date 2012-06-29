using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Threading;

namespace Pulse.Base
{
    public class DownloadManager
    {
        public delegate void PictureDownloadedHandler(Picture filePath);
        public event PictureDownloadedHandler PictureDownloaded;

        public delegate void PictureDownloadingHandler();
        public event PictureDownloadingHandler PictureDownloading;

        public delegate void PictureDownloadingCompleteHandler();
        public event PictureDownloadingCompleteHandler PictureDownloadingComplete;

        private readonly Random rnd = new Random();

        /// <summary>
        /// Retrieves a random picture from the picture list
        /// </summary>
        /// <param name="pl">Picture list from which to retrieve pictures</param>
        /// <param name="saveFolder">Location where to save the picture</param>
        /// <param name="currentPicture">(optional) the current picture, to avoid repeates.  Pass null if not needed or this is the first picture.</param>
        public Picture GetPicture(PictureList pl, string saveFolder, Picture currentPicture)
        {
            Picture pic = null;

            if (pl == null || pl.Pictures.Count == 0) return pic;

            //validate that the output directory exists
            if (!Directory.Exists(saveFolder))
                Directory.CreateDirectory(saveFolder);

            //pick the next picture at random
            // only "non-random" bit is that we make sure that the next random picture isn't the same as our current one
            var index = 0;
            do
            {
                index = rnd.Next(pl.Pictures.Count);
            } while (currentPicture != null && currentPicture.Url == pl.Pictures[index].Url);

            pic = pl.Pictures[index];
            //download current picture first
            GetPicture(pic, saveFolder, true, false);

            return pic;
        }
        
        public void GetPicture(Picture pic, string saveFolder, bool hookEvent, bool async)
        {
            //check if the requested image exists, if it does then fire event and return
            //if image already has a local path then use it (just what we need for local provider where images are not stored in cache).
            var picturePath = string.IsNullOrEmpty(pic.LocalPath) ? pic.CalculateLocalPath(saveFolder) : pic.LocalPath;
            pic.LocalPath = picturePath;


            if (pic.IsGood)
            {
                //if the wallpaper image already exists, and passes our 0 size check then fire the event
                if (PictureDownloaded != null && hookEvent)
                    PictureDownloaded(pic);

                return;
            }

            var fi = new FileInfo(picturePath);

            //for files that do not exist "Delete" does nothing per MSDN docs
            try { fi.Delete(); }
            catch (Exception ex)
            {
                Log.Logger.Write(string.Format("Error deleting 0 byte file '{0}' while preparint to redownload it. Exception details: {1}", fi.FullName, ex.ToString()), Log.LoggerLevels.Errors);
            }

            //if this will become our background image them hook into the event
            // for async instead of using WebClient async method lets just use seperate thread so we can use the same method for both
            if (async)
            {
                ThreadStart ts = () =>
                {
                    DownloadFile(pic, picturePath, hookEvent);
                };

                Thread t = new Thread(ts);
                t.Start();
            }
            else
            {
                DownloadFile(pic, picturePath, hookEvent);
            }
                
        }

        public void PreFetchFiles(PictureList pl, string saveFolder)
        {
            if (pl == null || pl.Pictures.Count == 0) return;

            //validate that the output directory exists
            if (!Directory.Exists(saveFolder))
                Directory.CreateDirectory(saveFolder);

            foreach (Picture pic in pl.Pictures)
            {
                GetPicture(pic, saveFolder, false, true);
            }
        }

        private void DownloadFile(Picture pic, string picturePath, bool hookEvent)
        {
            try
            {
                WebClient wcLocal = new WebClient();

                wcLocal.DownloadFile(new Uri(pic.Url), picturePath);
                if (PictureDownloaded != null && hookEvent)
                    PictureDownloaded(pic);
            }
            catch (Exception ex)
            {
                Log.Logger.Write(string.Format("Error downloading new picture. Url: '{0}', Save Path: '{1}'. Exception details: {2}", pic.Url, picturePath, ex.ToString()), Log.LoggerLevels.Errors);
            }
        }

        private void ClientDownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (PictureDownloaded != null)
                PictureDownloaded((Picture)e.UserState);
        }
    }
}
