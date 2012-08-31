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

namespace wallbase
{
    public class WallbaseImageSearchSettings : Pulse.Base.XmlSerializable<WallbaseImageSearchSettings>
    {
        private const string Url = "http://wallbase.cc/{0}/";

        //authentication info
        public string Username { get; set; }
        [XmlIgnore()]
        public string Password { get; set; }
        
        [XmlElement("Password")]
        public string xmlPassword {
            get { return Pulse.Base.GeneralHelper.Protect(Password); }
            set {
                //don't mess with empty passwords
                if (string.IsNullOrEmpty(value)) return;
                //if there is a password try to unprotect it
                Password = Pulse.Base.GeneralHelper.Unprotect(value); } 
        }

        //search location
        public string SA { get; set; }

        //categories
        public bool WG { get; set; }
        public bool W { get; set; }
        public bool HR { get; set; }

        //Purity
        public bool SFW { get; set; }
        public bool SKETCHY { get; set; }
        public bool NSFW { get; set; }

        //color, used for color searches (color searching is only available with "Search" type
        [XmlIgnore()]
        public System.Drawing.Color Color { get; set; }

        [XmlElement("Color")]
        public string ClrHtml
        {
            get { return ColorTranslator.ToHtml(Color); }
            set { Color = ColorTranslator.FromHtml(value); }
        }

        //collection ID for collection searches
        public string CollectionID { get; set; }

        //favorites ID for favorite showing
        public string FavoriteID { get; set; }

        //Image sizing information
        public string SO { get; set; }
        public int ImageWidth { get; set; }
        public int ImageHeight { get; set; }

        //order by
        public string OB { get; set; }
        public string OBD { get; set; }

        public WallbaseImageSearchSettings()
        {
            SA = "search";

            WG = true;
            W = true;

            SFW = true;

            SO = "gteq";
            ImageWidth = PictureManager.PrimaryScreenResolution.First;
            ImageHeight = PictureManager.PrimaryScreenResolution.Second;

            OB = "relevance";
            OBD = "desc";

            Color = System.Drawing.Color.Empty;
        }

        public string BuildPurityString()
        {
            return Convert.ToInt32(SFW).ToString() +
                Convert.ToInt32(SKETCHY).ToString() +
                Convert.ToInt32(NSFW).ToString();
        }

        public string BuildCategoryString()
        {
            return (WG ? "2" : "") +
                (W ? "1" : "") +
                (HR ? "3":"");
        }

        public string BuildResolutionString()
        {
            return ImageHeight > 0 && ImageWidth > 0 ? ImageWidth.ToString() + "x" + ImageHeight.ToString() : "0";
        }

        public string BuildURL()
        {
            string areaURL = string.Format(Url, SA);
            var resolutionString = BuildResolutionString();
            var pageSize = GetPageSize();

            //Search uses the post params, but random, top list, & collections do not
            // random includes the options in the URL string
            if (SA != "search")
            {
                //toplist put's it's page index before the categories
                if (SA == "toplist")
                {
                    //prepend placeholder for page number.  With toplist there is always a page numer (starting at 0, multiplied by item per page count)s
                    areaURL += "{0}/" + string.Format("{4}/{0}/{1}/0/{3}/{2}/3d", SO, resolutionString, pageSize.ToString(), BuildPurityString(), BuildCategoryString());
                }
                else if (SA == "random")
                {
                    //random does not need paging, we just reload the random page time and time again
                    areaURL += string.Format("{4}/{0}/{1}/0/{3}/{2}", SO, resolutionString, pageSize.ToString(), BuildPurityString(), BuildCategoryString());
                }
                else if (SA == "user/collection")
                {
                    areaURL += string.Format("{0}/{1}/0", CollectionID, Convert.ToInt32(NSFW).ToString());
                }
                else if (SA == "user/favorites")
                {
                    areaURL += string.Format("{0}/{1}/0/666", FavoriteID, "{0}");
                }
            }
            else
            {
                //if there is a color option and SA = search then add
                if (SA == "search" && Color != System.Drawing.Color.Empty)
                {
                    areaURL += string.Format("color/{0}/{1}/{2}/", Color.R.ToString(), Color.G.ToString(), Color.B.ToString());
                }

                //place holder for page number
                areaURL += "{0}";
            }

            return areaURL;
        }

        public int GetPageSize()
        {
            //override page size, since user collections dont' seem to be changeable from 32
            if (SA == "user/collection" || SA == "user/favorites") return 32;
            //if not user collection then max size
            return 60;
        }

        //public string GetPostParams(string search)
        //{
        //    string postParams = string.Empty;
            
        //    if(SA=="search") {
        //    postParams = string.Format("query={0}&board={7}&nsfw={6}&res_opt={2}&res={3}&aspect=0&orderby={4}&orderby_opt={5}&thpp={1}&section=wallpapers",
        //        search, GetPageSize().ToString(), SO, BuildResolutionString(), OB, OBD, BuildPurityString(), BuildCategoryString());
        //    }

        //    return postParams;
        //}

        public NameValueCollection GetPostParams(string search)
        {
            NameValueCollection postParams = new NameValueCollection();

            if (SA == "search")
            {
                postParams.Add("query",search);
                postParams.Add("board", BuildCategoryString());
                postParams.Add("nsfw",BuildPurityString());
                postParams.Add("res_opt",SO);
                postParams.Add("res",BuildResolutionString());
                postParams.Add("aspect","0.000");
                postParams.Add("orderby",OB);
                postParams.Add("orderby_opt",OBD);
                postParams.Add("thpp",GetPageSize().ToString());
                postParams.Add("section","wallpapers");
            }

            return postParams;
        }

        public class SearchArea
        {
            public string Name { get; private set; }
            public string Value { get; private set; }

            public static List<SearchArea> GetSearchAreas()
            {
                List<SearchArea> sa = new List<SearchArea>();

                sa.Add(new SearchArea() { Name = "Search", Value = "search" });
                sa.Add(new SearchArea() { Name = "Top List", Value = "toplist" });
                sa.Add(new SearchArea() { Name = "Random", Value = "random" });
                sa.Add(new SearchArea() { Name = "Collection", Value = "user/collection" });
                sa.Add(new SearchArea() { Name = "Favorite", Value = "user/favorites" });
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
                sa.Add(new OrderBy() { Name = "Date", Value = "date" });
                sa.Add(new OrderBy() { Name = "Views", Value = "views" });
                sa.Add(new OrderBy() { Name = "Favorites", Value = "favs" });
                sa.Add(new OrderBy() { Name = "Random", Value = "random" });

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

            public static List<SizingOption> GetDirectionList()
            {
                List<SizingOption> sa = new List<SizingOption>();

                sa.Add(new SizingOption() { Name = "Exactly", Value = "eqeq" });
                sa.Add(new SizingOption() { Name = "At Least", Value = "gteq" });

                return sa;
            }
        }
    }
}
