using System;
using System.ComponentModel;

namespace Pulse.Base
{
    public class ActiveProviderInfo : XmlSerializable<ActiveProviderInfo>
    {
        public Guid ProviderInstanceID { get; set; } = Guid.NewGuid();
        public bool Active { get; set; }
        public string ProviderLabel { get; set; } = "";
        public string ProviderName { get; set; } = "";
        public int ExecutionOrder { get; set; }
        public bool AsyncOK { get; set; }
        public string ProviderConfig { get; set; } = "";

        [System.Xml.Serialization.XmlIgnore()]
        public bool IsConfigurable => false;

        [System.Xml.Serialization.XmlIgnore()]
        public PictureList? SearchResults { get; set; }

        public ActiveProviderInfo() { }

        public ActiveProviderInfo(string name) : this()
        {
            ProviderName = name;
            ProviderLabel = name;
        }

        public ActiveProviderInfo(string name, Guid id) : this(name)
        {
            ProviderInstanceID = id;
        }

        public int GetComparisonHashCode()
        {
            return ProviderConfig.GetHashCode() * 31 + Active.GetHashCode();
        }
    }
}
