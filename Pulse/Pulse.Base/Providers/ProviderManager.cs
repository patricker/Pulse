using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Pulse.Base.Providers;

namespace Pulse.Base
{
    public partial class ProviderManager
    {
        public static ProviderManager Instance
        {
            get
            {
                if (_Instance == null) _Instance = new ProviderManager();

                return _Instance;
            }
        }

        private static ProviderManager _Instance;

        public Dictionary<string, Type> Providers
        {
            get
            {
                if (_Providers == null) { _Providers = FindProviders(); }
                return _Providers;
            }
        }

        private Dictionary<string, Type> _Providers;

        private ProviderManager()
        {

        }

        public Dictionary<string, Type> GetProvidersByType<T>() where T : IProvider
        {
            return Providers.Where(type => typeof(T).IsAssignableFrom(type.Value)).ToDictionary(type => type.Key, type => type.Value);
        }

        private Dictionary<string, Type> FindProviders()
        {
#if NET8_0_OR_GREATER
            // On .NET 8, use modern ALC-based loader if available
            try
            {
                var modernMethod = GetType().GetMethod("FindProvidersModern", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (modernMethod != null)
                {
                    var modernResult = modernMethod.Invoke(this, null) as Dictionary<string, Type>;
                    if (modernResult != null && modernResult.Count > 0)
                        return modernResult;
                }
            }
            catch { /* fallback to legacy */ }
#endif

            var result = new Dictionary<string, Type>();
            // FIX: Use AppContext.BaseDirectory for single-file + Environment.ProcessPath for exe dir
            string workingDirectory = AppContext.BaseDirectory;
            try
            {
                string exeDir = Path.GetDirectoryName(Environment.ProcessPath ?? AppContext.BaseDirectory) ?? AppContext.BaseDirectory;
                if (!string.IsNullOrEmpty(exeDir) && Directory.Exists(Path.Combine(exeDir, "Providers")))
                    workingDirectory = exeDir;
            }
            catch { }

            var providersDirectory = Path.Combine(workingDirectory, "Providers");

            // Fallback to LocalAppData\Pulse\Providers
            if (!Directory.Exists(providersDirectory))
            {
                try
                {
                    var alt = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Pulse", "Providers");
                    if (Directory.Exists(alt)) providersDirectory = alt;
                    else return result;
                }
                catch { return result; }
            }

            var files = from x in Directory.GetFiles(providersDirectory)
                        where x.EndsWith(".dll") || x.EndsWith(".exe")
                        select x;

            if (files.Count() == 0) return result;

            foreach (var f in files)
            {
                Assembly assembly;
                try
                {
                    assembly = Assembly.LoadFrom(f);
                }
                catch
                {
                    // Skip non-.NET dll or native dependency missing
                    continue;
                }

                var providerType =
                    assembly.GetTypes().Where(type => typeof(IProvider).IsAssignableFrom(type));

                foreach (Type ipType in providerType)
                {
                    // Skip retired providers marked with ObsoleteAttribute (GoogleImages, NatGeo) - hide from UI and loading
                    if (ipType.IsDefined(typeof(ObsoleteAttribute), false))
                    {
                        Log.Logger.Write($"Provider '{ipType.FullName}' skipped - marked as retired/Obsolete", Log.LoggerLevels.Info);
                        continue;
                    }

                    //look for a description attribute on the class to use as the name
                    var strName = GetProviderName(ipType);
                    var attrPlatform = GetSupportedPlatformsForType(ipType);
                    
                    //if this provider has a list of supported platforms
                    // Windows 10/11 fix: Without manifest, OS returns 6.2. With manifest, 10.0.
                    // Old providers marked 6.1 (Win7) and 6.2 (Win8) should also run on 10.0+.
                    // New logic: allow if OS major >= required major, and if major equal, minor >= required minor (or required 0 = any)
                    if (attrPlatform.Any())
                    {
                        var currentOS = Environment.OSVersion;
                        var ppa = from ProviderPlatformAttribute ppaI in attrPlatform 
                                  where ppaI.Platform == currentOS.Platform
                                        && (ppaI.MajorVersion == 0 || 
                                            currentOS.Version.Major > ppaI.MajorVersion ||
                                            (currentOS.Version.Major == ppaI.MajorVersion && 
                                             (ppaI.MinorVersion == 0 || currentOS.Version.Minor >= ppaI.MinorVersion)))
                                  select ppaI;

                        // BUT none of the platforms are supported by this computer, then skip this provider
                        if (!ppa.Any())
                        {
                            Log.Logger.Write(string.Format("Provider '{0}' skipped - requires platform {1} but current is {2} {3}.{4}", 
                                strName, 
                                string.Join(",", attrPlatform.Select(a => $"{a.Platform} {a.MajorVersion}.{a.MinorVersion}")),
                                currentOS.Platform, currentOS.Version.Major, currentOS.Version.Minor), Log.LoggerLevels.Info);
                            continue;
                        }
                    }

                    // Allow duplicate display names by auto-renaming with suffix (e.g., Wallhaven (2))
                    // This enables power users to have 2 DLLs with same Description for testing variants
                    // Instances via Guid already work for same provider type with different configs
                    string uniqueName = strName;
                    int suffix = 2;
                    while (result.ContainsKey(uniqueName))
                    {
                        // If same Type already registered from same file, skip duplicate
                        if (result[uniqueName] == ipType)
                            break;

                        uniqueName = $"{strName} ({suffix})";
                        suffix++;
                        if (suffix > 10) // prevent infinite loop
                        {
                            Log.Logger.Write($"Provider duplicate limit reached for '{strName}' in {f}", Log.LoggerLevels.Warnings);
                            break;
                        }
                    }

                    if (!result.ContainsKey(uniqueName))
                    {
                        result.Add(uniqueName, ipType);
                        if (uniqueName != strName)
                        {
                            Log.Logger.Write($"Provider duplicate renamed '{strName}' -> '{uniqueName}' from {f}", Log.LoggerLevels.Info);
                        }
                    }
                }
            }

            return result;
        }

        public static string GetProviderName(Type ipType)
        {
            string strName = "";

            var attrDescription = ipType.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false);
            if (attrDescription.Length >= 1)
            {
                strName = (attrDescription[0] as System.ComponentModel.DescriptionAttribute).Description;
            }
            else
            {
                strName = ipType.FullName;
            }

            return strName;
        }

        public static List<Pulse.Base.ProviderPlatformAttribute> GetSupportedPlatformsForType(Type ipType)
        {
            var attrPlatform = ipType.GetCustomAttributes(typeof(Pulse.Base.ProviderPlatformAttribute), true);

            return (from Pulse.Base.ProviderPlatformAttribute c in attrPlatform select c).ToList();
        }

        public static bool GetAsyncStatusForType(Type ipType)
        {
            var attrPlatform = ipType.GetCustomAttributes(typeof(Pulse.Base.Providers.ProviderRunsAsyncAttribute), true);

            return (from Pulse.Base.Providers.ProviderRunsAsyncAttribute c in attrPlatform select c.AsyncOK).FirstOrDefault();
        }

        public Image GetProviderIcon(string provName)
        {
            return GetProviderIcon(Providers[provName]);
        }

        public static Image GetProviderIcon(Type ipType)
        {
            var attrImage = ipType.GetCustomAttributes(typeof(Pulse.Base.Providers.ProviderIconAttribute), true);

            if (attrImage.Length == 0) return null;

            return ((Pulse.Base.Providers.ProviderIconAttribute)attrImage[0]).ProviderIcon;
        }

        public IProvider InitializeProvider(string name, object initArgs)
        {
            return InitializeProvider(name, initArgs, null);
        }

        public IProvider InitializeProvider(string name, object initArgs, params object[] activationArgs)
        {
            if (!Providers.ContainsKey(name)) return null;

            var ipType = Providers[name];

            if (ipType == null) return null;

            return InitializeProvider(ipType, initArgs, activationArgs);
        }

        public static IProvider InitializeProvider(Type ipType, object initArgs)
        {
            return InitializeProvider(ipType, initArgs, null);
        }

        public static IProvider InitializeProvider(Type ipType, object initArgs, params object[] activationArgs)
        {
            if (ipType == null) return null;

            var aProv = Activator.CreateInstance(ipType) as IProvider;
            if (activationArgs != null) aProv.Activate(activationArgs);

            aProv.Initialize(initArgs);

            return aProv;
        }

        public IProviderConfigurationEditor InitializeConfigurationWindow(string name)
        {
            Type ipType = HasConfigurationWindow(name);

            var aProv = Activator.CreateInstance(ipType) as IProviderConfigurationEditor;

            return aProv;
        }

        public static IProviderConfigurationEditor InitializeConfigurationWindow(Type ipType)
        {
            var aProv = Activator.CreateInstance(ipType) as IProviderConfigurationEditor;

            return aProv;
        }

        public Type HasConfigurationWindow(string name)
        {
            if (!Providers.ContainsKey(name)) return null;

            var ipType = Providers[name];

            return HasConfigurationWindow(ipType);
        }

        public static Type HasConfigurationWindow(Type ipType)
        {
            //Find any instances of the user control definition attribute on the class
            var attrConfig = ipType.GetCustomAttributes(typeof(ProviderConfigurationAttribute), false);

            //if none found, return null
            if (attrConfig.Length == 0) return null;

            //for storing editor type
            Type tConfit = null;

            //check if we have a ProviderConfigurationUserControlAttribute, or just a configuration class defined
            var definedEditor = (from ProviderConfigurationAttribute c in attrConfig
                                 where c.GetType() == typeof(ProviderConfigurationUserControlAttribute)
                                 select c);

            if (definedEditor.Any())
            {
                ProviderConfigurationAttribute pcattrType = definedEditor.Single();
                tConfit = pcattrType.Type;
            }
            else
            {
                ProviderConfigurationAttribute pcattrType = attrConfig[0] as ProviderConfigurationAttribute;
                tConfit = pcattrType.Type;

                if (pcattrType.GetType() == typeof(ProviderConfigurationClassAttribute))
                {
                    tConfit = typeof(ProviderConfigurationPropertyGrid<>).MakeGenericType(tConfit);
                }
            }

            return tConfit;
        }

        public object InitializeNewProviderSettings(string pname)
        {
            if (!Providers.ContainsKey(pname)) return null;

            return InitializeNewProviderSettings(Providers[pname]);
        }

        public static object InitializeNewProviderSettings(Type ipType)
        {
            Type t = GetProviderSettingsClass(ipType);

            if (t == null) return null;

            var provSettings = Activator.CreateInstance(t);

            return provSettings;
        }

        public Type GetProviderSettingsClass(string pname)
        {
            if (!Providers.ContainsKey(pname)) return null;

            return GetProviderSettingsClass(Providers[pname]);
        }

        public static Type GetProviderSettingsClass(Type ipType)
        {
            var attrConfig = ipType.GetCustomAttributes(typeof(ProviderConfigurationClassAttribute), false);

            //if none found, return null
            if (attrConfig.Length == 0) return null;

            //return type
            return ((ProviderConfigurationClassAttribute)attrConfig[0]).Type;
        }
    }
}
