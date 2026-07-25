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
        // New Wallhaven API v1
        private const string ApiBase = "https://wallhaven.cc/api/v1";
        private const string SearchEndpoint = ApiBase + "/search";
        private const string CollectionEndpoint = ApiBase + "/collections/{0}/{1}";

        // Legacy URL kept for reference, no longer used
        private const string LegacyUrl = "http://alpha.wallhaven.cc/search?q={0}&categories={1}&purity={2}&resolutions={3}&sorting={4}&order={5}&page={6}";

        public string Query { get; set; }
        public string WallbaseSearchLabel { get; set; }

        // --- NEW: API key based auth (replaces username/password) ---
        [XmlIgnore()]
        public string ApiKey { get; set; }

        [XmlElement("ApiKey")]
        public string xmlApiKey
        {
            get { return Pulse.Base.GeneralHelper.Protect(ApiKey); }
            set
            {
                if (string.IsNullOrEmpty(value)) return;
                // Try to unprotect, if fails treat as plain (migration from old plain storage)
                try { ApiKey = Pulse.Base.GeneralHelper.Unprotect(value); }
                catch { ApiKey = value; }
                // If unprotect returned empty but value was plain, keep plain
                if (string.IsNullOrEmpty(ApiKey) && !string.IsNullOrEmpty(value) && value.Length < 100)
                {
                    // Possibly old plain storage, keep as is if unprotect failed
                    // Check if value looks like base64 (protected) - if not, treat as plain
                    try { Convert.FromBase64String(value); } catch { ApiKey = value; }
                }
            }
        }

        // Keep username for collections browsing (public collections)
        public string ApiUsername { get; set; }

        // Legacy auth (kept for backward compat, but not used)
        public string Username { get; set; }
        [XmlIgnore()]
        public string Password { get; set; }
        
        [XmlElement("Password")]
        public string xmlPassword {
            get { return Pulse.Base.GeneralHelper.Protect(Password); }
            set {
                if (string.IsNullOrEmpty(value)) return;
                Password = Pulse.Base.GeneralHelper.Unprotect(value); } 
        }

        // search location
        public string SA { get; set; }

        // categories
        public bool WG { get; set; }
        public bool W { get; set; }
        public bool HR { get; set; }

        // Purity
        public bool SFW { get; set; }
        public bool SKETCHY { get; set; }
        public bool NSFW { get; set; }

        // color
        [XmlIgnore()]
        public System.Drawing.Color Color { get; set; }

        [XmlElement("Color")]
        public string ClrHtml
        {
            get { return ColorTranslator.ToHtml(Color); }
            set { 
                try { Color = ColorTranslator.FromHtml(value); } 
                catch { Color = System.Drawing.Color.Empty; }
            }
        }

        // collection ID for collection searches
        public string CollectionID { get; set; }
        // favorites ID (legacy) - now treated as username for favorites browsing
        public string FavoriteID { get; set; }

        // Image sizing
        public string SO { get; set; } // gteq = atleast, eqeq = exact
        public int ImageWidth { get; set; }
        public int ImageHeight { get; set; }
        public string AR { get; set; } // aspect ratio legacy value like "1.77" or new like "16x9"

        // order by
        public string OB { get; set; }
        public string OBD { get; set; }

        // NEW: toplist range (1d,3d,1w,1M,3M,6M,1y)
        public string TopRange { get; set; }

        // NEW: seed for random pagination (API returns seed)
        public string LastSeed { get; set; }

        public WallbaseImageSearchSettings()
        {
            Query = "nature";

            SA = "search";

            WG = true;
            W = true;

            SFW = true;

            SO = "gteq";
            ImageWidth = 0;
            ImageHeight = 0;
            AR = "";

            OB = "relevance";
            OBD = "desc";
            TopRange = "1M";

            Color = System.Drawing.Color.Empty;
            ApiKey = "";
            ApiUsername = "";
        }

        public string BuildPurityString()
        {
            string s = Convert.ToInt32(SFW).ToString() +
                Convert.ToInt32(SKETCHY).ToString() +
                Convert.ToInt32(NSFW).ToString();
            // Validate at least one purity
            if (s == "000") return "100"; // default to SFW
            return s;
        }

        public string BuildCategoryString()
        {
            string c = (WG ? "1" : "0") +
                (W ? "1" : "0") +
                (HR ? "1":"0");
            // Validate at least one category
            if (c == "000") return "111";
            return c;
        }

        public string BuildResolutionString()
        {
            return ImageHeight > 0 && ImageWidth > 0 ? ImageWidth.ToString() + "x" + ImageHeight.ToString() : "";
        }

        public string GetColor()
        {
            return (Color == Color.Empty) ? "" : ClrHtml.Replace("#","");
        }

        // Legacy BuildURL - now redirects to API URL
        public string BuildURL()
        {
            return BuildAPIUrl(1);
        }

        public string BuildAPIUrl(int page)
        {
            if (page <= 0) page = 1;

            // Collections handling
            if (SA == "user/collection" && !string.IsNullOrEmpty(CollectionID))
            {
                // Need username - prefer ApiUsername, then FavoriteID as fallback for legacy, then Username
                string username = !string.IsNullOrEmpty(ApiUsername) ? ApiUsername : (!string.IsNullOrEmpty(FavoriteID) ? FavoriteID : Username);
                if (string.IsNullOrEmpty(username))
                    throw new InvalidOperationException("Username required for collection browsing. Set ApiUsername in Wallhaven settings.");

                string url = string.Format(CollectionEndpoint, Uri.EscapeDataString(username), Uri.EscapeDataString(CollectionID));
                var qs = new List<string>();
                qs.Add("page=" + page);
                if (!string.IsNullOrEmpty(ApiKey))
                    qs.Add("apikey=" + Uri.EscapeDataString(ApiKey));
                // purity filter works for collections
                string purity = BuildPurityString();
                if (!string.IsNullOrEmpty(purity) && purity != "100")
                    qs.Add("purity=" + Uri.EscapeDataString(purity));

                return url + "?" + string.Join("&", qs);
            }
            else if (SA == "user/favorites")
            {
                string fav = FavoriteID ?? "";
                fav = fav.Trim();
                if (!string.IsNullOrEmpty(fav))
                {
                    bool isNumeric = fav.All(char.IsDigit);
                    if (isNumeric)
                    {
                        string username = !string.IsNullOrEmpty(ApiUsername) ? ApiUsername : Username;
                        if (string.IsNullOrEmpty(username))
                            throw new InvalidOperationException("Username required for favorites collection. Set ApiUsername.");
                        string url = string.Format(CollectionEndpoint, Uri.EscapeDataString(username), Uri.EscapeDataString(fav));
                        var qs = new List<string>();
                        qs.Add("page=" + page);
                        if (!string.IsNullOrEmpty(ApiKey))
                            qs.Add("apikey=" + Uri.EscapeDataString(ApiKey));
                        return url + "?" + string.Join("&", qs);
                    }
                }
                // Fallback: search by user
                return BuildSearchUrl(page, !string.IsNullOrEmpty(fav) ? "@" + fav : Query);
            }
            else
            {
                // Normal search / toplist / random
                return BuildSearchUrl(page, Query);
            }
        }

        private string BuildSearchUrl(int page, string query)
        {
            var sb = new StringBuilder();
            sb.Append(SearchEndpoint);
            sb.Append("?");

            var parms = new List<string>();

            // q
            if (!string.IsNullOrEmpty(query))
                parms.Add("q=" + Uri.EscapeDataString(query));
            else if (!string.IsNullOrEmpty(Query))
                parms.Add("q=" + Uri.EscapeDataString(Query));

            // categories - escaped
            string cat = BuildCategoryString();
            if (!string.IsNullOrEmpty(cat) && cat != "111")
                parms.Add("categories=" + Uri.EscapeDataString(cat));

            // purity - escaped
            string purity = BuildPurityString();
            if (!string.IsNullOrEmpty(purity) && purity != "100")
                parms.Add("purity=" + Uri.EscapeDataString(purity));

            // sorting - validate against allow list and escape
            string sorting = OB ?? "relevance";
            if (sorting == "date") sorting = "date_added";
            if (sorting == "favs") sorting = "favorites";
            var allowedSorting = new HashSet<string> { "relevance", "date_added", "views", "favorites", "toplist", "random" };
            if (!allowedSorting.Contains(sorting)) sorting = "relevance";
            parms.Add("sorting=" + Uri.EscapeDataString(sorting));

            // order - validate and escape
            string order = OBD ?? "desc";
            if (order != "asc" && order != "desc") order = "desc";
            parms.Add("order=" + Uri.EscapeDataString(order));

            // toplist range - escaped and validated
            if (sorting == "toplist")
            {
                string range = string.IsNullOrEmpty(TopRange) ? "1M" : TopRange;
                var allowedRanges = new HashSet<string> { "1d", "3d", "1w", "1M", "3M", "6M", "1y" };
                if (!allowedRanges.Contains(range)) range = "1M";
                parms.Add("topRange=" + Uri.EscapeDataString(range));
            }

            // resolutions / atleast - escaped
            string res = BuildResolutionString();
            if (!string.IsNullOrEmpty(res))
            {
                // Validate format like 1920x1080
                var resRegex = new System.Text.RegularExpressions.Regex(@"^\d+x\d+$");
                if (resRegex.IsMatch(res))
                {
                    if (SO == "eqeq")
                        parms.Add("resolutions=" + Uri.EscapeDataString(res));
                    else
                        parms.Add("atleast=" + Uri.EscapeDataString(res));
                }
            }

            // ratios - map and escape
            string ratio = MapAspectRatio(AR);
            if (!string.IsNullOrEmpty(ratio))
            {
                // Allow comma-separated ratios? Validate each is like 16x9
                parms.Add("ratios=" + Uri.EscapeDataString(ratio));
            }

            // colors - validate hex and escape
            string color = GetColor();
            if (!string.IsNullOrEmpty(color))
            {
                var colorRegex = new System.Text.RegularExpressions.Regex(@"^[0-9a-fA-F]{6}$");
                if (colorRegex.IsMatch(color))
                    parms.Add("colors=" + Uri.EscapeDataString(color));
            }

            // page
            parms.Add("page=" + page);

            // seed for random - escaped already
            if (sorting == "random" && !string.IsNullOrEmpty(LastSeed))
                parms.Add("seed=" + Uri.EscapeDataString(LastSeed));

            // apikey - escaped
            if (!string.IsNullOrEmpty(ApiKey))
                parms.Add("apikey=" + Uri.EscapeDataString(ApiKey));

            sb.Append(string.Join("&", parms));
            return sb.ToString();
        }

        private string MapAspectRatio(string ar)
        {
            if (string.IsNullOrEmpty(ar)) return "";

            // If already in new format like "16x9", return as is
            if (ar.Contains("x")) return ar;

            // Map legacy float strings to new ratios
            // Legacy: 1.33=4:3, 1.25=5:4, 1.77=16:9, 1.60=16:10, 1.70=Netbook, 2.50=Dual, 3.20=Dual wide, 1.01=Widescreen, 0.99=Portrait, ""=All
            switch (ar)
            {
                case "1.33": return "4x3";
                case "1.25": return "5x4";
                case "1.77": return "16x9";
                case "1.60": return "16x10";
                case "1.70": return "16x9"; // netbook approx
                case "2.50": return "32x9"; // dual → super ultrawide
                case "3.20": return "48x9"; // dual wide → triple
                case "1.01": return ""; // widescreen = any wide
                case "0.99": return "9x16"; // portrait approx
                default: return ar; // return as is, maybe already custom
            }
        }

        public int GetPageSize()
        {
            return 24; // Wallhaven API v1 always 24 per page
        }

        public int GetMaxImageCount(int userMax)
        {
            // SECURITY FIX: Cap int.MaxValue which caused DoS fetching all pages
            // Old returned int.MaxValue for collections or 0, causing infinite loop fetching 1000s pages
            const int MAX_CAP = 500;
            const int DEFAULT_COLLECTION_CAP = 200;

            if (SA == "user/collection" || SA == "user/favorites")
            {
                if (userMax == 0) return DEFAULT_COLLECTION_CAP;
                return Math.Min(userMax, MAX_CAP);
            }

            if (userMax == 0) return DEFAULT_COLLECTION_CAP;
            return Math.Min(userMax, MAX_CAP);
        }

        // --- Helper lists for UI ---

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
                sa.Add(new SearchArea() { Name = "Favorite User", Value = "user/favorites" });
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
                sa.Add(new OrderBy() { Name = "Toplist", Value = "toplist" });
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
                tts.Add(new AspectRatio() { Name = "16x9", Value = "16x9" });
                tts.Add(new AspectRatio() { Name = "16x10", Value = "16x10" });
                tts.Add(new AspectRatio() { Name = "21x9", Value = "21x9" });
                tts.Add(new AspectRatio() { Name = "32x9 (dual 16:9 / super ultrawide)", Value = "32x9" });
                tts.Add(new AspectRatio() { Name = "48x9 (triple 16:9)", Value = "48x9" });
                tts.Add(new AspectRatio() { Name = "32x10", Value = "32x10" });
                tts.Add(new AspectRatio() { Name = "48x10", Value = "48x10" });
                tts.Add(new AspectRatio() { Name = "4x3", Value = "4x3" });
                tts.Add(new AspectRatio() { Name = "5x4", Value = "5x4" });
                tts.Add(new AspectRatio() { Name = "9x16", Value = "9x16" });
                tts.Add(new AspectRatio() { Name = "1x1", Value = "1x1" });
                // Keep legacy for compat
                tts.Add(new AspectRatio() { Name = "Portrait (legacy)", Value = "0.99" });
                tts.Add(new AspectRatio() { Name = "Widescreen (legacy)", Value = "1.01" });
                return tts;
            }
        }
    }
}
