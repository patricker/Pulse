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
using System.IO;

namespace wallbase
{
    [System.ComponentModel.Description("Wallbase")]
    [ProviderConfigurationUserControl(typeof(WallbaseProviderPreferences))]
    public class Provider : IInputProvider
    {
        private int totalCount;
        private int resultCount;
        private CookieContainer _cookies = new CookieContainer();
        //private string _cookies = string.Empty;

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
            if (!string.IsNullOrEmpty(wiss.Username) && !string.IsNullOrEmpty(wiss.Password))
            {
                //var test = HttpGet("http://wallbase.cc/home");
                //usrname=$USER&pass=$PASS&nopass_email=Type+in+your+e-mail+and+press+enter&nopass=0&1=1
                var hit1 = HttpPost("http://wallbase.cc/user/login", string.Format("usrname={0}&pass={1}&nopass_email=Type+in+your+e-mail+and+press+enter&nopass=0", wiss.Username, Pulse.Base.HttpUtility.UrlEncode(wiss.Password)));
                var hitConfirm = HttpPost("http://wallbase.cc/user/adult_confirm/1", "");
                var test2 = HttpPost("http://wallbase.cc/home", "");
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

                                try
                                {
                                    p.Url = GetDirectPictureUrl(p.Url);
                                    p.Id = System.IO.Path.GetFileNameWithoutExtension(p.Url);

                                    if (!string.IsNullOrEmpty(p.Url) && !string.IsNullOrEmpty(p.Id))
                                    {
                                        Log.Logger.Write(string.Format("Skipping banned image in wallbase.cc provider, '{0}'.", p.Url), Log.LoggerLevels.Verbose);

                                        result.Pictures.Add(p);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Log.Logger.Write(string.Format("Error downloading picture object from '{0}'. Exception details: {0}", ex.ToString()), Log.LoggerLevels.Errors);
                                }
                                finally
                                {
                                    mre.Set();
                                }

                            }), new object[] { manualEvents[i], toProcess[i] });
                        }

                        //wait for all items to finish
                        //one minute timeout
                        WaitHandle.WaitAll(manualEvents, 60 * 1000);
                    }
                }
                catch(Exception ex) {
                    Log.Logger.Write(string.Format("Error during multi-threaded wallbase.cc image get.  Exception details: {0}", ex.ToString()), Log.LoggerLevels.Errors);

                }
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

        private string HttpPost(string url, string parameters)
        {
            try
            {
                HttpWebRequest req = (HttpWebRequest)System.Net.WebRequest.Create(url);
                //as we're doing a POST
                req.Method = "POST";
                req.CookieContainer = _cookies;
                req.ContentType = "application/x-www-form-urlencoded";

                req.Headers[HttpRequestHeader.CacheControl] = "max-age=0";
                req.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/535.11 (KHTML, like Gecko) Chrome/17.0.963.78 Safari/535.11";
                req.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
                //req.Headers[HttpRequestHeader.AcceptEncoding] = "gzip,deflate,sdch";
                req.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                req.Headers[HttpRequestHeader.AcceptLanguage] = "en-US,en;q=0.8";
                req.Headers[HttpRequestHeader.AcceptCharset] = "ISO-8859-1,utf-8;q=0.7,*;q=0.3";

                //if (!string.IsNullOrEmpty(_cookies))
                //{
                //    req.Headers.Add("Cookie", _cookies);
                //}
                
                req.Referer = "http://wallbase.cc/home";
                //req.AllowAutoRedirect = false;

                //We need to count how many bytes we're sending. Post'ed Faked Forms should be name=value&
                byte[] bytes = System.Text.Encoding.ASCII.GetBytes(parameters);
                req.ContentLength = bytes.Length;
                System.IO.Stream os = req.GetRequestStream();
                os.Write(bytes, 0, bytes.Length); //Push it out there
                os.Close();

                //get response
                using (System.Net.HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                {
                    //if (!string.IsNullOrEmpty(resp.GetResponseHeader("Set-Cookie")))
                    //{
                    //    _cookies = resp.GetResponseHeader("Set-Cookie");
                    //}

                    //clear cookies from container and use response ones
                    //_cookies = new CookieContainer();

                    ////parse code copied and modified from: http://stackoverflow.com/a/3349654/328968 (credit where it's due)
                    
                    ////parse cookies
                    //var setCookie = resp.GetResponseHeader("Set-Cookie");

                    ////remove all expiration sections as they just muck up our logic
                    //Regex regExpires = new Regex("(?<name>.*?)=(?<value>.*?); expires=(?<expires>.*?); path=(?<path>.*?); domain=(?<domain>.*?)(,|\\Z)");

                    ////setCookie = regExpires.Replace(setCookie, "");
                    //var matches = regExpires.Matches(setCookie);

                    //foreach (Match m in matches)
                    //{

                    //    var c = new Cookie(m.Groups["name"].Value, m.Groups["value"].Value, m.Groups["path"].Value, m.Groups["domain"].Value);
                    //    c.Expires = DateTime.Parse(m.Groups["expires"].Value);

                    //    _cookies.Add(c);
                    //}


                    //check response
                    if (resp == null) return null;
                    using (Stream st = resp.GetResponseStream())
                    {
                        System.IO.StreamReader sr = new System.IO.StreamReader(st);
                        return sr.ReadToEnd().Trim();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Write(string.Format("Error in HttpPost request. Url: '{0}', Post Parameteres: '{1}'. Exception details: {2}", url, parameters, ex.ToString()), Log.LoggerLevels.Errors);
                return null;
            }
        }

        private static string StripTags(string source)
        {
            return Regex.Replace(source, "<.*?>", string.Empty);
        }
    }
}
