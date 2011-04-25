using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Pulse.Base;

namespace Pulse
{
    public class ProviderManager
    {
        public Provider CurrentProvider { get; set; }

        public List<Provider> Providers { get; private set; }

        public void FindProviders()
        {
            if (Directory.Exists(App.Path + "\\Providers"))
            {
                Providers = new List<Provider>();
                var files = from x in Directory.GetFiles(App.Path + "\\Providers")
                            where x.EndsWith(".dll")
                            select x;
                foreach (var f in files)
                {
                    var p = new Provider(f);
                    Providers.Add(p);
                    if (App.Settings.Provider == p.Name)
                    {
                        CurrentProvider = p;
                        p.Load();
                    }
                }
            }
        }
    }
}
