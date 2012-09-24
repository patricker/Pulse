using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using Pulse.Base;
using System.IO;
using System.Diagnostics;

namespace PulseForm
{
    public partial class frmPulseHost : Form
    {
        public static PulseRunner Runner;
        
        public frmPulseHost()
        {
            InitializeComponent();
        }

        private void frmPulseHost_Load(object sender, EventArgs e)
        {
            //Hook global exception handler for the app domain
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            //create and initialize the runner
            Runner = new PulseRunner();
            Runner.Initialize();

            if (Runner.CurrentInputProviders.Count == 0)
            {
                MessageBox.Show("A provider is not currently selected.  Please choose a wallpaper provider from the options menu.");
            }
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            Log.Logger.Write("Unhandled exception! Error Details: " + args.ExceptionObject.ToString(), Log.LoggerLevels.Errors);

            MessageBox.Show("Pulse has encountered an exception and will exit.  The exception has been logged to the Pulse Log file.  Please post on the Issue Tracker page located on the Pulse Website @ http://pulse.codeplex.com/.  Exception details: " + ((Exception)args.ExceptionObject).Message, 
                "Pulse Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            frmPulseOptions po = new frmPulseOptions();
            po.UpdateSettings += new EventHandler(po_UpdateSettings);
            po.Show();
        }

        void po_UpdateSettings(object sender, EventArgs e)
        {
            Runner.UpdateFromConfiguration();
            Runner.SkipToNextPicture();
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void openCacheFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(Settings.CurrentSettings.CachePath)) Directory.CreateDirectory(Settings.CurrentSettings.CachePath);
            //launch directory
            Process.Start(Settings.CurrentSettings.CachePath);
        }

        private void banImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Runner.CurrentBatch != null && Runner.CurrentBatch.CurrentPictures.Any())
            {
                foreach (Picture p in Runner.CurrentBatch.CurrentPictures)
                {
                    if (MessageBox.Show(string.Format("Ban '{0}'?", p.Url), "Image Ban", MessageBoxButtons.YesNo) != System.Windows.Forms.DialogResult.Yes)
                    {
                        return;
                    }

                    Runner.BanPicture(p.Url, p.LocalPath);
                }
            }
        }

        private void nextPictureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Runner.SkipToNextPicture();
        }
    }
}
