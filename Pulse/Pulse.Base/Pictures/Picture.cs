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
        /// Id is the file name, without extension, used to save the file when downloaded from the internet
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// The full URL to the picture, used for downloading and banning
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// The local path to the saved file
        /// </summary>
        public string LocalPath { get; set; }

        /// <summary>
        /// may be used for storing other properties of the file such as dimensions, thumbnail url, etc...
        /// </summary>
        public SerializableDictionary<string, string> Properties { get; set; }

        public Picture()
        {
            Properties = new SerializableDictionary<string, string>();
        }
    }
}
