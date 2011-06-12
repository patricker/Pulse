using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Pulse.Base;

namespace Pulse
{
    public class PictureManager
    {
        public Picture CurrentPicture;
        public List<Picture> Pictures;
        private readonly ProviderManager pmanager;
        private readonly Random rnd;
        private readonly WebClient client;

        public delegate void PictureDownloadedHandler();
        public event PictureDownloadedHandler PictureDownloaded;
        public delegate void PictureDownloadingHandler();
        public event PictureDownloadingHandler PictureDownloading;

        private string lastKeyword;
        private int index = 0;

        public int SearchResultsCount
        {
            get { return pmanager.CurrentProvider.GetResultsCount(); }
        }

        public Provider CurrentProvider
        {
            get { return pmanager.CurrentProvider; }
            set { pmanager.CurrentProvider = value; }
        }

        public List<Provider> Providers
        {
            get { return pmanager.Providers; }
        }

        public PictureManager()
        {
            pmanager = new ProviderManager();
            pmanager.FindProviders();

            rnd = new Random(Environment.TickCount);
            client = new WebClient();
            client.DownloadFileCompleted += ClientDownloadFileCompleted;
        }

        public void GetPicture(string keyword)
        {
            if (client.IsBusy)
                client.CancelAsync();
            ThreadStart threadStarter = () =>
            {
                if (string.IsNullOrEmpty(keyword))
                    return;
                if (PictureDownloading != null)
                    PictureDownloading();
                if (Pictures == null || lastKeyword != keyword)
                    Pictures = pmanager.CurrentProvider.GetPictures(keyword, App.Settings.SkipLowRes, App.Settings.GetMaxRes, App.FilterKeywords);
                if (Pictures == null)
                {
                    App.Log("Nothing found.");
                    if (PictureDownloaded != null)
                        PictureDownloaded();
                    return;
                }
                CurrentPicture = Pictures[rnd.Next(Pictures.Count)];
               // CurrentPicture = Pictures[index];
                if (!Directory.Exists(App.Path + "\\Cache"))
                    Directory.CreateDirectory(App.Path + "\\Cache");
                if (!File.Exists(App.Path + "\\Cache\\" + CurrentPicture.Id + ".jpg"))
                {
                    client.DownloadFileAsync(new Uri(CurrentPicture.Url), App.Path + "\\Cache\\" + CurrentPicture.Id + ".jpg");
                }
                else
                {
                    if (PictureDownloaded != null)
                        PictureDownloaded();
                    //WinAPI.SystemParametersInfo(WinAPI.SPI_SETDESKWALLPAPER, 0, App.Path + "\\Cache\\" + CurrentPicture.Id, WinAPI.SPIF_UPDATEINIFILE).ToString();
                }

                lastKeyword = keyword;
                index++;
                if (index > Pictures.Count)
                    index = 0;
            };
            var thread = new Thread(threadStarter);
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        void ClientDownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (PictureDownloaded != null)
                PictureDownloaded();
        }

        public static void ReduceQuality(string file, string destFile, int quality)
        {
            // We will store the correct image codec in this object
            ImageCodecInfo iciJpegCodec = null;
            // This will specify the image quality to the encoder
            EncoderParameter epQuality = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
            // Get all image codecs that are available
            ImageCodecInfo[] iciCodecs = ImageCodecInfo.GetImageEncoders();
            // Store the quality parameter in the list of encoder parameters
            EncoderParameters epParameters = new EncoderParameters(1);
            epParameters.Param[0] = epQuality;

            // Loop through all the image codecs
            for (int i = 0; i < iciCodecs.Length; i++)
            {
                // Until the one that we are interested in is found, which is image/jpeg
                if (iciCodecs[i].MimeType == "image/jpeg")
                {
                    iciJpegCodec = iciCodecs[i];
                    break;
                }
            }

            // Create a new Image object from the current file
            Image newImage = Image.FromFile(file);

            // Get the file information again, this time we want to find out the extension
            // Save the new file at the selected path with the specified encoder parameters, and reuse the same file name
            newImage.Save(destFile, iciJpegCodec, epParameters);
            newImage.Dispose();

            var info = new FileInfo(destFile);
            if (info.Length / 1024 > 256 && quality > 11)
            {
                ReduceQuality(file, destFile, quality - 10);
            }
        }
    }
}
