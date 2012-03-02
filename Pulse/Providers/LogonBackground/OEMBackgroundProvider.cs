﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pulse.Base;
using System.Diagnostics;
using System.Reflection;

namespace LogonBackground
{
    [System.ComponentModel.Description("Logon Background")]
    public class OEMBackgroundProvider : IOutputProvider
    {
        public void Initialize()
        {

        }

        /// <summary>
        /// activates the OEMBackground feature by calling this executable with the /enableoembg argument
        /// </summary>
        /// <param name="args"></param>
        public void Activate(object args)
        {
            CallMeWaitForExit("/enableoembg");
        }

        /// <summary>
        /// deactivates the OEMBackground feature by calling this executable with the /disableoembg argument
        /// </summary>
        /// <param name="args"></param>
        public void Deactivate(object args)
        {
            CallMeWaitForExit("/disableoembg");
        }

        private void CallMeWaitForExit(string args)
        {
            var p = new ProcessStartInfo { Verb = "runas", FileName = Assembly.GetExecutingAssembly().Location, Arguments = args };
            var proc = Process.Start(p);
            proc.WaitForExit();  
        }

        public void ProcessPicture(Picture p)
        {
            OEMBackgroundManager oembm = new OEMBackgroundManager();
            oembm.SetNewPicture(p);
        }


    }
}