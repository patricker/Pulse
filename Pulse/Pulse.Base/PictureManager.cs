using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace Pulse.Base
{
    public class PictureManager
    {
        public Picture CurrentPicture { get; set; }
        public PictureList Pictures;
        
        private readonly Random rnd = new Random();

        public delegate void PictureDownloadedHandler();
        public event PictureDownloadedHandler PictureDownloaded;
        public delegate void PictureDownloadingHandler();
        public event PictureDownloadingHandler PictureDownloading;

        public void GetPicture(PictureSearch ps)
        {
            if (ps == null || ps.SearchProvider == null) return;

            //if (client.IsBusy)
            //    client.CancelAsync();

            ThreadStart threadStarter = () =>
            {
                var loadedFromFile = false;
                var fPath = Path.Combine(ps.SaveFolder, "CACHE_" + ps.SearchProvider.GetType().ToString() + ".xml");

                //check if we should load from file
                if((Pictures == null || (Pictures != null && ps.GetSearchHash() != Pictures.SearchSettingsHash)) && File.Exists(fPath))
                {
                    try
                    {
                        Pictures = PictureList.LoadFromFile(fPath);
                        if(Pictures != null) loadedFromFile = true;
                    }
                    catch (Exception ex) { }
                }

                if (PictureDownloading != null)
                    PictureDownloading();
                //if our search settings have changed then null our existing list
                if (Pictures != null && ps.GetSearchHash() != Pictures.SearchSettingsHash)
                    Pictures = null;
                if (Pictures == null || Pictures.Pictures.Count == 0)
                {
                    Pictures = ps.SearchProvider.GetPictures(ps);
                    Pictures.SearchSettingsHash = ps.GetSearchHash();
                    loadedFromFile = false;
                }
                if (Pictures == null || Pictures.Pictures.Count == 0)
                {
                    //App.Log("Nothing found.");
                    if (PictureDownloaded != null)
                        PictureDownloaded();
                    return;
                }

                //validate that the output directory exists
                if (!Directory.Exists(ps.SaveFolder))
                    Directory.CreateDirectory(ps.SaveFolder);

                //pick the next picture at random
                // only "non-random" bit is that we make sure that the next random picture isn't the same as our current one
                var index = 0;
                do
                {
                    index = rnd.Next(Pictures.Pictures.Count);
                } while (CurrentPicture != null && CurrentPicture.Url == Pictures.Pictures[index].Url);

                CurrentPicture = Pictures.Pictures[index];
                //download current picture first
                GetPicture(CurrentPicture, ps.SaveFolder, true);                

                //if pre-fetch true, get the rest of the pictures
                if (ps.PreFetch)
                {
                    for (int i = 0; i < Pictures.Pictures.Count; i++)
                    {
                        //skip the image we already processed
                        if (i == index) continue;

                        GetPicture(Pictures.Pictures[i], ps.SaveFolder, false);
                    }
                }

                //cache the picture list to file
                if (!loadedFromFile)
                {
                    Pictures.Save(fPath);
                }
            };
            var thread = new Thread(threadStarter);
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        public void GetPicture(Picture pic, string saveFolder, bool hookEvent)
        {
            //check if the requested image exists, if it does then fire event if needed and return
            var picturePath = Path.Combine(saveFolder, pic.Id + ".jpg");

            if(File.Exists(picturePath)) {
                //if the wallpaper image already exists then fire the event
                if (PictureDownloaded != null && hookEvent)
                    PictureDownloaded();

                return;
            }

            //if the picture does not exist localy then download

            var wcLocal = new WebClient();
                
            //if this will become our background image them hook into the event
            if (hookEvent)
            {
                wcLocal.DownloadFileCompleted += ClientDownloadFileCompleted;
            }

            //download wallpaper async
            wcLocal.DownloadFileAsync(new Uri(pic.Url), picturePath);
        }


        void ClientDownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (PictureDownloaded != null)
                PictureDownloaded();
        }

        public static void ReduceQuality(string file, string destFile, int quality)
        {
            // we get the image/jpeg encoder using linq
            ImageCodecInfo iciJpegCodec = (from c in ImageCodecInfo.GetImageEncoders() where c.MimeType=="image/jpeg" select c).SingleOrDefault();

            // Store the quality parameter in the list of encoder parameters
            EncoderParameters epParameters = new EncoderParameters(1);
            epParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);

            //we use htis to keep track of the current size of the image, default to int.max to make sure we always run
            var fSize = int.MaxValue;
            //check if the image we generated is larger then 256kb, if it is reduce quality by 10 and try again
            while(fSize > 256) {
                // Create a new Image object from the current file
                using(Image newImage = Image.FromFile(file)) {
                    // Save the new file at the selected path with the specified encoder parameters, and reuse the same file name
                    newImage.Save(destFile, iciJpegCodec, epParameters);

                    //get output size in kb
                    fSize = (int)(new FileInfo(destFile).Length/1024);

                    //reduce quality by 10, this will only affect the output if while loop continues
                    quality -=10;
                    epParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
                }
            }
        }
    }
}
