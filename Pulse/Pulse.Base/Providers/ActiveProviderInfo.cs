using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pulse.Base
{
    public class ActiveProviderInfo : XmlSerializable<ActiveProviderInfo>
    {
        public bool Active { get; set; }
        public string ProviderName { get; set; }
        public int ExecutionOrder { get; set; }
        public bool AsyncOK { get; set; }
        public string ProviderConfig { get; set; }

        [System.Xml.Serialization.XmlIgnore()]
        public bool IsConfigurable { get { return ProviderManager.Instance.HasConfigurationWindow(ProviderName) != null; } }
        [System.Xml.Serialization.XmlIgnore()]
        public IProvider Instance { 
            get {
                if (_instance == null) _instance = ProviderManager.Instance.InitializeProvider(ProviderName);

                return _instance;
            } 
        }

        private IProvider _instance;

        public int GetComparisonHashCode()
        {
            return Save().GetHashCode();
        }
    }
}
