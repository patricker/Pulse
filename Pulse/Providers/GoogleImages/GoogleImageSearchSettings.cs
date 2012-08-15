using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Windows;
using Pulse.Base;

namespace GoogleImages
{
    public class GoogleImageSearchSettings : Pulse.Base.XmlSerializable<GoogleImageSearchSettings>
    {
        //most of these settings are in the tbs section

        //for exact searches with &tbs=isz:ex
        //iszw:{imageWidth}
        public int ImageWidth { get; set; }
        //iszh:{imageHeight}
        public int ImageHeight { get; set; }

        //color also goes under "tbs" as well.  See GoogleImageColors
        // for examples of colors and string format
        public string Color { get; set; }

        public GoogleImageSearchSettings(){
           ImageWidth = PictureManager.PrimaryScreenResolution.First;
           ImageHeight = PictureManager.PrimaryScreenResolution.Second;
        }

        public class GoogleImageColors
        {
            public string Name { get; private set; }
            public string Value { get; private set; }
            public bool Specific { get; private set; }

            public static string GetColorSearchString(GoogleImageColors color)
            {
                if (string.IsNullOrEmpty(color.Value)) return string.Empty;

                if (!color.Specific)
                {
                    return string.Format("ic:{0}", color.Value);
                }

                return string.Format("ic:specific,isc:{0}", color.Value);
            }

            public static List<GoogleImageColors> GetColors()
            {
                List<GoogleImageColors> gic = new List<GoogleImageColors>();
                //default option, no restriction based on color
                gic.Add(new GoogleImageColors() { Name = "Any Color", Value = "", Specific=false });
                //Full Color and Black & White are ic:{color,gray}
                gic.Add(new GoogleImageColors() { Name = "Full Color", Value = "color", Specific=false });
                gic.Add(new GoogleImageColors() { Name = "Black & White", Value = "gray", Specific = false });
                //all specific colors are ic:specific,isc:{red,green,etc}
                gic.Add(new GoogleImageColors() { Name = "Red", Value = "red", Specific = true });
                gic.Add(new GoogleImageColors() { Name = "Orange", Value = "orange", Specific = true });
                gic.Add(new GoogleImageColors() { Name = "Yellow", Value = "yellow", Specific = true });
                gic.Add(new GoogleImageColors() { Name = "Green", Value = "green", Specific = true });
                gic.Add(new GoogleImageColors() { Name = "Teal", Value = "teal", Specific = true });
                gic.Add(new GoogleImageColors() { Name = "Blue", Value = "blue", Specific = true });
                gic.Add(new GoogleImageColors() { Name = "Purple", Value = "purple", Specific = true });
                gic.Add(new GoogleImageColors() { Name = "Pink", Value = "pink", Specific = true });
                gic.Add(new GoogleImageColors() { Name = "White", Value = "white", Specific = true });
                gic.Add(new GoogleImageColors() { Name = "Gray", Value = "gray", Specific = true });
                gic.Add(new GoogleImageColors() { Name = "Black", Value = "black", Specific = true });
                gic.Add(new GoogleImageColors() { Name = "Brown", Value = "brown", Specific = true });

                return gic;
            }
        }
    }
}
