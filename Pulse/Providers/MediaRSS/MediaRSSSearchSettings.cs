using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Windows;
using Pulse.Base;

namespace MediaRSSProvider
{
    public class MediaRSSImageSearchSettings : Pulse.Base.XmlSerializable<MediaRSSImageSearchSettings>
    {
        //most of these settings are in the tbs section

        //for exact searches with &tbs=isz:ex
        //iszw:{imageWidth}
        public int ImageWidth { get; set; }
        //iszh:{imageHeight}
        public int ImageHeight { get; set; }

        //color also goes under "tbs" as well.  Seel MediaRSSImageColors
        // for examples of colors and string format
        public string Color { get; set; }

        public MediaRSSImageSearchSettings(){
           ImageWidth = PictureManager.PrimaryScreenResolution.First;
           ImageHeight = PictureManager.PrimaryScreenResolution.Second;
        }

        public class MediaRSSImageColors
        {
            public string Name { get; private set; }
            public string Value { get; private set; }
            public bool Specific { get; private set; }

            public static string GetColorSearchString(MediaRSSImageColors color)
            {
                if (string.IsNullOrEmpty(color.Value)) return string.Empty;

                if (!color.Specific)
                {
                    return string.Format("ic:{0}", color.Value);
                }

                return string.Format("ic:specific,isc:{0}", color.Value);
            }

            public static List<MediaRSSImageColors> GetColors()
            {
                List<MediaRSSImageColors> gic = new List<MediaRSSImageColors>();
                //default option, no restriction based on color
                gic.Add(new MediaRSSImageColors() { Name = "Any Color", Value = "", Specific=false });
                //Full Color and Black & White are ic:{color,gray}
                gic.Add(new MediaRSSImageColors() { Name = "Full Color", Value = "color", Specific=false });
                gic.Add(new MediaRSSImageColors() { Name = "Black & White", Value = "gray", Specific = false });
                //all specific colors are ic:specific,isc:{red,green,etc}
                gic.Add(new MediaRSSImageColors() { Name = "Red", Value = "red", Specific = true });
                gic.Add(new MediaRSSImageColors() { Name = "Orange", Value = "orange", Specific = true });
                gic.Add(new MediaRSSImageColors() { Name = "Yellow", Value = "yellow", Specific = true });
                gic.Add(new MediaRSSImageColors() { Name = "Green", Value = "green", Specific = true });
                gic.Add(new MediaRSSImageColors() { Name = "Teal", Value = "teal", Specific = true });
                gic.Add(new MediaRSSImageColors() { Name = "Blue", Value = "blue", Specific = true });
                gic.Add(new MediaRSSImageColors() { Name = "Purple", Value = "purple", Specific = true });
                gic.Add(new MediaRSSImageColors() { Name = "Pink", Value = "pink", Specific = true });
                gic.Add(new MediaRSSImageColors() { Name = "White", Value = "white", Specific = true });
                gic.Add(new MediaRSSImageColors() { Name = "Gray", Value = "gray", Specific = true });
                gic.Add(new MediaRSSImageColors() { Name = "Black", Value = "black", Specific = true });
                gic.Add(new MediaRSSImageColors() { Name = "Brown", Value = "brown", Specific = true });

                return gic;
            }
        }
    }
}
