using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pulse.Base
{
    public class PictureSearch : XmlSerializable<PictureSearch>
    {
        //provider to use for search
        [System.Xml.Serialization.XmlIgnore()]
        public IInputProvider SearchProvider { get; set; }
        //for serialization
        public string SearchProviderXML
        {
            get
            {
                return SearchProvider.GetType().ToString();
            }
            set { }
        }

        //search query
        public string SearchString {get; set; }
        //banned images
        public List<string> BannedURLs { get; set; }
        //the providers search string.  This can be in whatever format the provider needs
        // for most I expect it will be in XML which can then be deserialized to a custom settings object
        // in the provider.
        public string ProviderSearchSettings { get; set; }
        //Maximum number of pictures to download from provider.  0 means all
        public int MaxPictureCount { get; set; }
        //Where to save the pictures
        public string SaveFolder { get; set; }

        public PictureSearch()
        {
            BannedURLs = new List<string>();
        }

        public int GetSearchHash()
        {
            return Save().GetHashCode();
        }
    }
}
