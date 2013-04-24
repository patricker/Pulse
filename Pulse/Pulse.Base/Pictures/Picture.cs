using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Drawing;
using System.Net;

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

        private Image _cachedThumb = null;

        public Picture()
        {
            Properties = new SerializableDictionary<string, string>();
        }

        public string CalculateLocalPath(string baseFolder)
        {
            return Path.Combine(baseFolder, Id + Path.GetExtension(Url));
        }

        /// <summary>
        /// Does the file appear to be present and in good shape (size > 0kb)
        /// </summary>
        public bool IsGood
        {
            get
            {
                if (string.IsNullOrEmpty(LocalPath)) return false;

                FileInfo fi = new FileInfo(LocalPath);
                return (fi.Exists && fi.Length > 0);
            }
        }

        public Image GetThumbnail()
        {
            if (_cachedThumb == null)
            {
                if (Properties.ContainsKey(StandardProperties.Thumbnail))
                {
                    _cachedThumb = Image.FromStream(
                            new WebClient().OpenRead(
                                Properties[Picture.StandardProperties.Thumbnail]));
                }
                else
                {
                    Uri uriUrl = new Uri(Url);
                    if (uriUrl.Scheme == "file")
                    {
                        _cachedThumb = PictureManager.ShrinkImage(Url, 0, 150);
                    }
                }
            }

            return _cachedThumb;
        }

        public static class StandardProperties
        {
            public static readonly string Thumbnail = "thumb";
            public static readonly string Referrer = "referrer";
            public static readonly string BanImageKey = "banImageKey";
        }
    }
}
