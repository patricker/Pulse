﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Pulse.Base;

namespace Pulse.Forms.UI
{
    public partial class DownloadMonitor : Form
    {
        private Dictionary<PictureDownload, PictureStatus> pictureStatusList = new Dictionary<PictureDownload, PictureStatus>();
        private System.Timers.Timer _timer = new System.Timers.Timer(1000);

        public DownloadMonitor()
        {
            InitializeComponent();
        }

        private void DownloadMonitor_Load(object sender, EventArgs e)
        {
            DownloadManager.Current.PictureAddedToQueue += Current_PictureAddedToQueue;
            DownloadManager.Current.PictureRemovedFromQueue += Current_PictureRemovedFromQueue;
            DownloadManager.Current.QueueIsEmpty += Current_QueueIsEmpty;

            _timer.Elapsed += _timer_Elapsed;
            _timer.Start();
        }

        void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            lock (pictureStatusList)
            {
                var controlsSorted = pictureStatusList.Keys
                                            .OrderBy(k => k.Status)
                                            .ThenBy(k => Path.GetFileName(k.Picture.LocalPath));
                this.Invoke((Action)delegate
                {
                    int index = 0;
                    foreach (PictureDownload pd in controlsSorted)
                    {
                        flpDownloads.Controls.SetChildIndex(pictureStatusList[pd], index);
                        index++;
                    }
                });
            }
        }

        void Current_QueueIsEmpty(PictureDownload sender)
        {
            this.Invoke((Action)delegate
            {
                
            });
        }

        void Current_PictureAddedToQueue(PictureDownload sender)
        {
            if (pictureStatusList.ContainsKey(sender)) return;

            this.Invoke((Action)delegate
            {
                PictureStatus ps = new PictureStatus();
                flpDownloads.Controls.Add(ps);
                ps.Dock = DockStyle.Top;

                pictureStatusList.Add(sender, ps);

                ps.SetPicture(sender);
            });
        }

        void Current_PictureRemovedFromQueue(PictureDownload sender)
        {
            lock (pictureStatusList)
            {
                if (!pictureStatusList.ContainsKey(sender)) return;

                this.Invoke((Action)delegate
                {
                    PictureStatus ps = pictureStatusList[sender];

                    ps.UnhookEvents();
                    flpDownloads.Controls.Remove(ps);

                    pictureStatusList.Remove(sender);
                });
            }
        }

        private void DownloadMonitor_FormClosing(object sender, FormClosingEventArgs e)
        {
            DownloadManager.Current.PictureAddedToQueue -= Current_PictureAddedToQueue;
            DownloadManager.Current.PictureRemovedFromQueue -= Current_PictureRemovedFromQueue;
            DownloadManager.Current.QueueIsEmpty -= Current_QueueIsEmpty;
            _timer.Stop();
            
            foreach (PictureStatus pd in pictureStatusList.Values)
            {
                pd.UnhookEvents();
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnClearQueue_Click(object sender, EventArgs e)
        {
            _timer.Stop();

            lock (pictureStatusList)
            {
                DownloadManager.Current.ClearQueue();

                var t = pictureStatusList.ToList();

                foreach (KeyValuePair<PictureDownload, PictureStatus> ps in t)
                {
                    flpDownloads.Controls.Remove(ps.Value);
                    pictureStatusList.Remove(ps.Key);
                }
            }

            _timer.Start();
        }
    }
}
