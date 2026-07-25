using System;
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
        public static readonly string DataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Pulse");

        public static Settings CurrentSettings 
        {
            get
            {
                if(_current==null)
                {
                    // No migration needed per user request - fresh start uses DataPath
                    // Try DataPath first (modern, multi-platform friendly), then AppPath for legacy compat if file exists
                    string dataSettings = Path.Combine(DataPath, "settings.conf");
                    string legacySettings = Path.Combine(AppPath, "settings.conf");

                    if (File.Exists(dataSettings))
                        _current = Settings.LoadFromFile(dataSettings);
                    else if (File.Exists(legacySettings))
                        _current = Settings.LoadFromFile(legacySettings); // still support old location if present, but don't copy
                    else
                        _current = null;

                    _current ??= new Settings();
                }

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
        public int MaxPreviousPictureDepth { get; set; }
        public string CachePath { get; set; }
        public bool CheckForNewPulseVersions { get; set; }
        public bool SkipChangeIfFullScreen { get; set; }

        public string Language { get; set; }
        
        public int ClearInterval { get; set; }
        public bool ClearOldPics { get; set; }

        public bool RunOnWindowsStartup { get; set; }

        //provider settings
        public SerializableDictionary<Guid, ActiveProviderInfo> ProviderSettings { get; set; }

        public static bool IsReparsePoint(string path)
        {
            try
            {
                if (string.IsNullOrEmpty(path)) return false;
                var attr = File.GetAttributes(path);
                return (attr & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint;
            }
            catch { return false; }
        }

        public static string GetFinalPath(string path)
        {
            try
            {
                // Try to resolve symlink/junction to final path via GetFinalPathNameByHandle (Windows)
                // For .NET Framework, use FileInfo LinkTarget if available in .NET 8, else fallback to GetFullPath
                if (IsReparsePoint(path))
                {
                    Log.Logger.Write($"Settings: Blocked reparse point/junction {path}", Log.LoggerLevels.Warnings);
                    return "";
                }

                // Check parent directories for reparse points
                string current = Path.GetFullPath(path);
                while (!string.IsNullOrEmpty(current))
                {
                    string parent = Path.GetDirectoryName(current);
                    if (string.IsNullOrEmpty(parent)) break;
                    if (IsReparsePoint(parent))
                    {
                        Log.Logger.Write($"Settings: Blocked reparse point in parent {parent} for {path}", Log.LoggerLevels.Warnings);
                        return "";
                    }
                    current = parent;
                    // Avoid infinite loop for root
                    if (current == Path.GetDirectoryName(current)) break;
                }

                return Path.GetFullPath(path);
            }
            catch { return Path.GetFullPath(path); }
        }

        public static string GetSafeCachePath(string proposedPath)
        {
            try
            {
                if (string.IsNullOrEmpty(proposedPath))
                    return Path.Combine(DataPath, "Cache");

                string full = GetFinalPath(proposedPath);
                if (string.IsNullOrEmpty(full)) return Path.Combine(DataPath, "Cache");

                full = Path.GetFullPath(full);
                string dataPathFull = Path.GetFullPath(DataPath);
                string localAppData = Path.GetFullPath(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
                string temp = Path.GetFullPath(Path.GetTempPath());

                // Allow paths inside dataPath, localappdata, or temp
                if (full.StartsWith(dataPathFull, StringComparison.OrdinalIgnoreCase) ||
                    full.StartsWith(localAppData, StringComparison.OrdinalIgnoreCase) ||
                    full.StartsWith(temp, StringComparison.OrdinalIgnoreCase))
                {
                    return full;
                }

                // Block system paths like Windows\System32
                string winDir = Path.GetFullPath(Environment.GetFolderPath(Environment.SpecialFolder.Windows));
                if (full.StartsWith(winDir, StringComparison.OrdinalIgnoreCase))
                {
                    Log.Logger.Write($"Settings: Blocked unsafe CachePath {proposedPath} inside Windows dir, using default", Log.LoggerLevels.Warnings);
                    return Path.Combine(DataPath, "Cache");
                }

                // For any other absolute path outside allowed roots, still allow but log warning if suspicious
                // Check for startup folder, ProgramData etc.
                string programData = Path.GetFullPath(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
                string startup = Path.Combine(programData, "Microsoft", "Windows", "Start Menu", "Programs", "Startup");
                if (full.StartsWith(startup, StringComparison.OrdinalIgnoreCase) ||
                    full.StartsWith(Path.Combine(programData, "Microsoft", "Windows", "Start Menu"), StringComparison.OrdinalIgnoreCase))
                {
                    Log.Logger.Write($"Settings: Blocked unsafe CachePath {proposedPath} inside Startup, using default", Log.LoggerLevels.Warnings);
                    return Path.Combine(DataPath, "Cache");
                }

                return full;
            }
            catch
            {
                return Path.Combine(DataPath, "Cache");
            }
        }

        public Settings()
        {
            Language = CultureInfo.CurrentUICulture.Name;
            ChangeOnTimer = true;
            RefreshInterval = 30; // increased from 20 for battery/metered
            IntervalUnit = IntervalUnits.Minutes;

            ClearOldPics = true; // was false, now true to prevent unbounded growth (perf persona)
            ClearInterval = 2; // was 3, now 2 days
            PreFetch = false;
            MaxPictureDownloadCount = 25; // was 100, too heavy for HDD/metered (now 25)
            MaxPreviousPictureDepth = 5;
            CheckForNewPulseVersions = true;
            // Use LocalAppData as default for multi-platform and no migration needed
            try { Directory.CreateDirectory(DataPath); } catch { }
            CachePath = GetSafeCachePath(Path.Combine(DataPath, "Cache"));
            ProviderSettings = new SerializableDictionary<Guid, ActiveProviderInfo>();
            DownloadOnAppStartup = true; // true for Bing default immediate wallpaper (Maya casual)
            RunOnWindowsStartup = false;
            SkipChangeIfFullScreen = false;

            BannedImages = new List<string>();

            //set wallpaper changer as a default provider for output
            ActiveProviderInfo apiWallpaper = new ActiveProviderInfo("Desktop Wallpaper");
            apiWallpaper.Active = true;
            apiWallpaper.ExecutionOrder = 1;

            ProviderSettings.Add(apiWallpaper.ProviderInstanceID, apiWallpaper);

            //set default input providers - Bing (no key, cross-platform) + Wallhaven
            // Old default was Wallbase which is dead, now Bing is primary for casual user (Maya persona)
            ActiveProviderInfo apiBing = new ActiveProviderInfo("Bing Wallpaper (daily, no key needed)");
            ProviderSettings.Add(apiBing.ProviderInstanceID, apiBing);
            apiBing.Active = true;
            apiBing.ExecutionOrder = 1;

            ActiveProviderInfo apiWallbase = new ActiveProviderInfo("Wallhaven");
            ProviderSettings.Add(apiWallbase.ProviderInstanceID, apiWallbase);
            apiWallbase.Active = false; // inactive by default, user can enable with API key
            apiWallbase.ExecutionOrder = 2;
        }

        public string GetProviderSettings(Guid prov) {
            if (!ProviderSettings.ContainsKey(prov)) return string.Empty;

            return ProviderSettings[prov].ProviderConfig;
        }
    }
}
