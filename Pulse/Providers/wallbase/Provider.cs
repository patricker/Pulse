using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using Pulse.Base;

namespace wallbase
{
    public class Provider : IProvider
    {
        private int totalCount;
        private int resultCount;

        private const string Url = "http://wallbase.cc/search/";
        private Random rnd = new Random(Environment.TickCount);

        public void Initialize()
        {
            System.Net.ServicePointManager.Expect100Continue = false;
        }

        public List<Picture> GetPictures(string search, bool skipLowRes, bool getMaxRes, List<string> filterKeywords)
        {
            string content;
            if (totalCount == 0)
            {
                content = HttpPost(Url, string.Format("query={0}&board=all&thpp=32&res_opt=eqeq&aspect=0&orderby=relevance&orderby_opt=desc", search));
                if (string.IsNullOrEmpty(content))
                    return null;
                GetPagesCount(content);
            }
            else
            {
                content = HttpPost(Url + rnd.Next(totalCount / 32), string.Format("query={0}&board=all&thpp=32&res_opt=eqeq&aspect=0&orderby=relevance&orderby_opt=desc", search));
                if (string.IsNullOrEmpty(content))
                    return null;
            }
            var pics = ParsePictures(content, skipLowRes);
            var result = new List<Picture>();
            foreach (var wallPicture in pics)
            {
                var p = new Picture();
                p.Url = GetDirectPictureUrl(wallPicture.PageUrl);
                p.Id = GetPictureId(wallPicture.PageUrl);
                result.Add(p);
            }

            return result;
        }

        //find links to pages with wallpaper only with matching resolution
        private IEnumerable<WallPicture> ParsePictures(string content, bool skipLowRes)
        {
            var picsRegex = new Regex("<a href=\"http://.*\" id=\".*\" .*>");
            var resRegex = new Regex("<span class=\"res\">.*</span>");
            var picsMatches = picsRegex.Matches(content);
            var resMatches = resRegex.Matches(content);
            var result = new List<WallPicture>();
            for (var i = 0; i < picsMatches.Count; i++)
            {
                var resString = StripTags(resMatches[i].Value);
                var res = new Size(Convert.ToInt32(resString.Split('x')[0]), Convert.ToInt32(resString.Split('x')[1]));
                var curRes = new Size((int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight);

                if (skipLowRes && (res.Height < curRes.Height || res.Width < curRes.Width))
                    continue;
                var pic = new WallPicture();
                var url = picsMatches[i].Value;
                url = url.Substring(url.IndexOf('"') + 1, url.IndexOf(' ', 5) - url.IndexOf('"') - 2);
                pic.PageUrl = url;
                pic.Width = res.Width;
                pic.Height = res.Height;

                result.Add(pic);
            }
            resultCount = result.Count;
            return result;
        }

        private string GetDirectPictureUrl(string pageUrl)
        {
            var webClient = new WebClient();
            var content = webClient.DownloadString(pageUrl);
            var regex = new Regex(@"<img src='.*(wallpaper.*\.(jpg|png))'");
            var m = regex.Match(content);
            if (!string.IsNullOrEmpty(m.Value))
            {
                var url = m.Value;
                url = url.Substring(url.IndexOf('\'') + 1, url.LastIndexOf('\'') - url.IndexOf('\'') - 1);
                return url;
            }

            return null;
        }

        private string GetPictureId(string pageUrl)
        {
            return pageUrl.Substring(pageUrl.LastIndexOf('/') + 1);
        }

        private void GetPagesCount(string content)
        {
            var regex = new Regex("<span>.*</span>");
            var count = StripTags(regex.Match(content).Value);
            if (count.Contains(","))
                count = count.Remove(count.IndexOf(','), 1);
            totalCount = int.Parse(count);
        }

        public int GetResultsCount()
        {
            return resultCount;
        }

        private static string HttpPost(string url, string parameters)
        {
            try
            {


                System.Net.WebRequest req = System.Net.WebRequest.Create(url);
                //Add these, as we're doing a POST
                req.ContentType = "application/x-www-form-urlencoded";
                req.Method = "POST";
                //We need to count how many bytes we're sending. Post'ed Faked Forms should be name=value&
                byte[] bytes = System.Text.Encoding.ASCII.GetBytes(parameters);
                req.ContentLength = bytes.Length;
                System.IO.Stream os = req.GetRequestStream();
                os.Write(bytes, 0, bytes.Length); //Push it out there
                os.Close();
                System.Net.WebResponse resp = req.GetResponse();

                if (resp == null) return null;
                System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream());
                return sr.ReadToEnd().Trim();
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static string StripTags(string source)
        {
            return Regex.Replace(source, "<.*?>", string.Empty);
        }
    }
}
