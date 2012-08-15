using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Drawing;

namespace WallpaperSetter
{
    public class WallpaperSetterSettings : Pulse.Base.XmlSerializable<WallpaperSetterSettings>
    {
        //Picture Position
        public PicturePositions Position { get; set; }

        //background color mode
        [DisplayName("Background Color Mode")]
        public BackgroundColorModes BackgroundColorMode { get; set; }

        //Windows Background Color
        [XmlIgnore()]
        [Browsable(true)]
        public Color Color { get; set; }

        //used for serialization since "Color" objects aren't serializable
        [XmlElement("Color")]
        [Browsable(false)]
        public string ClrHtml
        {
            get { return ColorTranslator.ToHtml(Color); }
            set { Color = ColorTranslator.FromHtml(value); }
        }

        public WallpaperSetterSettings()
        {
            Position = PicturePositions.NotSet;
            BackgroundColorMode = BackgroundColorModes.NotSet;
        }

        public enum PicturePositions  {
            Fill,
            Fit,
            Stretch,
            Tile,
            Center,
            NotSet
        }

        public enum BackgroundColorModes
        {
            Specific,
            Computed,
            NotSet
        }
    }
}
