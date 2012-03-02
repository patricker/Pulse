using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pulse.Base;
using System.Text.RegularExpressions;
using System.Windows;
using System.ComponentModel;

namespace GoogleImages
{
    [ProviderConfigurationUserControl(typeof(ProviderPreferences))]
    [System.ComponentModel.Description("Google Images")]
    public class Provider : IInputProvider
    {
        private Regex imagesRegex2 = new Regex(@"(imgurl=)(?<imgurl>http[^&>]*)([>&]{1})");
        private string baseURL = "http://images.google.com/search?tbm=isch&hl=en&source=hp&biw=&bih=&gbv=1&q={0}{1}&start={2}";
        
        public Provider()
        {
        }

        public void Initialize()
        {
            //nothing to do here
        }

        public void Activate(object args) { }
        public void Deactivate(object args) { }


        public PictureList GetPictures(PictureSearch ps)
        {
            var result = new PictureList() { FetchDate = DateTime.Now };

            //load provider search settings
            GoogleImageSearchSettings giss = GoogleImageSearchSettings.LoadFromXML(ps.ProviderSearchSettings);

            //to help not have so many null checks, if no search settings provided then use default ones
            if (giss == null) giss=new GoogleImageSearchSettings();

            //if search is empty, return now since we can't search without it
            if (string.IsNullOrEmpty(ps.SearchString)) return result;

            System.Net.WebClient client = new System.Net.WebClient();                        

            var pageIndex = 0;
            var imgFoundCount =0;
            
            //if max picture count is 0, then no maximum, else specified max
            var maxPictureCount = ps.MaxPictureCount > 0?ps.MaxPictureCount : int.MaxValue;

            //build tbs strring
            var tbs = "";//isz:ex,iszw:{1},iszh:{2}

            //handle sizeing
            if (giss.ImageHeight > 0 && giss.ImageWidth > 0)
            {
                tbs += string.Format("isz:ex,iszw:{0},iszh:{1},", giss.ImageWidth.ToString(), giss.ImageHeight.ToString());
            }

            //handle colors
            if (!string.IsNullOrEmpty(giss.Color))
            {
                tbs += GoogleImageSearchSettings.GoogleImageColors.GetColorSearchString((from c in GoogleImageSearchSettings.GoogleImageColors.GetColors() where c.Value == giss.Color select c).Single()) + ",";
            }

            //if we have a filter string then add it and trim off trailing commas
            if (!string.IsNullOrEmpty(tbs)) tbs = ("&tbs=" + tbs).Trim(new char[]{','});

            do
            {
                //build URL from query, dimensions and page index
                var url = string.Format(baseURL, ps.SearchString, tbs, (pageIndex * 20).ToString());

                var response = client.DownloadString(url);

                var images = imagesRegex2.Matches(response);

                //track number of images found for paging purposes
                imgFoundCount = images.Count;

                //convert images found into picture entries
                foreach (Match item in images)
                {
                    var purl = item.Groups[3].Value;
                    //get id and trim if necessary (ran into a few cases of rediculously long filenames)
                    var id = System.IO.Path.GetFileNameWithoutExtension(purl);
                    if (id.Length > 50) id = id.Substring(0, 50);

                    result.Pictures.Add(new Picture() { Url = purl, Id = id });
                }

                //if we have an image ban list check for them
                // doing this in the provider instead of picture manager
                // ensures that our count does not go down if we have a max
                if (ps.BannedURLs != null && ps.BannedURLs.Count > 0)
                {
                    result.Pictures = (from c in result.Pictures where !(ps.BannedURLs.Contains(c.Url)) select c).ToList();
                }

                //increment page index so we can get the next 20 images if they exist
                pageIndex++;
                // Max Picture count is defined in search settings passed in, check for it here too
            } while (imgFoundCount > 0 && result.Pictures.Count < maxPictureCount);

            return result;
        }
    }
}
