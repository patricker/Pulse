using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Pulse.Base
{
    public class ProviderManager
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
            var result = new Dictionary<string, Type>();
            var workingDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var providersDirectory = Path.Combine(workingDirectory, "Providers");

            //if no providers directory, return
            if (!Directory.Exists(providersDirectory)) return result;

            //get dll's in provider directory, if none found return empty results
            var files = from x in Directory.GetFiles(providersDirectory)
                        where x.EndsWith(".dll") || x.EndsWith(".exe")
                        select x;

            if (files.Count() == 0) return result;

            //for each providers dll look through the classes for ones that implement the passed in type
            foreach (var f in files)
            {
                var assembly = Assembly.LoadFrom(f);

                var providerType =
                    assembly.GetTypes().Where(type => typeof(IProvider).IsAssignableFrom(type));

                foreach (Type ipType in providerType)
                {
                    //look for a description attribute on the class to use as the name
                    var strName = System.IO.Path.GetFileNameWithoutExtension(f);
                    var attrDescription = ipType.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false);
                    var attrPlatform = ipType.GetCustomAttributes(typeof(Pulse.Base.ProviderPlatformAttribute), true);

                    if (attrPlatform.Length > 0)
                    {
                        var ppa = from ProviderPlatformAttribute ppaI in attrPlatform 
                                  where ppaI.Platform == Environment.OSVersion.Platform
                                  select ppaI;


                        if (ppa.Count() == 0)
                        {
                            continue;
                        }
                    }

                    if (attrDescription.Length >= 1)
                    {
                        strName = (attrDescription[0] as System.ComponentModel.DescriptionAttribute).Description;
                    }

                    if (!result.ContainsKey(strName))
                    {
                        result.Add(strName, ipType);
                    }
                }
            }

            return result;
        }

        public IProvider InitializeProvider(string name)
        {
            return InitializeProvider(name, null);
        }

        public IProvider InitializeProvider(string name, params object[] activationArgs)
        {
            var ipType = Providers[name];

            if (ipType == null) return null;

            return InitializeProvider(ipType, activationArgs);
        }

        public static IProvider InitializeProvider(Type ipType)
        {
            return InitializeProvider(ipType, null);
        }

        public static IProvider InitializeProvider(Type ipType, params object[] activationArgs)
        {
            if (ipType == null) return null;

            var aProv = Activator.CreateInstance(ipType) as IProvider;
            if (activationArgs != null) aProv.Activate(activationArgs);

            aProv.Initialize();

            return aProv;
        }

        public ProviderConfigurationBase InitializeConfigurationWindow(string name)
        {
            Type ipType = HasConfigurationWindow(name);

            var aProv = Activator.CreateInstance(ipType) as ProviderConfigurationBase;

            return aProv;
        }

        public Type HasConfigurationWindow(string name)
        {
            var ipType = Providers[name];

            if (ipType == null) return null;

            //Find any instances of the user control definition attribute on the class
            var attrConfig = ipType.GetCustomAttributes(typeof(ProviderConfigurationUserControlAttribute), false);

            //if none found, return null
            if (attrConfig.Length == 0) return null;

            Type tConfit = (attrConfig[0] as ProviderConfigurationUserControlAttribute).UserControlType;

            return tConfit;
        }

        public static Type HasConfigurationWindow(Type ipType)
        {
            //Find any instances of the user control definition attribute on the class
            var attrConfig = ipType.GetCustomAttributes(typeof(ProviderConfigurationUserControlAttribute), false);

            //if none found, return null
            if (attrConfig.Length == 0) return null;

            Type tConfit = (attrConfig[0] as ProviderConfigurationUserControlAttribute).UserControlType;

            return tConfit;
        }
    }
}
