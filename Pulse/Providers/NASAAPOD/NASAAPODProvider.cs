using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Text.RegularExpressions;
using Pulse.Base;
using Pulse.Base.Providers;

namespace NASAAPOD
{
    [System.ComponentModel.Description("NASA Astronomy Picture of the Day")]
    [ProviderIcon(typeof(Properties.Resources),"nasa")]
    public class NASAAPODProvider : Pulse.Base.IInputProvider
    {
        private static string _archiveUrl = "https://apod.nasa.gov/apod/archivepix.html";
        private static string _baseUrl = "https://apod.nasa.gov/apod/";
        private const string USER_AGENT = "Pulse/2.0 (NASA APOD; +https://github.com/patricker/Pulse)";

        // Attempt new NASA API first if available, fallback to scraping
        private const string NASA_API_URL = "https://api.nasa.gov/planetary/apod?api_key={0}&count={1}&thumbs=True";
        private const string DEMO_API_KEY = "DEMO_KEY";

        public PictureList GetPictures(PictureSearch ps)
        {
            EnsureTLS();

            var pl = new PictureList() { FetchDate = DateTime.Now };

            // Try new API approach (better)
            var apiResult = TryGetViaApi(ps);
            if (apiResult != null && apiResult.Pictures.Count > 0)
            {
                Log.Logger.Write($"NASA APOD: Got {apiResult.Pictures.Count} pictures via api.nasa.gov", Log.LoggerLevels.Info);
                return apiResult;
            }

            // Fallback to legacy scraping with fixes
            Log.Logger.Write("NASA APOD: API failed or empty, falling back to archive scraping", Log.LoggerLevels.Info);
            return GetViaScraping(ps);
        }

        private PictureList TryGetViaApi(PictureSearch ps)
        {
            try
            {
                int max = ps.MaxPictureCount > 0 ? ps.MaxPictureCount : 20;
                // NASA API count max 100
                max = Math.Min(max, 50);

                string apiKey = DEMO_API_KEY; // TODO: could load from settings if we add config
                string url = string.Format(NASA_API_URL, apiKey, max);

                string json;
                using (var wc = new WebClient())
                {
                    wc.Headers.Add(HttpRequestHeader.UserAgent, USER_AGENT);
                    wc.Headers.Add(HttpRequestHeader.Accept, "application/json");
                    json = wc.DownloadString(url);
                }

                if (string.IsNullOrEmpty(json))
                    return null;

                // Parse JSON - reuse similar approach as wallhaven
                var pics = ParseApiJson(json, ps);
                var list = new PictureList() { FetchDate = DateTime.Now };
                list.Pictures.AddRange(pics);
                return list;
            }
            catch (Exception ex)
            {
                Log.Logger.Write($"NASA APOD API error: {ex}", Log.LoggerLevels.Warnings);
                return null;
            }
        }

        private List<Picture> ParseApiJson(string json, PictureSearch ps)
        {
            var result = new List<Picture>();
            try
            {
                // Manual parsing - look for entries with media_type=image
                // Each entry has: "url": "https://apod.nasa.gov/...", "media_type": "image", "title": ...
                // Use regex to extract objects
                // Split by "media_type"

                // Find all image entries
                var pattern = "\"media_type\"\\s*:\\s*\"image\"";
                var matches = Regex.Matches(json, "\"url\"\\s*:\\s*\"(?<url>[^\"]+)\"[^}]*?\"media_type\"\\s*:\\s*\"image\"", RegexOptions.Singleline);
                // Alternative direction because order varies
                var matches2 = Regex.Matches(json, "\"media_type\"\\s*:\\s*\"image\"[^}]*?\"url\"\\s*:\\s*\"(?<url>[^\"]+)\"", RegexOptions.Singleline);

                var allUrls = new HashSet<string>();
                foreach (Match m in matches) allUrls.Add(m.Groups["url"].Value.Replace("\\/", "/"));
                foreach (Match m in matches2) allUrls.Add(m.Groups["url"].Value.Replace("\\/", "/"));

                // Also try to get hdurl if available (prefer hd)
                var hdPattern = "\"hdurl\"\\s*:\\s*\"(?<hd>[^\"]+)\"";
                var hdMatches = Regex.Matches(json, hdPattern);
                var hdDict = new Dictionary<string, string>();
                // This is simplistic - associating hdurl with url in same object is complex with regex
                // We'll just prefer hdurl when present by scanning objects

                // Better: parse array objects individually
                result = ParseObjectsManually(json, ps);
            }
            catch (Exception ex)
            {
                Log.Logger.Write($"NASA APOD API parse error: {ex}", Log.LoggerLevels.Errors);
            }
            var banned = ps.BannedURLs ?? new List<string>();
            return result.Where(p => !banned.Contains(p.Url)).ToList();
        }

        private List<Picture> ParseObjectsManually(string json, PictureSearch ps)
        {
            var list = new List<Picture>();
            // Very simple: split by "},"
            // Extract each {...} that contains media_type image
            // We look for {...} blocks, then check media_type
            var objectPattern = @"\{[^\{\}]*""media_type""[^\}]*\}";
            // But JSON may have nested, so we use a more robust split for this flat API (no deep nesting except)
            // The APOD API returns array of flat objects with strings, no nested objects except maybe
            try
            {
                // Take substring between [ and ]
                int arrStart = json.IndexOf('[');
                int arrEnd = json.LastIndexOf(']');
                if (arrStart < 0 || arrEnd < 0) return list;

                string arrayContent = json.Substring(arrStart + 1, arrEnd - arrStart - 1);

                // Split objects - accounting for }{ is boundary
                // Replace "},{" with "}##SPLIT##{"
                string temp = Regex.Replace(arrayContent, @"\}\s*,\s*\{", "}##SPLIT##{");
                var objects = temp.Split(new string[] { "##SPLIT##" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var obj in objects)
                {
                    string o = obj.Trim();
                    if (!o.StartsWith("{")) o = "{" + o;
                    if (!o.EndsWith("}")) o = o + "}";

                    if (!o.Contains("\"media_type\"")) continue;
                    if (o.Contains("\"media_type\"") && !o.Contains("\"image\"")) continue; // skip videos

                    // Prefer hdurl, then url, then thumbnail_url for videos we already filtered
                    var hdMatch = Regex.Match(o, "\"hdurl\"\\s*:\\s*\"(?<url>[^\"]+)\"");
                    var urlMatch = Regex.Match(o, "\"url\"\\s*:\\s*\"(?<url>[^\"]+)\"");
                    var titleMatch = Regex.Match(o, "\"title\"\\s*:\\s*\"(?<title>[^\"]+)\"");

                    string finalUrl = null;
                    if (hdMatch.Success) finalUrl = hdMatch.Groups["url"].Value.Replace("\\/", "/");
                    else if (urlMatch.Success) finalUrl = urlMatch.Groups["url"].Value.Replace("\\/", "/");

                    if (string.IsNullOrEmpty(finalUrl)) continue;

                    // APOD sometimes returns relative? No, always absolute http
                    // But ensure https
                    if (finalUrl.StartsWith("//")) finalUrl = "https:" + finalUrl;

                    string id = System.IO.Path.GetFileNameWithoutExtension(finalUrl);
                    if (string.IsNullOrEmpty(id)) id = Guid.NewGuid().ToString();

                    var p = new Picture() { Url = finalUrl, Id = id.Length > 50 ? id.Substring(0, 50) : id };
                    // Use url as thumb if needed? APOD thumb for image is same
                    p.Properties.Add(Picture.StandardProperties.Thumbnail, finalUrl);
                    p.Properties.Add(Picture.StandardProperties.Referrer, "https://apod.nasa.gov/apod/");
                    p.Properties.Add(Picture.StandardProperties.BanImageKey, finalUrl);
                    p.Properties.Add(Picture.StandardProperties.ProviderLabel, "NASA APOD");
                    if (titleMatch.Success)
                        p.Properties.Add("title", titleMatch.Groups["title"].Value);

                    var banned2 = ps.BannedURLs ?? new List<string>();
                    if (!banned2.Contains(finalUrl))
                        list.Add(p);
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Write($"NASA APOD manual object parse error: {ex}", Log.LoggerLevels.Warnings);
            }
            return list;
        }

        private PictureList GetViaScraping(PictureSearch ps)
        {
            var pl = new PictureList() { FetchDate = DateTime.Now };

            try
            {
                string archiveHtml;
                using (var wc = new HttpUtility.CookieAwareWebClient())
                {
                    wc.UserAgent = USER_AGENT;
                    archiveHtml = wc.DownloadString(_archiveUrl);
                }

                // Fixed regex - case insensitive, handles lower case now
                Regex reg = new Regex("<a href=\"(?<picPage>ap.*\\.html)\">", RegexOptions.IgnoreCase);
                Regex regPic = new Regex("<IMG\\s+SRC=\"(?<picURL>image/.*?|https?://.*?)\"|<img\\s+src=\"(?<picURL2>image/.*?|https?://.*?)\"", RegexOptions.IgnoreCase);
                Regex regHrefImage = new Regex("<a href=\"(?<img>image/.*?)\"", RegexOptions.IgnoreCase);

                var matches = reg.Matches(archiveHtml);

                // FIXED: Cap int.MaxValue, handle null banned list
                var bannedForCount = ps.BannedURLs ?? new List<string>();
                int maxPictureCount = ps.MaxPictureCount > 0
                    ? (ps.MaxPictureCount + bannedForCount.Count(u => u.StartsWith("https://apod.nasa.gov/apod/") || u.StartsWith("http://apod.nasa.gov/apod/")))
                    : 20; // cap when 0
                maxPictureCount = Math.Min(matches.Count, Math.Min(maxPictureCount, 100));

                var matchesToGet = (from Match c in matches select c)
                    .OrderBy(x => Guid.NewGuid())
                    .Take(maxPictureCount);

                foreach (Match m in matchesToGet)
                {
                    try
                    {
                        string pageUrl = _baseUrl + m.Groups["picPage"].Value;
                        string photoPage;
                        using (var wc = new HttpUtility.CookieAwareWebClient())
                        {
                            wc.UserAgent = USER_AGENT;
                            photoPage = wc.DownloadString(pageUrl);
                        }

                        // Try to find image - multiple patterns
                        string imgUrl = null;

                        // Pattern 1: IMG SRC
                        var imgMatch = regPic.Match(photoPage);
                        if (imgMatch.Success)
                        {
                            imgUrl = imgMatch.Groups["picURL"].Success ? imgMatch.Groups["picURL"].Value : imgMatch.Groups["picURL2"].Value;
                        }

                        // Pattern 2: HREF to image (some APOD pages link to image)
                        if (string.IsNullOrEmpty(imgUrl))
                        {
                            var hrefMatch = regHrefImage.Match(photoPage);
                            if (hrefMatch.Success)
                                imgUrl = hrefMatch.Groups["img"].Value;
                        }

                        if (string.IsNullOrEmpty(imgUrl))
                            continue;

                        // Resolve relative
                        if (imgUrl.StartsWith("image/"))
                            imgUrl = _baseUrl + imgUrl;
                        else if (!imgUrl.StartsWith("http"))
                            imgUrl = _baseUrl + imgUrl;

                        // Skip videos - if still not image extension, check if youtube etc
                        if (imgUrl.Contains("youtube") || imgUrl.Contains("youtu.be") || imgUrl.Contains("vimeo"))
                            continue;

                        var banned3 = ps.BannedURLs ?? new List<string>();
                        if (banned3.Contains(imgUrl))
                            continue;

                        var pic = new Picture()
                        {
                            Url = imgUrl,
                            Id = System.IO.Path.GetFileNameWithoutExtension(imgUrl)
                        };
                        if (string.IsNullOrEmpty(pic.Id)) continue;
                        if (pic.Id.Length > 50) pic.Id = pic.Id.Substring(0, 50);

                        pl.Pictures.Add(pic);
                    }
                    catch (Exception ex)
                    {
                        Log.Logger.Write($"NASA APOD scraping item error: {ex}", Log.LoggerLevels.Debug);
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Write($"NASA APOD scraping archive error: {ex}", Log.LoggerLevels.Errors);
            }

            return pl;
        }

        private void EnsureTLS()
        {
            try
            {
                ServicePointManager.SecurityProtocol |= (SecurityProtocolType)3072 | (SecurityProtocolType)768 | SecurityProtocolType.Tls;
                ServicePointManager.Expect100Continue = false;
            }
            catch { }
        }

        public void Activate(object args) { }
        public void Deactivate(object args) { }
        public void Initialize(object args) { EnsureTLS(); }
    }

    // Keep old typo class for backward compat - inherits fixed version
    public class NASAAPODProviderza : NASAAPODProvider { }
}
