using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Windows;
using System.Drawing;
using System.Security.Cryptography;
using System.Net;
using Pulse.Base;
using System.Collections.Specialized;

namespace wallhaven
{
    public class WallhavenImageSearchSettings : XmlSerializable<WallhavenImageSearchSettings>
    {                       
        public string Query { get; set; }
        public string WallbaseSearchLabel { get; set; }

        //authentication info
        public string APIKey { get; set; }

        //search location
        public string SearchType { get; set; }

        public string TopRange { get; set; }

        //categories
        /// <summary>General</summary>
        public bool General { get; set; }
        /// <summary>Anime</summary>
        public bool Anime { get; set; }
        /// <summary>People</summary>
        public bool People { get; set; }

        //Purity
        public bool SFW { get; set; }
        public bool SKETCHY { get; set; }
        public bool NSFW { get; set; }

        //color, used for color searches (color searching is only available with "Search" type
        public string Color { get; set; }

        //collection ID for collection searches
        public string CollectionID { get; set; }

        //favorites ID for favorite showing
        public string FavoriteID { get; set; }

        //Image sizing information
        public string SO { get; set; }
        public int ImageWidth { get; set; }
        public int ImageHeight { get; set; }
        public string AR { get; set; }

        /// <summary>
        /// Order By
        /// </summary>
        public string OB { get; set; }
        /// <summary>
        ///  Order By Direction
        /// </summary>
        public string OBD { get; set; }
        
        public WallhavenImageSearchSettings()
        {
            Query = "nature";

            SearchType = "search";

            General = true;
            Anime = true;

            SFW = true;

            SO = "gteq";
            ImageWidth = PictureManager.PrimaryScreenResolution.First;
            ImageHeight = PictureManager.PrimaryScreenResolution.Second;
            AR = "";

            OB = "relevance";
            OBD = "desc";

            Color = "";

            TopRange = "1M";
        }

        public string BuildPurityString()
        {
            return Convert.ToInt32(SFW).ToString() +
                Convert.ToInt32(SKETCHY).ToString() +
                Convert.ToInt32(NSFW).ToString();
        }

        public string BuildCategoryString()
        {
            return (General ? "1" : "0") +
                (Anime ? "1" : "0") +
                (People ? "1":"0");
        }

        public string BuildResolutionString()
        {
            return ImageHeight > 0 && ImageWidth > 0 ? ImageWidth.ToString() + "x" + ImageHeight.ToString() : "";
        }

        public Dictionary<string, string> BuildURLDictionary()
        {
            var parms = new Dictionary<string, string>();

            var resolutionString = BuildResolutionString();

            if (SearchType == "search" || SearchType == "random" || SearchType == "latest" || SearchType == "toplist")
            {
                if (!string.IsNullOrEmpty(Query)) parms.Add("q", Query);

                parms.Add("categories", BuildCategoryString());
                parms.Add("purity", BuildPurityString());

                if (!string.IsNullOrEmpty(resolutionString)) parms.Add("resolutions", resolutionString);

                // Don't sort TopList
                if (SearchType == "toplist")
                {
                    parms.Add("sorting", "toplist");
                    if (!string.IsNullOrEmpty(TopRange)) parms.Add("topRange", TopRange);
                    if (!string.IsNullOrEmpty(OBD)) parms.Add("order", OBD);
                }
                else if (SearchType == "random") parms.Add("sorting", "random");
                else if (SearchType == "latest") parms.Add("sorting", "latest");
                else
                {
                    if (!string.IsNullOrEmpty(OB)) parms.Add("sorting", OB);
                    if (!string.IsNullOrEmpty(OBD)) parms.Add("order", OBD);
                }

                if (!string.IsNullOrEmpty(Color)) parms.Add("colors", Color);

                // If user is logged in, add user API Key
                if (!string.IsNullOrEmpty(APIKey)) parms.Add("apikey", APIKey);

                // Include a placeholder for page#
                parms.Add("page","{0}");
            }

            return parms;
        }

        public string BuildURL()
        {
            string areaURL = "search?";

            var paramDict = BuildURLDictionary();

            //new URL includes {0} placeholder for page number
            areaURL += String.Join("&", paramDict.Select(x => x.Key + "=" + x.Value));

            return areaURL;
        }

        public int GetMaxImageCount(int userMax)
        {
            if (userMax == 0) return int.MaxValue;
            else return userMax;
        }

        public class ColorList
        {
            public static List<string> GetColors()
            {
                string colors = "NONE 660000 990000 cc0000 cc3333 ea4c88 " +
                    "993399 663399 333399 0066cc 0099cc " +
                    "66cccc 77cc33 669900 336600 666600 " +
                    "999900 cccc33 ffff00 ffcc33 ff9900 " +
                    "ff6600 cc6633 996633 663300 000000 " +
                    "999999 cccccc ffffff 424153";
                List<string> colorList = colors.Split(new char[] { ' ' }).ToList();
                
                return colorList;
            }
        }

        public class SearchArea
        {
            public string Name { get; private set; }
            public string Value { get; private set; }

            public static List<SearchArea> GetSearchAreas()
            {
                List<SearchArea> sa = new List<SearchArea>();

                sa.Add(new SearchArea() { Name = "Search", Value = "search" });
                sa.Add(new SearchArea() { Name = "Latest", Value = "latest" });
                sa.Add(new SearchArea() { Name = "Random", Value = "random" });
                sa.Add(new SearchArea() { Name = "Toplist", Value = "toplist" });
                //sa.Add(new SearchArea() { Name = "Collection", Value = "collection" });
                //sa.Add(new SearchArea() { Name = "Favorite", Value = "favorites" });
                return sa;
            }
        }

        public class OrderBy
        {
            public string Name { get; private set; }
            public string Value { get; private set; }

            public static List<OrderBy> GetOrderByList()
            {
                List<OrderBy> sa = new List<OrderBy>();

                sa.Add(new OrderBy() { Name = "Relevancy", Value = "relevance" });
                sa.Add(new OrderBy() { Name = "Date", Value = "date_added" });
                sa.Add(new OrderBy() { Name = "Views", Value = "views" });
                sa.Add(new OrderBy() { Name = "Favorites", Value = "favorites" });

                return sa;
            }
        }

        public class OrderByDirection
        {
            public string Name { get; private set; }
            public string Value { get; private set; }

            public static List<OrderByDirection> GetDirectionList()
            {
                List<OrderByDirection> sa = new List<OrderByDirection>();

                sa.Add(new OrderByDirection() { Name = "Descending", Value = "desc" });
                sa.Add(new OrderByDirection() { Name = "Ascending", Value = "asc" });

                return sa;
            }
        }

        public class SizingOption
        {
            public string Name { get; private set; }
            public string Value { get; private set; }

            public static List<SizingOption> GetSizingList()
            {
                List<SizingOption> sa = new List<SizingOption>();

                sa.Add(new SizingOption() { Name = "Exactly", Value = "resolutions" });
                sa.Add(new SizingOption() { Name = "At Least", Value = "atleast" });

                return sa;
            }
        }

        public class TopTimeSpan
        {
            public string Name { get; private set; }
            public string Value { get; private set; }

            public static List<TopTimeSpan> GetTimespanList()
            {
                List<TopTimeSpan> tts = new List<TopTimeSpan>();

                tts.Add(new TopTimeSpan() { Name = "1 day (24h)", Value = "1d" });
                tts.Add(new TopTimeSpan() { Name = "3 days", Value = "3d" });
                tts.Add(new TopTimeSpan() { Name = "1 week", Value = "1w" });
                tts.Add(new TopTimeSpan() { Name = "1 month", Value = "1M" });
                tts.Add(new TopTimeSpan() { Name = "3 months", Value = "3M" });
                tts.Add(new TopTimeSpan() { Name = "6 months", Value = "6M" });
                tts.Add(new TopTimeSpan() { Name = "1 year", Value = "1y" });

                return tts;
            }
        }

        public class AspectRatio
        {
            public string Name { get; private set; }
            public string Value { get; private set; }

            public static List<AspectRatio> GetAspectRatioList()
            {
                List<AspectRatio> tts = new List<AspectRatio>();

                tts.Add(new AspectRatio() { Name = "All", Value = "" });
                tts.Add(new AspectRatio() { Name = "16:9", Value = "16:9" });
                tts.Add(new AspectRatio() { Name = "16:10", Value = "16:10" });

                tts.Add(new AspectRatio() { Name = "21:9", Value = "21:9" });
                tts.Add(new AspectRatio() { Name = "32:9", Value = "32:9" });
                tts.Add(new AspectRatio() { Name = "48:9", Value = "48:9" });

                tts.Add(new AspectRatio() { Name = "9:16", Value = "9:16" });
                tts.Add(new AspectRatio() { Name = "10:16", Value = "10:16" });
                tts.Add(new AspectRatio() { Name = "9:18", Value = "9:18" });

                tts.Add(new AspectRatio() { Name = "1:1", Value = "1:1" });
                tts.Add(new AspectRatio() { Name = "4:3", Value = "4:3" });
                tts.Add(new AspectRatio() { Name = "5:4", Value = "5:4" });

                return tts;
            }
        }
    }
}
