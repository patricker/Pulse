using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

namespace Pulse.Base
{
    public class PictureDownload
    {
        public delegate void PictureDownloadEvent(PictureDownload pic);

        public event PictureDownloadEvent PictureDownloaded;
        public event PictureDownloadEvent PictureDownloading;
        public event PictureDownloadEvent PictureDownloadingAborted;
        public event PictureDownloadEvent PictureDownloadProgressChanged;

        public Picture Picture { get; private set; }
        public int DownloadProgress { get; private set; }
        public DownloadStatus Status { get; private set; }
        public Exception LastError { get; private set; }
        public int Priority { get; set; }

        private WebClient _client = new WebClient();
        private int _failureCount = 0;        

        public enum DownloadStatus
        {
            Stopped,
            Downloading,
            Complete,
            Cancelled,
            Error
        }

        public PictureDownload(Picture p)
        {
            if (string.IsNullOrEmpty(p.LocalPath)) throw new ArgumentNullException("The Local Path of the picture must be defined before downloading it");

            this.Picture = p;
            //set default priority
            this.Priority = 999999999;

            //set default status of Stopped
            Status = DownloadStatus.Stopped;

            //setup webclient events
            _client.DownloadFileCompleted += new System.ComponentModel.AsyncCompletedEventHandler(_client_DownloadFileCompleted);
            _client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(_client_DownloadProgressChanged);
        }

        public void StartDownload()
        {
            _client.DownloadFileAsync(new Uri(Picture.Url), Picture.LocalPath, Picture);

            if (!this.Picture.IsGood)
            {
                Status = DownloadStatus.Downloading;
                //call picture downloading event
                if (PictureDownloading != null) PictureDownloading(this);
            }
            else
            {
                MarkAsComplete();
            }
        }

        public void CancelDownload()
        {
            _client.CancelAsync();
        }

        void _client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            DownloadProgress = e.ProgressPercentage;
            //call picture download progress changed
            if (PictureDownloadProgressChanged != null) PictureDownloadProgressChanged(this);
        }

        void _client_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            //if this was cancelled by the user then don't retry.  Set progress to 0 and downloading flag to false
            if (e.Cancelled) { 
                Status = DownloadStatus.Cancelled;
                DownloadProgress = 0;
                //try to delete the partially downloaded file
                try { File.Delete(Picture.LocalPath); } catch(Exception){}

                //call aborted event
                if (PictureDownloadingAborted != null) PictureDownloadingAborted(this);

                return; 
            }
            
            //if this download errored check our retry count and initiate it again
            if(e.Error != null) {
                LastError = e.Error;

                if(this._failureCount < 3) {
                    _failureCount++;
                    StartDownload();
                }else { 
                    Status = DownloadStatus.Error;
                    //call download aborted
                    if (PictureDownloadingAborted != null) PictureDownloadingAborted(this);
                }
            }

            MarkAsComplete();
        }

        private void MarkAsComplete()
        {
            //if we aren't cancelled and didn't error then we were successful
            Status = DownloadStatus.Complete;

            //call download complete
            if (PictureDownloaded != null) PictureDownloaded(this);
        }
    }
}
