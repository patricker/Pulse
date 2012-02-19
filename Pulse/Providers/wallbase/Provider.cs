using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using Pulse.Base;
using System.Threading;

namespace wallbase
{
    [System.ComponentModel.Description("Wallbase")]
    public class Provider : IProvider
    {
        private int totalCount;
        private int resultCount;

        private const string Url = "http://wallbase.cc/search/";
        private Random rnd = new Random();

        public void Initialize()
        {
            System.Net.ServicePointManager.Expect100Continue = false;
        }

        public PictureList GetPictures(PictureSearch ps)
        {
            string search=ps.SearchString;
            bool skipLowRes=true;
                        
            //if max picture count is 0, then no maximum, else specified max
            var maxPictureCount = ps.MaxPictureCount > 0?ps.MaxPictureCount : int.MaxValue;
            int pageSize = 60;
            int pageIndex = 0;
            var imgFoundCount = 0;

            var result = new PictureList() { FetchDate = DateTime.Now };
            
            var wallResults = new List<WallPicture>();

            string postParams = "query={0}&board=all&thpp={1}&res_opt=eqeq&aspect=0&orderby=relevance&orderby_opt=desc";

            do
            {
                string strPageNum = pageIndex > 0 ? (pageIndex * pageSize).ToString() : "";

                string content = HttpPost(Url + strPageNum, string.Format(postParams, search, pageSize.ToString()));
                if (string.IsNullOrEmpty(content))
                    break;

                //parse html and get count
                var pics = ParsePictures(content, skipLowRes);
                imgFoundCount = pics.Count();

                wallResults.AddRange(pics);

                //increment page index so we can get the next set of images if they exist
                pageIndex++;

            } while (imgFoundCount > 0 && wallResults.Count < maxPictureCount);

            ManualResetEvent mreThread = new ManualResetEvent(false);

            ThreadStart threadStarter = () =>
            {
                //download in parallel
                var processCounter = 0;

                try
                {
                    while (processCounter < wallResults.Count)
                    {
                        var toProcess = wallResults.Skip(processCounter).Take(60).ToList();
                        processCounter += toProcess.Count;

                        ManualResetEvent[] manualEvents = new ManualResetEvent[toProcess.Count];

                        // Queue the work items that create and write to the files.
                        for (int i = 0; i < toProcess.Count; i++)
                        {
                            manualEvents[i] = new ManualResetEvent(false);

                            ThreadPool.QueueUserWorkItem(new WaitCallback(delegate(object state)
                            {
                                object[] states = (object[])state;

                                ManualResetEvent mre = (ManualResetEvent)states[0];
                                WallPicture wp = (WallPicture)states[1];

                                var p = new Picture();
                                p.Url = GetDirectPictureUrl(wp.PageUrl);
                                p.Id = GetPictureId(wp.PageUrl);
                                result.Pictures.Add(p);

                                mre.Set();
                            }), new object[] { manualEvents[i], toProcess[i] });
                        }

                        //wait for all items to finish
                        //one minute timeout
                        WaitHandle.WaitAll(manualEvents, 60 * 1000);
                    }
                }
                catch (Exception ex) { }
                finally
                {
                    mreThread.Set();
                }
            };

            var thread = new Thread(threadStarter);
            thread.SetApartmentState(ApartmentState.MTA);
            thread.Start();

            mreThread.WaitOne();

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
            var regex = new Regex(@"<img src="".*(wallpaper.*\.(jpg|png))""");
            var m = regex.Match(content);
            if (!string.IsNullOrEmpty(m.Value))
            {
                var url = m.Value;
                url = url.Substring(url.IndexOf('\"') + 1, url.LastIndexOf('\"') - url.IndexOf('\"') - 1);
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
