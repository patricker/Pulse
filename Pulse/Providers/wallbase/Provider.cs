using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using Pulse.Base;
using Pulse.Base.Providers;
using System.Threading;
using System.IO;
using System.Collections.Specialized;
using wallhaven;
using Newtonsoft.Json;

namespace wallhaven
{
    [System.ComponentModel.Description("Wallhaven")]
    [ProviderConfigurationUserControl(typeof(WallbaseProviderPrefs))]
    [ProviderConfigurationClass(typeof(WallhavenImageSearchSettings))]
    [ProviderIcon(typeof(wallhaven.Properties.Resources), "wallhaven")]
    public class Provider : IInputProvider
    {
        private int resultCount;
        private string _rootURL = "https://wallhaven.cc/api/v1/";

        public void Initialize(object args)
        {

        }

        public void Activate(object args) { }
        public void Deactivate(object args) { }

        public PictureList GetPictures(PictureSearch ps)
        {
            WallhavenImageSearchSettings wiss = string.IsNullOrEmpty(ps.SearchProvider.ProviderConfig) ? new WallhavenImageSearchSettings() : WallhavenImageSearchSettings.LoadFromXML(ps.SearchProvider.ProviderConfig);
                                    
            //if max picture count is 0, then no maximum, else specified max
            var maxPictureCount = wiss.GetMaxImageCount(ps.MaxPictureCount);
            int pageIndex = (ps.PageToRetrieve<=0?1:ps.PageToRetrieve); //set page to retreive if one is specified

            var imgFoundCount = 0;
            
            var wallResults = new List<Picture>();

            string areaURL = _rootURL + wiss.BuildURL();

            do
            {
                //get page index.
                string strPageNum = pageIndex.ToString();

                string pageURL = areaURL.Contains("{0}") ? string.Format(areaURL, strPageNum) : areaURL;
                string content = string.Empty;

                using (HttpUtility.CookieAwareWebClient _client = new HttpUtility.CookieAwareWebClient())
                {
                    try
                    {
                        Log.Logger.Write(String.Format("Downloading rest results from {0}", pageURL), Log.LoggerLevels.Verbose);
                        content = _client.DownloadString(pageURL);
                    }
                    catch (Exception ex)
                    {
                        Log.Logger.Write(string.Format("Failed to download search results from wallbase.cc, error: {0}", ex.ToString()), Log.LoggerLevels.Warnings);
                    }
                }

                if (string.IsNullOrEmpty(content))
                    break;

                //deserialize JSON and get count
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
            } while (imgFoundCount > 0 && wallResults.Count < maxPictureCount && ps.PageToRetrieve == 0);

            PictureList result = FetchPictures(wallResults, ps.PreviewOnly);
            result.Pictures = result.Pictures.Take(maxPictureCount).ToList();

            return result;
        }

        private PictureList FetchPictures(List<Picture> wallResults, bool previewOnly) 
        {
            var result = new PictureList() { FetchDate = DateTime.Now };

            try
            {
                System.Threading.Tasks.TaskFactory tf = new System.Threading.Tasks.TaskFactory();
                
                wallResults
                    .AsParallel()
                    .WithDegreeOfParallelism(10)
                    .ForAll(delegate(Picture p){
                        try
                        {
                            //save original URL as referrer
                            p.Properties.Add(Picture.StandardProperties.Referrer, p.Url);
                            p.Properties.Add(Picture.StandardProperties.BanImageKey, Path.GetFileName(p.Url));
                            p.Properties.Add(Picture.StandardProperties.ProviderLabel, "Wallhaven");
                            p.Id = System.IO.Path.GetFileNameWithoutExtension(p.Url);

                            if (!string.IsNullOrEmpty(p.Url) && !string.IsNullOrEmpty(p.Id))
                            {
                                result.Pictures.Add(p);
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Logger.Write(string.Format("Error downloading picture object from '{0}'. Exception details: {0}", ex.ToString()), Log.LoggerLevels.Errors);
                        }
                        finally
                        {
                        }
                    });
            }
            catch(Exception ex) {
                Log.Logger.Write(string.Format("Error during multi-threaded wallbase.cc image get.  Exception details: {0}", ex.ToString()), Log.LoggerLevels.Errors);
            }
            finally
            {
            }

            return result;
        }

        //find links to pages with wallpaper only with matching resolution
        private List<Picture> ParsePictures(string content)
        {
            WallhavenResponse response = JsonConvert.DeserializeObject<WallhavenResponse>(content);
            var picList = new List<Picture>();

            foreach (var whp in response.data)
            {
                var p = new Picture()
                {
                    Url = whp.path
                };

                p.Properties.Add(Picture.StandardProperties.Thumbnail, whp.thumbs.small);

                picList.Add(p);
            }

            return picList;
        }
    }
}
