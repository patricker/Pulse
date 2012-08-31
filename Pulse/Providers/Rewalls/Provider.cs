using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Pulse.Base;

namespace Rewalls
{
    [System.ComponentModel.Description("Rewalls")]
    public class Provider : IInputProvider
    {
        private WebClient webClient;

        private const string PicUrlTemplate = "http://rewalls.com/pic/{0}/{1}/reWalls.com-{2}.jpg";
        private const string Url = "http://rewalls.com/xml/tag.php?tag={0}";

        //all available sizes on rewalls.com
        private readonly List<Size> Sizes = new List<Size>(){new Size(2560,1600), new Size(2560,1440), new Size(1920,1200), new Size(1920,1080),
            new Size(1680,1050), new Size(1600,1200), new Size(1600,900), new Size(1440,900), new Size(1366,768), new Size(1280,1024),
            new Size(1280,960), new Size(1280,800), new Size(1280,768), new Size(1152,864), new Size(1024,768), new Size(1024, 600),
            new Size(800,600), new Size(640,960), new Size(320,480)};

        public void Initialize()
        {
            webClient = new WebClient();
            webClient.Headers["User-Agent"] =
                "Mozilla/4.0 (Compatible; Windows NT 5.1; MSIE 8.0) (compatible; MSIE 8.0; Windows NT 5.1;)";
        }

        public void Activate(object args) { }
        public void Deactivate(object args) { }


        public PictureList GetPictures(PictureSearch ps)
        {
            string search = ps.SearchString; bool skipLowRes = true; bool getMaxRes = true; List<string> filterKeywords = null;
            var result = new PictureList() { FetchDate = DateTime.Now };

            var query = HttpUtility.UrlEncode(search, Encoding.GetEncoding(1251));
            var url = string.Format(Url, query);
            var content = GeneralHelper.GetWebPageContent(url);
            if (string.IsNullOrEmpty(content))
                return result;
            //check for no results
            if (content.StartsWith("<br")) { return result; }

            var xml = XElement.Parse(content);
            foreach (var el in xml.Elements("Image"))
            {
                var tags = el.Element("tags").Value;

                if (filterKeywords != null && IsImageUnwanted(tags, filterKeywords))
                    continue;
                
                var id = el.Element("id").Value;
                var folder = el.Element("folder").Value;
                var maxResString = el.Element("res").Value;
                var maxRes = new Size(Convert.ToInt32(maxResString.Split('x')[0]), Convert.ToInt32(maxResString.Split('x')[1]));
                var curRes = new Size(Pulse.Base.PictureManager.PrimaryScreenResolution.First, Pulse.Base.PictureManager.PrimaryScreenResolution.Second);

                if (skipLowRes && (maxRes.Height < curRes.Height || maxRes.Width < curRes.Width))
                    continue;

                var res = FindNearestSize(curRes.Width, curRes.Height);
                
                var pic = new Picture();
                pic.Id = id;
                if (getMaxRes)
                    pic.Url = string.Format(PicUrlTemplate, folder, maxRes.Width + "x" + maxRes.Height, pic.Id);
                else 
                    pic.Url = string.Format(PicUrlTemplate, folder, res.Width + "x" + res.Height, pic.Id);
                result.Pictures.Add(pic);
            }

            return result;
        }

        private bool IsImageUnwanted(string tags, IEnumerable<string> filterKeywords)
        {
            return filterKeywords.Any(tags.Contains);
        }


        private static string GetFileName(string source)
        {
            var r = new Regex("/.*?\"");
            var result = r.Match(source).Value;
            result = result.Substring(result.LastIndexOf('/') + 1, result.Length - result.LastIndexOf('/') - 2);
            return result;
        }

        private static string GetMaxResolution(string source)
        {
            var r = new Regex("<div .*?>.*?</div>");
            var result = r.Match(source).Value;
            r = new Regex("<(.|\n)*?>");
            result = r.Replace(result, "");
            return result;
        }

        private static string GetDate(string source)
        {
            var r = new Regex("/.*?\"");
            var result = r.Match(source).Value;
            result = result.Replace("/large/", "");
            result = result.Remove(result.IndexOf('/'));
            return result;
        }

        private static int GetPagesCount(string source)
        {
            //var r = new Regex("<span class=\"nav_ext\">...</span>.*?<a href=\".*?\">.*?</a>");
            //var result = r.Match(source).Value;
            //r = new Regex("<(.|\n)*?>");
            //result = r.Replace(result, "");
            //result = result.Replace("...", "");
            //result = result.Replace(" ", "");
            var r = new Regex("<div align=\"left\"><h2>.*?</h2></div>");
            var result = r.Match(source).Value;
            r = new Regex("<(.|\n)*?>");
            result = r.Replace(result, "");
            r = new Regex("[^\\d]");
            result = r.Replace(result, "");
            if (!string.IsNullOrEmpty(result))
            {
                var num = Convert.ToInt32(result);
                return (int)(num / 16);
            }
            return 0;
        }

        private Size FindNearestSize(int width, int height)
        {
            var r = from x in Sizes where (x.Width <= width && x.Height <= height) select x;
            return r.First();
        }
    }
}
