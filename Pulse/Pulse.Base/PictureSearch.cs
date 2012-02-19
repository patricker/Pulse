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
        public IProvider SearchProvider { get; set; }
        //search query
        public string SearchString {get; set; }
        //ignored works (not supported by all providers
        public List<string> IgnoredWords { get; set; }
        //banned images
        public List<string> BannedURLs { get; set; }
        //the providers search string.  This can be in whatever format the provider needs
        // for most I expect it will be in XML which can then be deserialized to a custom settings object
        // in the provider.
        public string ProviderSearchSettings { get; set; }
        //Maximum number of pictures to download from provider.  0 means all
        public int MaxPictureCount { get; set; }
        //whether or not the images should be prefetched
        public bool PreFetch { get; set; }
        //Where to save the pictures
        public string SaveFolder { get; set; }

        public PictureSearch()
        {
            IgnoredWords = new List<string>();
            BannedURLs = new List<string>();
        }

        public int GetSearchHash()
        {
            return Save().GetHashCode();
        }
    }
}
