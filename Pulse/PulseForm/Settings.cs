using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.IO;
using Pulse.Base;
using System.Reflection;

namespace PulseForm
{
    public class Settings : XmlSerializable<Settings>
    {
        public enum IntervalUnits
        {
            Seconds, 
            Minutes,
            Hours,
            Days
        }

        public static readonly string Path = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public static Settings CurrentSettings 
        {
            get
            {
                //load settings from file
                if(_current==null)
                    _current = Settings.LoadFromFile("settings.conf") ?? new Settings();

                return _current;
            }
            set {
                _current = value;
            }
        }

        private static Settings _current = null;

        public string Provider { get; set; }

        public string Search { get; set; }
        public string Filter { get; set; }
        public List<string> BannedImages { get; set; }
        
        public bool DownloadAutomatically { get; set; }
        public bool DownloadOnAppStartup { get; set; }
        public int RefreshInterval { get; set; }
        public bool PreFetch { get; set; }
        public int MaxPictureDownloadCount { get; set; }
        public string CachePath { get; set; }

        public string Language { get; set; }
        
        public int ClearInterval { get; set; }
        public IntervalUnits IntervalUnit { get; set; }
        public bool ClearOldPics { get; set; }

        //provider settings
        public SerializableDictionary<string, ActiveProviderInfo> ProviderSettings { get; set; }


        public Settings()
        {
            Provider = "Wallbase";
            Language = CultureInfo.CurrentUICulture.Name;
            DownloadAutomatically = true;
            RefreshInterval = 20;
            ClearOldPics = false;
            ClearInterval = 3;
            PreFetch = false;
            MaxPictureDownloadCount = 100;
            CachePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Cache");
            ProviderSettings = new SerializableDictionary<string, ActiveProviderInfo>();
            DownloadOnAppStartup = false;

            //set wallpaper changer as a default provider for output
            ProviderSettings.Add("Desktop Wallpaper", new ActiveProviderInfo()
            {
                Active = true,
                AsyncOK = false,
                ExecutionOrder = 1,
                ProviderConfig = string.Empty,
                ProviderName = "Desktop Wallpaper"
            });

            Search = "Nature";
        }
    }
}
