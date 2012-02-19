using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Pulse.Base
{
    public class Picture
    {
        /// <summary>
        /// Id is the file name, without extension, used to save the file
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// The full URL to the picture, used for downloading and banning
        /// </summary>
        public string Url { get; set; }

        public Picture()
        {
        }
    }

    public class PictureList : XmlSerializable<PictureList>
    {
        public string ProviderName { get; set; }
        public DateTime FetchDate { get; set; }
        public int SearchSettingsHash { get; set; }
        
        public List<Picture> Pictures { get; set; }

        public PictureList()
        {
            Pictures = new List<Picture>();
        }
    }
}
