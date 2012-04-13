using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace PulseForm
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmPulseHost));
            
            frmPulseHost fph = new frmPulseHost();

            TrayIconWrapper icon = new TrayIconWrapper(fph);
            //icon.Icon = ((System.Drawing.Icon)(resources.GetObject("niOptions.Icon")));
            icon.Icon = Properties.Resources.PulseIcon;
            icon.Text = "Pulse";
            icon.Visible = true;
            icon.ContextMenuStrip = fph.cmsTrayIcon;

            icon.Run();

           
        }
    }
}
