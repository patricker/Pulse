using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Pulse.Base;

namespace PulseForm
{
    public partial class DownloadMonitor : Form
    {
        public DownloadMonitor()
        {
            InitializeComponent();
        }

        private void DownloadMonitor_Load(object sender, EventArgs e)
        {
            //hook into download manager events
            DownloadManager.Current.PictureAddedToQueue += Current_PictureAddedToQueue;
            DownloadManager.Current.PictureDownloaded += Current_PictureDownloaded;
            DownloadManager.Current.PictureDownloading += Current_PictureDownloading;
            DownloadManager.Current.PictureDownloadingAborted += Current_PictureDownloadingAborted;
            DownloadManager.Current.PictureDownloadProgressChanged += Current_PictureDownloadProgressChanged;
            DownloadManager.Current.PictureRemovedFromQueue += Current_PictureRemovedFromQueue;
            DownloadManager.Current.QueueIsEmpty += Current_QueueIsEmpty;
        }

        void Current_QueueIsEmpty(PictureDownload sender)
        {
            //ShowLogMessage("Queue is now empty");
        }

        void Current_PictureRemovedFromQueue(PictureDownload sender)
        {
            ShowLogMessage(sender.Picture.Id + " started.");
        }

        void Current_PictureDownloadProgressChanged(PictureDownload sender)
        {
            ShowLogMessage(sender.Picture.Id + " @ " + sender.DownloadProgress.ToString() + "%");
        }

        void Current_PictureDownloadingAborted(PictureDownload sender)
        {
            ShowLogMessage(sender.Picture.Id + " ABORTED.");
        }

        void Current_PictureDownloading(PictureDownload sender)
        {
            ShowLogMessage(sender.Picture.Id + " DOWNLOADING.");
        }

        void Current_PictureDownloaded(PictureDownload sender)
        {
            ShowLogMessage(sender.Picture.Id + " COMPLETE.");
        }

        void Current_PictureAddedToQueue(PictureDownload sender)
        {
            ShowLogMessage(sender.Picture.Id + " ADDED TO QUEUE.");
        }

        private void ShowLogMessage(string t)
        {
            if (this.textBox1.InvokeRequired)
            {
                this.BeginInvoke(new Action<string>(ShowLogMessage), t);
                return;
            }
            textBox1.Text += string.Format("{0} - {1}{2}", DateTime.Now.ToLongDateString(), t, Environment.NewLine);
        }
    }
}
