using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pulse.Base;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Pulse.Base.Providers;

namespace NationalGeographicWallpapers
{
    // RETIRED 2026: ngm.nationalgeographic.com/wallpaper/download -> redirects to /magazine/ since ~2015.
    // Site is now React SPA, no <option value=...xml>. Use Bing Wallpaper API or NASA APOD instead.
    [Obsolete("NatGeo wallpaper XML endpoint dead since 2015. Site is React SPA now. Use Wallhaven/Bing/NASA.")]
    [System.ComponentModel.Description("National Geographic (RETIRED - use Wallhaven/Bing)")]
    [ProviderIcon(typeof(Properties.Resources),"favicon_cb1274471343")]
    public class NationalGeographicProvider : Pulse.Base.IInputProvider
    {
        private string _baseURL = "http://ngm.nationalgeographic.com";
        public Pulse.Base.PictureList GetPictures(Pulse.Base.PictureSearch ps)
        {
            PictureList pl = new PictureList() { FetchDate = DateTime.Now };
            Log.Logger.Write("NationalGeographic provider is RETIRED. ngm.nationalgeographic.com/wallpaper/download died ~2015, now React SPA at /photo-of-the-day. Use Wallhaven, Bing HPImageArchive, or NASA APOD. Returning empty. See WIN10-11-PATH.md", Log.LoggerLevels.Warnings);
            return pl;
        }

        private List<string> ParseXMLPaths(string content)
        {
            //ex: <option value="/wallpaper/2011/September_2011_Wallpapers.xml">September 2011</option>
            Regex regXMLPaths = new Regex("<option value=\"(?<xmlPath>.*)\">(?<title>.*)</option>");

            var matches = regXMLPaths.Matches(content);

            return (from Match c in matches select c.Groups["xmlPath"].Value).ToList();
        }

        private List<Picture> ParsePictures(string xmlUri)
        {
            XDocument xdoc = XDocument.Load(_baseURL + xmlUri);

            List<Picture> pics = (from c in xdoc.Element("PhotoGallery").Elements("photo")
                                  let wElement = c.Element("wallpaper")
                                  let wCount = wElement.Elements().Count()
                                  let wallpaper = wCount == 0 ? wElement.Value.Replace("\t", "").Replace("\n", "") : wElement.Elements().Last().Value.Replace("\t", "").Replace("\n", "")
                                  select new Picture()
                                  {
                                      Url = _baseURL + wallpaper,
                                      Id = System.IO.Path.GetFileNameWithoutExtension(_baseURL + wallpaper)
                                  }).ToList();

            return pics;
        }

        public void Activate(object args)
        {
           
        }

        public void Deactivate(object args)
        {
            
        }

        public void Initialize(object args)
        {
            
        }
    }
}
