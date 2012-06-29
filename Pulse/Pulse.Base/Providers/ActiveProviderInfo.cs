using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Pulse.Base
{
    public class ActiveProviderInfo : XmlSerializable<ActiveProviderInfo>
    {
        public bool Active { get; set; }

        [DisplayName("Name")]
        public string ProviderName { get; set; }

        [Browsable(false)]
        public int ExecutionOrder { get; set; }

        [Browsable(false)]
        public bool AsyncOK { get; set; }

        [Browsable(false)]
        public string ProviderConfig { get; set; }

        [Browsable(false)]
        [System.Xml.Serialization.XmlIgnore()]
        public bool IsConfigurable { get { return ProviderManager.Instance.HasConfigurationWindow(ProviderName) != null; } }
 
        [Browsable(false)]
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
