using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Pulse.Base
{
    public class Provider
    {
        private readonly string path;
        private Assembly assembly;
        private IProvider provider;

        public bool IsLoaded { get; private set; }
        public string Name { get; private set; }

        public Provider(string file)
        {
            path = file;
            Name = path.Substring(path.LastIndexOf(@"\") + 1, path.Length - path.LastIndexOf(@"\") - 5);
        }

        public void Load()
        {
            assembly = Assembly.LoadFrom(path);
            Type providerType =
                assembly.GetTypes().FirstOrDefault(type => typeof(IProvider).IsAssignableFrom(type));
            if (providerType == null)
            {
                IsLoaded = false;
                throw new TypeLoadException(String.Format("Failed to find IProvider in {0}", path));
            }

            provider = Activator.CreateInstance(providerType) as IProvider;
            provider.Initialize();
            IsLoaded = true;
        }

        public List<Picture> GetPictures(string search, bool skipLowRes, bool getMaxRes, List<string> filterKeywords)
        {
            return provider.GetPictures(search, skipLowRes, getMaxRes, filterKeywords);
        }

        public int GetResultsCount()
        {
            return provider.GetResultsCount();
        }
    }
}
