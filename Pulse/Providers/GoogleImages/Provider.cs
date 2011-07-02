using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pulse.Base;
using System.Text.RegularExpressions;
using System.Windows;

namespace GoogleImages
{
    public class Provider : IProvider
    {
        private int _resultCount = 0;

        public void Initialize()
        {
            //nothing to do here
        }

        public List<Picture> GetPictures(string search, bool skipLowRes, bool getMaxRes, List<string> filterKeywords)
        {
            var result = new List<Picture>();

            var height = (int)SystemParameters.PrimaryScreenHeight;
            var width = (int)SystemParameters.PrimaryScreenWidth;
            var baseURL = "http://images.google.com/search?tbm=isch&hl=en&source=hp&biw=&bih=&gbv=1&q={0}&tbs=isz:ex,iszw:{1},iszh:{2}&start={3}";

            System.Net.WebClient client = new System.Net.WebClient();
            Regex imagesRegex2 = new Regex(@"(imgurl=)(?<imgurl>http[^&>]*)([>&]{1})");

            var pageIndex = 0;
            var imgFoundCount =0;

            do
            {
                //build URL from query, dimensions and page index
                var url = string.Format(baseURL, search, width.ToString(), height.ToString(), (pageIndex * 20).ToString());

                var response = client.DownloadString(url);

                var images = imagesRegex2.Matches(response);

                //track number of images found for paging purposes
                imgFoundCount = images.Count;

                //convert images found into picture entries
                foreach (Match item in images)
                {
                    var purl = item.Groups[3].Value;
                    //id must be a value that can be used as a file name, but also should be unique.
                    // so we'll use a guid
                    //NOTE id should not be unique to make caching possible
                    var id = Guid.NewGuid().ToString();
                    result.Add(new Picture() { Url = purl, Id = id });
                }

                //increment page index so we can get the next 20 images if they exist
                pageIndex++;
            } while (imgFoundCount > 0);

            //set result count
            _resultCount = result.Count;

            return result;
        }

        public int GetResultsCount()
        {
            return _resultCount;
        }
    }
}
