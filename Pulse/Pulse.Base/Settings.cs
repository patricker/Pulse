﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

namespace Pulse.Base
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

        public static readonly string AppPath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

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

        public List<string> BannedImages { get; set; }
        
        public bool ChangeOnTimer { get; set; }
        public bool DownloadOnAppStartup { get; set; }

        public int RefreshInterval { get; set; }
        public IntervalUnits IntervalUnit { get; set; }

        public bool PreFetch { get; set; }
        public int MaxPictureDownloadCount { get; set; }
        public string CachePath { get; set; }

        public string Language { get; set; }
        
        public int ClearInterval { get; set; }
        public bool ClearOldPics { get; set; }

        public bool RunOnWindowsStartup { get; set; }

        //provider settings
        public SerializableDictionary<Guid, ActiveProviderInfo> ProviderSettings { get; set; }

        public Settings()
        {
            Language = CultureInfo.CurrentUICulture.Name;
            ChangeOnTimer = true;
            RefreshInterval = 20;
            IntervalUnit = IntervalUnits.Minutes;

            ClearOldPics = false;
            ClearInterval = 3;
            PreFetch = false;
            MaxPictureDownloadCount = 100;
            CachePath = System.IO.Path.Combine(AppPath, "Cache");
            ProviderSettings = new SerializableDictionary<Guid, ActiveProviderInfo>();
            DownloadOnAppStartup = false;
            RunOnWindowsStartup = false;

            //set wallpaper changer as a default provider for output
            var wID = Guid.NewGuid();
            ProviderSettings.Add(wID, new ActiveProviderInfo("Desktop Wallpaper", wID)
            {
                Active = true,
                ExecutionOrder = 1
            });

            //set wallbase as default for inputs
            var wallID = Guid.NewGuid();
            ProviderSettings.Add(Guid.NewGuid(), new ActiveProviderInfo("Wallbase", wallID)
            {
                Active = true,
                ExecutionOrder = 1
            });
        }

        public string GetProviderSettings(Guid prov) {
            if (!ProviderSettings.ContainsKey(prov)) return string.Empty;

            return ProviderSettings[prov].ProviderConfig;
        }
    }
}
