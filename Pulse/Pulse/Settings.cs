using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Pulse.Base;

namespace Pulse
{
    public class Settings : XmlSerializable
    {
        public Settings()
        {
            Provider = "Rewalls";
            Language = CultureInfo.CurrentUICulture.Name;
            DownloadAutomatically = true;
            RefreshInterval = 20;
            SkipLowRes = true;
            GetMaxRes = false;
            ClearOldPics = true;
            ClearInterval = 3;
            switch (DateTime.Now.Month)
            {
                case 12:
                case 1:
                case 2:
                    Search = Properties.Resources.Winter;
                    break;
                case 3:
                case 4:
                case 5:
                    Search = Properties.Resources.Spring;
                    break;
                case 6:
                case 7:
                case 8:
                    Search = Properties.Resources.Summer;
                    break;
                case 9:
                case 10:
                case 11:
                    Search = Properties.Resources.Autumn;
                    break;
            }

            ChangeLogonBg = false;
        }

        public string Provider { get; set; }
        public string Search { get; set; }
        public string Language { get; set; }
        public bool DownloadAutomatically { get; set; }
        public double RefreshInterval { get; set; }
        public bool SkipLowRes { get; set; }
        public bool GetMaxRes { get; set; }
        public string Filter { get; set; }
        public int ClearInterval { get; set; }
        public bool ClearOldPics { get; set; }
        public bool ChangeLogonBg { get; set; }
    }
}
