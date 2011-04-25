using System;
using System.Collections.Generic;
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

        public int SearchResultsCount
        {
            get { return pmanager.CurrentProvider.GetResultsCount(); }
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
            ThreadStart threadStarter = () =>
            {
                if (string.IsNullOrEmpty(keyword))
                    return;
                if (PictureDownloading != null)
                    PictureDownloading();
                Pictures = pmanager.CurrentProvider.GetPictures(keyword, App.Settings.SkipLowRes, App.Settings.GetMaxRes);
                if (Pictures == null)
                {
                    App.Log("Nothing found.");
                    if (PictureDownloaded != null)
                        PictureDownloaded();
                    return;
                }
                CurrentPicture = Pictures[rnd.Next(Pictures.Count)];
                if (!Directory.Exists(App.Path + "\\Cache"))
                    Directory.CreateDirectory(App.Path + "\\Cache");
                if (!File.Exists(App.Path + "\\Cache\\" + CurrentPicture.Id))
                {
                    client.DownloadFileAsync(new Uri(CurrentPicture.Url), App.Path + "\\Cache\\" + CurrentPicture.Id);
                }
                else
                {
                    if (PictureDownloaded != null)
                        PictureDownloaded();
                    //WinAPI.SystemParametersInfo(WinAPI.SPI_SETDESKWALLPAPER, 0, App.Path + "\\Cache\\" + CurrentPicture.Id, WinAPI.SPIF_UPDATEINIFILE).ToString();
                }
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

    }
}
