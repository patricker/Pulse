using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Text.RegularExpressions;

namespace NASAAPOD
{
    [System.ComponentModel.Description("NASA Astronomy Picture of the Day")]
    public class NASAAPODProviderza:Pulse.Base.IInputProvider
    {
        private static string _url = "http://apod.nasa.gov/apod/archivepix.html";

        public Pulse.Base.PictureList GetPictures(Pulse.Base.PictureSearch ps)
        {
            WebClient wc = new WebClient();

            //download archive webpage
            var pg = wc.DownloadString(_url);

            //regex out the links to the individual pages
            Regex reg = new Regex("<a href=\"(?<picPage>ap.*\\.html)\">");
            Regex regPic = new Regex("<IMG SRC=\"(?<picURL>image.*)\"");
            var matches = reg.Matches(pg);

            var pl = new Pulse.Base.PictureList();

            for (int i = 0; i < ps.MaxPictureCount; i++)
            {
                var photoPage = wc.DownloadString("http://apod.nasa.gov/apod/" + matches[i].Groups["picPage"].Value);
                var photoURL = regPic.Match(photoPage).Groups["picURL"].Value;

                pl.Pictures.Add(new Pulse.Base.Picture() { Url = "http://apod.nasa.gov/apod/" + photoURL, Id = System.IO.Path.GetFileNameWithoutExtension(photoURL) });
            }

            return pl;
        }

        public void Activate(object args)
        {
            
        }

        public void Deactivate(object args)
        {
            
        }

        public void Initialize()
        {
            
        }
    }
}
