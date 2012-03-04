using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LocalDirectory
{
    public class LocalDirectorySettings : Pulse.Base.XmlSerializable<LocalDirectorySettings>
    {
        public string Directory { get; set; }
        public string Extensions { get; set; }

        public LocalDirectorySettings()
        {
            //set default directory to users Pictures folder
            Directory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            Extensions = "jpg;jpeg;png;bmp";
        }
    }
}
