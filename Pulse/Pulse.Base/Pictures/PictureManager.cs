using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Drawing.Drawing2D;

namespace Pulse.Base
{
    public class PictureManager
    {
        public Picture CurrentPicture { get; set; }
        public PictureList Pictures;
        
        private readonly Random rnd = new Random();

        public delegate void PictureDownloadedHandler(Picture filePath);
        public event PictureDownloadedHandler PictureDownloaded;
        public delegate void PictureDownloadingHandler();
        public event PictureDownloadingHandler PictureDownloading;

        public static Pair<int, int> PrimaryScreenResolution { 
            get {
                var rect = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
                return new Pair<int, int>(rect.Width, rect.Height);
            }
        }

        public static List<Rectangle> ScreenResolutions
        {
            get
            {
                List<Rectangle> rects = new List<Rectangle>();

                rects.AddRange(from c in System.Windows.Forms.Screen.AllScreens select c.Bounds);

                return rects;
            }
        }

        public static void ShrinkImage(string imgPath, string outPath, int destWidth, int destHeight, int quality)
        {
            Image img = ShrinkImage(imgPath, destWidth, destHeight);

            //-----write out Thumbnail to the output stream------        
            //get jpeg image coded info so we can use it when saving
            ImageCodecInfo ici = ImageCodecInfo.GetImageEncoders().Where(c => c.MimeType == "image/jpeg").First();

            EncoderParameters epParameters = new EncoderParameters(1);
            epParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);

            //save image to file
            img.Save(outPath, ici, epParameters);
        }

        
        //Patricker - This code came from a stackoverflow answer I posted a while back.  I converted it to C# from VB.Net and from
        // asp.net to windows.  Link: http://stackoverflow.com/questions/4436209/asp-net-version-of-timthumb-php-class/4506072#4506072
        /// <summary>
        /// 
        /// </summary>
        /// <param name="imgPath">Path to image to load</param>
        /// <param name="destWidth">Desired width (0 to base off of aspect ratio and specified height)</param>
        /// <param name="destHeight">Desired Height (0 to base off of aspect ratio and specified width)</param>
        /// <remarks>If both height and width are 0 or non zero if the aspect ratio of the image
        /// does not match the aspect ratio based on width/height specifed then the image will skew. 
        /// (if both height/width are zero then screen resolution is used)</remarks>
        public static Image ShrinkImage(string imgPath, int destWidth, int destHeight)
        {
            //load image from file path
            Image img = Bitmap.FromFile(imgPath);
            double origRatio = (Math.Min(img.Width, img.Height) / Math.Max(img.Width, img.Height));

            //---Calculate thumbnail sizes---
            double destRatio = 0;

            //if both width and height are 0 then use defaults (Screen resolution)
            if (destWidth == 0 & destHeight == 0)
            {
                var scResolution = PrimaryScreenResolution;
                destWidth = scResolution.First;
                destHeight = scResolution.Second;

            }
            else if (destWidth > 0 & destHeight > 0)
            {
                //do nothing, we have both sizes already
            }
            else if (destWidth > 0)
            {
                destHeight = (int)Math.Floor((double)img.Height * (destWidth / img.Width));
            }
            else if (destHeight > 0)
            {
                destWidth = (int)Math.Floor((double)img.Width * (destHeight / img.Height));
            }

            destRatio = (Math.Min(destWidth, destHeight) / Math.Max(destWidth, destHeight));

            //calculate source image sizes (rectangle) to get pixel data from        
            int sourceWidth = img.Width;
            int sourceHeight = img.Height;

            int sourceX = 0;
            int sourceY = 0;

            int cmpx = img.Width / destWidth;
            int cmpy = img.Height / destHeight;

            //selection is based on the smallest dimension
            if (cmpx > cmpy)
            {
                sourceWidth = img.Width / cmpx * cmpy;
                sourceX = ((img.Width - (img.Width / cmpx * cmpy)) / 2);
            }
            else if (cmpy > cmpx)
            {
                sourceHeight = img.Height / cmpy * cmpx;
                sourceY = ((img.Height - (img.Height / cmpy * cmpx)) / 2);
            }

            //---create the new image---
            Bitmap bmpThumb = new Bitmap(destWidth, destHeight);
            var g = Graphics.FromImage(bmpThumb);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            g.DrawImage(img, new Rectangle(0, 0, destWidth, destHeight), new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight), GraphicsUnit.Pixel);


            return bmpThumb;
        }


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
                if((Pictures != null && ps.GetSearchHash() != Pictures.SearchSettingsHash) && File.Exists(fPath))
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
                        PictureDownloaded(null);
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
                GetPicture(CurrentPicture, ps.SaveFolder, true, false);                

                //if pre-fetch true, get the rest of the pictures
                if (ps.PreFetch)
                {
                    for (int i = 0; i < Pictures.Pictures.Count; i++)
                    {
                        //skip the image we already processed
                        if (i == index) continue;

                        GetPicture(Pictures.Pictures[i], ps.SaveFolder, false, true);
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

        public void GetPicture(Picture pic, string saveFolder, bool hookEvent, bool async)
        {
            //check if the requested image exists, if it does then fire event if needed and return
            //if image already has a local path then use it (just what we need for local provider where images are not stored in cache).
            var picturePath = string.IsNullOrEmpty(pic.LocalPath) ? Path.Combine(saveFolder, pic.Id + ".jpg") : pic.LocalPath;
            pic.LocalPath = picturePath;

            var fi = new FileInfo(picturePath);

            if(fi.Exists) {
                if (fi.Length > 0)
                {
                    //if the wallpaper image already exists, and passes our 0 size check then fire the event
                    if (PictureDownloaded != null && hookEvent)
                        PictureDownloaded(pic);

                    return;
                }
                    //if file size is 0 then delete so we can retry
                else { try { fi.Delete(); } catch { } }
            }

            //if the picture does not exist localy (or failed 0 size check) then download
            var wcLocal = new WebClient();
                
            //if this will become our background image them hook into the event
            if (hookEvent && async)
            {
                wcLocal.DownloadFileCompleted += ClientDownloadFileCompleted;
            }

            try
            {
                if (async) wcLocal.DownloadFileAsync(new Uri(pic.Url), picturePath, pic);
                else
                {
                    wcLocal.DownloadFile(new Uri(pic.Url), picturePath);
                    PictureDownloaded(pic);
                }
            }
            catch { }
        }

        void ClientDownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (PictureDownloaded != null)
                PictureDownloaded((Picture)e.UserState);
        }
    }
}
