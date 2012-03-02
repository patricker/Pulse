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
    [ProviderConfigurationUserControl(typeof(WallbaseProviderPreferences))]
    public class Provider : IInputProvider
    {
        private int totalCount;
        private int resultCount;
        private CookieContainer _cookies = new CookieContainer();

        private Random rnd = new Random();

        public void Initialize()
        {
            System.Net.ServicePointManager.Expect100Continue = false;
        }

        public void Activate(object args) { }
        public void Deactivate(object args) { }

        public PictureList GetPictures(PictureSearch ps)
        {
            string search=ps.SearchString;

            WallbaseImageSearchSettings wiss = string.IsNullOrEmpty(ps.ProviderSearchSettings) ? new WallbaseImageSearchSettings() : WallbaseImageSearchSettings.LoadFromXML(ps.ProviderSearchSettings);
            
            //if we have a username/password and we aren't already authenticated then authenticate
            if (_cookies.Count == 0 && !string.IsNullOrEmpty(wiss.Username) && !string.IsNullOrEmpty(wiss.Password))
            {
                var test = HttpGet("http://wallbase.cc/home");
                //usrname=$USER&pass=$PASS&nopass_email=Type+in+your+e-mail+and+press+enter&nopass=0&1=1
                var hit1 = HttpPost("http://wallbase.cc/user/login", string.Format("usrname={0}&pass={1}&nopass_email=Type+in+your+e-mail+and+press+enter&nopass=0&1=1", wiss.Username, wiss.Password));
                var hitConfirm = HttpPost("http://wallbase.cc/user/adult_confirm/1", "");
            }
                                    
            //if max picture count is 0, then no maximum, else specified max
            var maxPictureCount = ps.MaxPictureCount > 0?ps.MaxPictureCount : int.MaxValue;
            int pageSize = wiss.GetPageSize();
            int pageIndex = 0;
            var imgFoundCount = 0;

            
            var wallResults = new List<Picture>();

            string areaURL = wiss.BuildURL();
            string postParams = wiss.GetPostParams(search);

            do
            {
                //calculate page index.  Random does not use pages, so for random just refresh with same url
                string strPageNum = (pageIndex > 0 && wiss.SA != "random") || (wiss.SA == "toplist" || wiss.SA=="user/collection") ? (pageIndex * pageSize).ToString() : "";

                string pageURL = areaURL.Contains("{0}") ? string.Format(areaURL, strPageNum) : areaURL;
                string content = HttpPost(pageURL, postParams);
                if (string.IsNullOrEmpty(content))
                    break;

                //parse html and get count
                var pics = ParsePictures(content);
                imgFoundCount = pics.Count();

                //if we have an image ban list check for them
                // doing this in the provider instead of picture manager
                // ensures that our count does not go down if we have a max
                if (ps.BannedURLs != null && ps.BannedURLs.Count > 0)
                {
                    pics = (from c in pics where !(ps.BannedURLs.Contains(c.Url)) select c).ToList();
                }

                wallResults.AddRange(pics);

                //increment page index so we can get the next set of images if they exist
                pageIndex++;
            } while (imgFoundCount > 0 && wallResults.Count < maxPictureCount);

            PictureList result = FetchPictures(wallResults);
            

            return result;
        }

        private PictureList FetchPictures(List<Picture> wallResults) 
        {
            var result = new PictureList() { FetchDate = DateTime.Now };

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
                                Picture p = (Picture)states[1];

                                p.Url = GetDirectPictureUrl(p.Url);
                                p.Id = System.IO.Path.GetFileNameWithoutExtension(p.Url);

                                if (!string.IsNullOrEmpty(p.Url) && !string.IsNullOrEmpty(p.Id))
                                {
                                    result.Pictures.Add(p);
                                }

                                mre.Set();
                            }), new object[] { manualEvents[i], toProcess[i] });
                        }

                        //wait for all items to finish
                        //one minute timeout
                        WaitHandle.WaitAll(manualEvents, 60 * 1000);
                    }
                }
                catch { }
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
        private List<Picture> ParsePictures(string content)
        {
            var picsRegex = new Regex("<a href=\"(?<link>http://wallbase.cc/wallpaper/.*)\" id=\".*\" .*>.*<img.*src=\"(?<img>.*)\".*style=\".*</a>");
            //var resRegex = new Regex("<span class=\"res\">.*</span>");
            var picsMatches = picsRegex.Matches(content);
            //var resMatches = resRegex.Matches(content);
            var result = new List<Picture>();
            for (var i = 0; i < picsMatches.Count; i++)
            {
                var pic = new Picture();

                pic.Url = picsMatches[i].Groups["link"].Value;
                pic.Properties["thumb"] = picsMatches[i].Groups["img"].Value;

                result.Add(pic);
            }
            resultCount = result.Count;
            return result;
        }

        private string GetDirectPictureUrl(string pageUrl)
        {            
            var content = HttpPost(pageUrl,"");
            if (string.IsNullOrEmpty(content)) return string.Empty;

            var regex = new Regex(@"<img src=""(?<img>.*(wallpaper.*\.(jpg|png)))""");
            var m = regex.Match(content);
            if (m.Groups["img"].Success && !string.IsNullOrEmpty(m.Groups["img"].Value))
            {
                return m.Groups["img"].Value;
            }

            return string.Empty;
        }

        private string HttpGet(string url)
        {
            System.Net.WebRequest req = System.Net.WebRequest.Create(url);
            ((HttpWebRequest)req).CookieContainer = _cookies;

            var resp = req.GetResponse();

            if (resp == null) return null;
            System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream());
            return sr.ReadToEnd().Trim();

        }

        private string HttpPost(string url, string parameters)
        {
            try
            {
                System.Net.WebRequest req = System.Net.WebRequest.Create(url);
                //Add these, as we're doing a POST
                req.ContentType = "application/x-www-form-urlencoded";
                req.Method = "POST";

                ((HttpWebRequest)req).Referer = "http://wallbase.cc/home/";
                //((HttpWebRequest)req).UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/535.11 (KHTML, like Gecko) Chrome/17.0.963.56 Safari/535.11";                
                //add cookies if there are any
                ((HttpWebRequest)req).CookieContainer = _cookies;

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
