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
        public IProvider CurrentProvider { get; set; }

        public Dictionary<string,Type> Providers { 
            get{
                if (_Providers == null) { _Providers = FindProviders(); }
                return _Providers;
            }
        }

        private Dictionary<string, Type> _Providers;

        private Dictionary<string, Type> FindProviders()
        {
            var result = new Dictionary<string, Type>();
            var workingDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var providersDirectory = Path.Combine(workingDirectory, "Providers");

            //if no providers directory, return
            if (!Directory.Exists(providersDirectory)) return result;


            var files = from x in Directory.GetFiles(providersDirectory)
                        where x.EndsWith(".dll")
                        select x;

            if (files.Count() == 0) return result;

            //for each providers dll look through the classes for ones that implement iProvider
            foreach (var f in files)
            {
                var assembly = Assembly.LoadFrom(f);

                var providerType =
                    assembly.GetTypes().Where(type => typeof(IProvider).IsAssignableFrom(type));

                foreach (Type ipType in providerType)
                {
                    ////get the type and initialize it
                    //var aProv = Activator.CreateInstance(ipType) as IProvider;
                    //aProv.Initialize();

                    //look for a description attribute on the class to use as the name
                    var strName = System.IO.Path.GetFileNameWithoutExtension(f);
                    var attrDescription = ipType.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false);

                    if (attrDescription.Length >= 1)
                    {
                        strName = (attrDescription[0] as System.ComponentModel.DescriptionAttribute).Description;
                    }

                    result.Add(strName, ipType);
                }
            }

            return result;
        }

        public IProvider InitializeProvider(string name)
        {
            var ipType = Providers[name];

            if (ipType == null) return null;

            return InitializeProvider(ipType);
        }

        public static IProvider InitializeProvider(Type ipType)
        {
            if (ipType == null) return null;

            var aProv = Activator.CreateInstance(ipType) as IProvider;
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
    }
}
