using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Xml.Linq;
using Pulse.Base;

namespace Rewalls
{
    public class Provider : IProvider
    {
        private WebClient webClient;
        private StreamReader reader;
        private int resultsCount;

        private const string PicUrlTemplate = "http://rewalls.com/pic/{0}/{1}/reWalls.com-{2}.jpg";
        private const string Url = "http://rewalls.com/xml/tag.php?tag={0}";

        //all available sizes on rewall.com
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

        public List<Picture> GetPictures(string search, bool skipLowRes, bool getMaxRes, List<string> filterKeywords = null)
        {
            var query = HttpUtility.UrlEncode(search, Encoding.GetEncoding(1251));
            var url = string.Format(Url, query);
            var content = GeneralHelper.GetWebPageContent(url);
            if (string.IsNullOrEmpty(content))
                return null;
            var xml = XElement.Parse(content);
            var result = new List<Picture>();
            foreach (var el in xml.Elements("Image"))
            {
                var tags = el.Element("tags").Value;

                if (filterKeywords != null && IsImageUnwanted(tags, filterKeywords))
                    continue;
                
                var id = el.Element("id").Value;
                var folder = el.Element("folder").Value;
                var maxResString = el.Element("res").Value;
                var maxRes = new Size(Convert.ToInt32(maxResString.Split('x')[0]), Convert.ToInt32(maxResString.Split('x')[1]));
                var curRes = new Size((int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight);

                if (skipLowRes && (maxRes.Height < curRes.Height || maxRes.Width < curRes.Width))
                    continue;

                var res = FindNearestSize(curRes.Width, curRes.Height);
                
                var pic = new Picture();
                pic.Id = id;
                if (getMaxRes)
                    pic.Url = string.Format(PicUrlTemplate, folder, maxRes.Width + "x" + maxRes.Height, pic.Id);
                else 
                    pic.Url = string.Format(PicUrlTemplate, folder, res.Width + "x" + res.Height, pic.Id);
                result.Add(pic);
            }

            resultsCount = result.Count;

            return result;
        }

        private bool IsImageUnwanted(string tags, IEnumerable<string> filterKeywords)
        {
            return filterKeywords.Any(tags.Contains);
        }

        /*public List<Picture> GetPictures(string search, bool skipLowRes, bool getMaxRes)
        {
            var rnd = new Random(Environment.TickCount);
            var query = HttpUtility.UrlEncode(search, Encoding.GetEncoding(1251));
            var url = "http://rewalls.com/tags/" + query;
            try
            {
                reader = new StreamReader(webClient.OpenRead(url));
            }
            catch (Exception)
            {
                return null;
            }
            var content = reader.ReadToEnd();
            var pagesCount = GetPagesCount(content);
            if (pagesCount > 0)
            {
                reader.Close();
                try
                {
                    reader = new StreamReader(webClient.OpenRead(url + "/page/" + rnd.Next(pagesCount)));
                }
                catch (Exception)
                {
                    return null;
                }
                content = reader.ReadToEnd();
            }
            resultsCount = pagesCount * 16;
            var result = new List<Picture>();
            //parse wallpapers on the first page
            var r = new Regex("<a href=\".*\">.*</a>.*<div class=\"razmer\">.*</div>");
            var c = 0;
            foreach (Match m in r.Matches(content))
            {
                var v = m.Value;
                var pic = new Picture();
                var fileName = GetFileName(v);
                var maxRes = GetMaxResolution(v);
                var date = GetDate(v);

                var res = maxRes.Split(new[] { 'x' });
                var maxWidth = Convert.ToInt32(res[0]);
                var maxHeight = Convert.ToInt32(res[1]);
                if (skipLowRes && (maxHeight < SystemParameters.PrimaryScreenHeight || maxWidth < SystemParameters.PrimaryScreenWidth))
                    continue;

                pic.Id = fileName;
                if (!getMaxRes)
                    pic.Url = string.Format(PicUrlTemplate, date, SystemParameters.PrimaryScreenWidth + "x" + SystemParameters.PrimaryScreenHeight, fileName);
                else
                {
                    //2560x1600
                    if (maxWidth > 2560 && maxHeight > 1600)
                    {
                        maxWidth = 2560;
                        maxHeight = 1600;
                    }
                    var size = FindNearestSize(maxWidth, maxHeight);
                    pic.Url = string.Format(PicUrlTemplate, date, size.Width + "x" + size.Height, fileName);
                }
                result.Add(pic);
                c++;
            }
            if (resultsCount == 0)
                resultsCount = c;
            reader.Close();
            return result;
        }*/

        public int GetResultsCount()
        {
            return resultsCount;
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
