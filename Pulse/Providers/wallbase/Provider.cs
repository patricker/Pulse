using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.IO;
using Pulse.Base;
using Pulse.Base.Providers;
using System.Threading;

namespace wallbase
{
    [System.ComponentModel.Description("Wallhaven")]
    [ProviderConfigurationUserControl(typeof(WallbaseProviderPrefs))]
    [ProviderConfigurationClass(typeof(WallbaseImageSearchSettings))]
    [ProviderIcon(typeof(wallhaven.Properties.Resources), "wallhaven")]
    public class Provider : IInputProvider
    {
        private const string USER_AGENT = "Pulse/2.0 (Wallhaven API v1; +https://github.com/patricker/Pulse)";

        public void Initialize(object args)
        {
            // SECURITY FIX: Only TLS 1.2, no SSL3/TLS1.0 (POODLE)
            try
            {
                ServicePointManager.Expect100Continue = false;
                var tls12 = (SecurityProtocolType)3072;
                ServicePointManager.SecurityProtocol |= tls12;
            }
            catch { }
        }

        public void Activate(object args) { }
        public void Deactivate(object args) { }

        public PictureList GetPictures(PictureSearch ps)
        {
            Initialize(null);

            WallbaseImageSearchSettings wiss = string.IsNullOrEmpty(ps.SearchProvider.ProviderConfig)
                ? new WallbaseImageSearchSettings()
                : WallbaseImageSearchSettings.LoadFromXML(ps.SearchProvider.ProviderConfig);

            if (wiss == null)
                wiss = new WallbaseImageSearchSettings();

            int rawMax = wiss.GetMaxImageCount(ps.MaxPictureCount);
            // SECURITY/CORRECTNESS: Cap int.MaxValue (collections or userMax=0) to avoid DoS fetching all pages
            int maxPictureCount = rawMax == int.MaxValue ? 200 : Math.Min(rawMax, 500);
            int pageSize = wiss.GetPageSize(); // 24
            int pageIndex = (ps.PageToRetrieve <= 0 ? 1 : ps.PageToRetrieve);

            var wallResults = new List<Picture>();
            string seed = wiss.LastSeed;
            int lastPage = int.MaxValue;
            int retriesCurrentPage = 0;
            bool localNSFW = wiss.NSFW; // Don't mutate wiss.NSFW (BUG-04)

            // SECURITY: Sanitize query for log (remove newlines)
            string safeQuery = (wiss.Query ?? "").Replace("\r", "").Replace("\n", "").Replace("\0", "");
            Log.Logger.Write($"Wallhaven API: Starting search query='{safeQuery}' categories={wiss.BuildCategoryString()} purity={wiss.BuildPurityString()} sorting={wiss.OB} order={wiss.OBD} max={maxPictureCount} pageStart={pageIndex}", Log.LoggerLevels.Info);

            do
            {
                string apiUrl;
                try
                {
                    apiUrl = wiss.BuildAPIUrl(pageIndex);
                }
                catch (InvalidOperationException ex)
                {
                    Log.Logger.Write($"Wallhaven API: Configuration error - {ex.Message}. Returning empty. Please set username for collections.", Log.LoggerLevels.Warnings);
                    break;
                }
                // FIX seed handling: strip any existing seed from BuildAPIUrl and inject current seed
                if (wiss.OB == "random")
                {
                    apiUrl = System.Text.RegularExpressions.Regex.Replace(apiUrl, @"[&?]seed=[^&]*", "");
                    if (!string.IsNullOrEmpty(seed))
                    {
                        apiUrl += (apiUrl.Contains("?") ? "&" : "?") + "seed=" + Uri.EscapeDataString(seed);
                    }
                }

                // SECURITY: Redact apikey in logs
                string logUrl = System.Text.RegularExpressions.Regex.Replace(apiUrl, @"apikey=[^&]+", "apikey=REDACTED");
                Log.Logger.Write($"Wallhaven API: Fetching {logUrl}", Log.LoggerLevels.Debug);

                string json = "";
                try
                {
                    json = DownloadString(apiUrl, wiss.ApiKey);
                    retriesCurrentPage = 0; // reset on success
                }
                catch (WebException webEx)
                {
                    var resp = webEx.Response as HttpWebResponse;
                    if (resp != null)
                    {
                        Log.Logger.Write($"Wallhaven API: HTTP {(int)resp.StatusCode} {resp.StatusCode} for {logUrl}", Log.LoggerLevels.Warnings);
                        if ((int)resp.StatusCode == 429)
                        {
                            Log.Logger.Write("Wallhaven API: Rate limited (429), sleeping 5s", Log.LoggerLevels.Warnings);
                            Thread.Sleep(5000);
                            retriesCurrentPage++;
                            if (retriesCurrentPage < 3) continue;
                        }
                        else if ((int)resp.StatusCode == 401)
                        {
                            Log.Logger.Write("Wallhaven API: Unauthorized (401) - check API key for NSFW content", Log.LoggerLevels.Warnings);
                            if (localNSFW && string.IsNullOrEmpty(wiss.ApiKey))
                            {
                                Log.Logger.Write("Wallhaven API: Retrying without NSFW due to missing API key", Log.LoggerLevels.Warnings);
                                localNSFW = false;
                                wiss.NSFW = false; // fallback to SFW for retry (mutate is okay here as we're downgrading)
                                continue;
                            }
                        }
                    }
                    Log.Logger.Write($"Wallhaven API: Failed to download {logUrl}, error: {webEx.Message}", Log.LoggerLevels.Warnings);
                    break;
                }
                catch (Exception ex)
                {
                    Log.Logger.Write($"Wallhaven API: Failed to download {logUrl}, error: {ex.Message}", Log.LoggerLevels.Warnings);
                    break;
                }

                if (string.IsNullOrEmpty(json))
                {
                    Log.Logger.Write("Wallhaven API: Empty response, breaking", Log.LoggerLevels.Warnings);
                    break;
                }

                List<Picture> pics = new List<Picture>();
                WallhavenMeta meta = null;

                try
                {
                    var parsed = ParseApiResponse(json);
                    pics = parsed.Item1;
                    meta = parsed.Item2;
                }
                catch (Exception ex)
                {
                    Log.Logger.Write($"Wallhaven API: Failed to parse JSON: {ex}\nJSON snippet: {json.Substring(0, Math.Min(500, json.Length))}", Log.LoggerLevels.Errors);
                    break;
                }

                if (pics == null || pics.Count == 0)
                {
                    Log.Logger.Write("Wallhaven API: No pictures in response, breaking", Log.LoggerLevels.Info);
                    break;
                }

                // Update seed and paging info
                if (meta != null)
                {
                    lastPage = meta.last_page;
                    if (!string.IsNullOrEmpty(meta.seed))
                        seed = meta.seed;
                    Log.Logger.Write($"Wallhaven API: Page {meta.current_page}/{meta.last_page}, total {meta.total}, seed={meta.seed}", Log.LoggerLevels.Debug);
                }

                // Banned filter
                if (ps.BannedURLs != null && ps.BannedURLs.Count > 0)
                {
                    pics = pics.Where(c => !ps.BannedURLs.Contains(c.Url)).ToList();
                }

                wallResults.AddRange(pics);

                pageIndex++;

                // Respect obvious rate limiting courtesy
                if (wallResults.Count < maxPictureCount && pageIndex <= lastPage)
                    Thread.Sleep(250);

            } while (wallResults.Count < maxPictureCount && pageIndex <= lastPage && ps.PageToRetrieve == 0);

            // Trim to max
            var resultPics = wallResults.Take(maxPictureCount).ToList();

            var result = new PictureList() { FetchDate = DateTime.Now };
            result.Pictures.AddRange(resultPics);

            Log.Logger.Write($"Wallhaven API: Returning {result.Pictures.Count} pictures (requested {maxPictureCount})", Log.LoggerLevels.Info);

            return result;
        }

        private string DownloadString(string url, string apiKey)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
                throw new ArgumentException("Invalid URL scheme, only http/https allowed");

            // SECURITY FIX: Manual redirect handling with private IP validation
            string currentUrl = url;
            int redirectCount = 0;
            const int maxRedirects = 3;

            while (redirectCount <= maxRedirects)
            {
                if (!Uri.TryCreate(currentUrl, UriKind.Absolute, out var curUri) ||
                    (curUri.Scheme != Uri.UriSchemeHttp && curUri.Scheme != Uri.UriSchemeHttps))
                    throw new ArgumentException("Invalid redirect URL scheme");

                // Block private IPs
                string host = curUri.Host.ToLowerInvariant();
                if (host == "localhost" || host == "127.0.0.1" || host == "::1" || host == "0.0.0.0" ||
                    host.StartsWith("10.") || host.StartsWith("192.168.") || host.StartsWith("169.254.") ||
                    host.StartsWith("172.16.") || host.StartsWith("172.17.") || host.StartsWith("172.18.") ||
                    host.StartsWith("172.19.") || host.StartsWith("172.20.") || host.StartsWith("172.21.") ||
                    host.StartsWith("172.22.") || host.StartsWith("172.23.") || host.StartsWith("172.24.") ||
                    host.StartsWith("172.25.") || host.StartsWith("172.26.") || host.StartsWith("172.27.") ||
                    host.StartsWith("172.28.") || host.StartsWith("172.29.") || host.StartsWith("172.30.") ||
                    host.StartsWith("172.31.") || host.StartsWith("fc00:") || host.StartsWith("fe80:"))
                    throw new InvalidOperationException($"Blocked private IP redirect: {host}");

                try
                {
                    var request = (HttpWebRequest)WebRequest.Create(currentUrl);
                    request.Method = "GET";
                    request.UserAgent = USER_AGENT;
                    request.Accept = "application/json";
                    request.Timeout = 15000;
                    request.AllowAutoRedirect = false;
                    request.CookieContainer = new CookieContainer();
                    if (!string.IsNullOrEmpty(apiKey))
                        request.Headers.Add("X-API-Key", apiKey);

                    try { ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072; } catch { }

                    using (var response = (HttpWebResponse)request.GetResponse())
                    {
                        if ((int)response.StatusCode >= 300 && (int)response.StatusCode < 400)
                        {
                            string? location = response.Headers["Location"];
                            if (string.IsNullOrEmpty(location))
                                throw new WebException("Redirect with no Location header");

                            // Resolve relative redirect
                            if (!Uri.TryCreate(location, UriKind.Absolute, out var locUri))
                            {
                                locUri = new Uri(curUri, location);
                            }
                            currentUrl = locUri.ToString();
                            redirectCount++;
                            continue;
                        }

                        using (var stream = response.GetResponseStream())
                        using (var reader = new StreamReader(stream, Encoding.UTF8))
                        {
                            return reader.ReadToEnd();
                        }
                    }
                }
                catch (WebException wex) when (wex.Response is HttpWebResponse resp && (int)resp.StatusCode >= 300 && (int)resp.StatusCode < 400)
                {
                    // Handle redirect via exception path (some WebRequest versions throw on 302 when AllowAutoRedirect=false)
                    string? location = resp.Headers["Location"];
                    if (string.IsNullOrEmpty(location)) throw;
                    if (!Uri.TryCreate(location, UriKind.Absolute, out var locUri))
                        locUri = new Uri(curUri, location);
                    currentUrl = locUri.ToString();
                    redirectCount++;
                    continue;
                }
            }

            throw new WebException($"Too many redirects for {url}");
        }

        #region JSON parsing

        // Returns Tuple<List<Picture>, Meta>
        private Tuple<List<Picture>, WallhavenMeta> ParseApiResponse(string json)
        {
            // Try JavaScriptSerializer first (built-in .NET 4.0 if System.Web.Extensions referenced)
            try
            {
                // Use reflection to avoid compile-time dependency if assembly missing
                var dict = TryDeserializeWithJavaScriptSerializer(json);
                if (dict != null)
                    return ParseFromDictionary(dict);
            }
            catch (Exception ex)
            {
                Log.Logger.Write($"Wallhaven API: JavaScriptSerializer failed, falling back to manual parse: {ex.Message}", Log.LoggerLevels.Debug);
            }

            // Fallback: manual regex-based parsing for minimal fields
            return ParseManually(json);
        }

        private Dictionary<string, object> TryDeserializeWithJavaScriptSerializer(string json)
        {
            // Dynamically load System.Web.Extensions
            // In .NET 4.0 this is available, but not referenced by default in this project.
            // We'll try to use it via reflection, or if direct type exists, use it.
            try
            {
                var serializerType = Type.GetType("System.Web.Script.Serialization.JavaScriptSerializer, System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
                if (serializerType == null)
                {
                    // Try other version
                    serializerType = Type.GetType("System.Web.Script.Serialization.JavaScriptSerializer, System.Web.Extensions");
                }
                if (serializerType == null)
                    return null;

                dynamic serializer = Activator.CreateInstance(serializerType);
                // Max length big
                try { serializer.MaxJsonLength = int.MaxValue; } catch { }

                var result = serializer.DeserializeObject(json) as Dictionary<string, object>;
                return result;
            }
            catch
            {
                // Try direct reference if assembly was added
                try
                {
#pragma warning disable 0219
                    // This will compile only if reference exists; we guard with try
                    var direct = new System.Web.Script.Serialization.JavaScriptSerializer();
                    direct.MaxJsonLength = int.MaxValue;
                    return direct.DeserializeObject(json) as Dictionary<string, object>;
#pragma warning restore 0219
                }
                catch { return null; }
            }
        }

        private Tuple<List<Picture>, WallhavenMeta> ParseFromDictionary(Dictionary<string, object> root)
        {
            var pics = new List<Picture>();
            WallhavenMeta meta = new WallhavenMeta();

            if (root == null) return Tuple.Create(pics, meta);

            // data
            if (root.TryGetValue("data", out object dataObj) && dataObj is System.Collections.ArrayList dataList)
            {
                foreach (var item in dataList)
                {
                    if (item is Dictionary<string, object> wpDict)
                    {
                        var pic = ParseWallpaperDict(wpDict);
                        if (pic != null) pics.Add(pic);
                    }
                }
            }
            else if (root.TryGetValue("data", out object dataObj2) && dataObj2 is object[] dataArr)
            {
                foreach (var item in dataArr)
                {
                    if (item is Dictionary<string, object> wpDict)
                    {
                        var pic = ParseWallpaperDict(wpDict);
                        if (pic != null) pics.Add(pic);
                    }
                }
            }

            // meta
            if (root.TryGetValue("meta", out object metaObj) && metaObj is Dictionary<string, object> metaDict)
            {
                meta = ParseMetaDict(metaDict);
            }

            return Tuple.Create(pics, meta);
        }

        private Picture ParseWallpaperDict(Dictionary<string, object> d)
        {
            try
            {
                string id = d.ContainsKey("id") ? d["id"]?.ToString() : "";
                string path = d.ContainsKey("path") ? d["path"]?.ToString() : "";
                string url = d.ContainsKey("url") ? d["url"]?.ToString() : $"https://wallhaven.cc/w/{id}";
                string purity = d.ContainsKey("purity") ? d["purity"]?.ToString() : "";
                string category = d.ContainsKey("category") ? d["category"]?.ToString() : "";

                if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(path))
                    return null;

                string thumb = "";
                if (d.TryGetValue("thumbs", out object thumbsObj) && thumbsObj is Dictionary<string, object> thumbsDict)
                {
                    // prefer large, then original, then small
                    if (thumbsDict.TryGetValue("large", out object large)) thumb = large?.ToString();
                    else if (thumbsDict.TryGetValue("original", out object orig)) thumb = orig?.ToString();
                    else if (thumbsDict.TryGetValue("small", out object small)) thumb = small?.ToString();
                }

                var p = new Picture()
                {
                    Id = id,
                    Url = path // direct full image
                };
                // If thumbnail missing, leave empty, Picture class will fallback
                if (!string.IsNullOrEmpty(thumb))
                    p.Properties.Add(Picture.StandardProperties.Thumbnail, thumb);
                p.Properties.Add(Picture.StandardProperties.Referrer, url);
                p.Properties.Add(Picture.StandardProperties.BanImageKey, id);
                p.Properties.Add(Picture.StandardProperties.ProviderLabel, "Wallhaven");
                // Extra metadata as properties (optional)
                if (!string.IsNullOrEmpty(purity))
                    p.Properties.Add("purity", purity);
                if (!string.IsNullOrEmpty(category))
                    p.Properties.Add("category", category);

                return p;
            }
            catch { return null; }
        }

        private WallhavenMeta ParseMetaDict(Dictionary<string, object> d)
        {
            var m = new WallhavenMeta();
            try
            {
                if (d.TryGetValue("current_page", out object cp)) m.current_page = Convert.ToInt32(cp);
                if (d.TryGetValue("last_page", out object lp)) m.last_page = Convert.ToInt32(lp);
                if (d.TryGetValue("per_page", out object pp)) m.per_page = Convert.ToInt32(pp);
                if (d.TryGetValue("total", out object tot)) m.total = Convert.ToInt32(tot);
                if (d.TryGetValue("seed", out object seed)) m.seed = seed?.ToString();
            }
            catch { }
            return m;
        }

        // Manual fallback parser using simple string extraction - robust for .NET 4.0 without extra refs
        private Tuple<List<Picture>, WallhavenMeta> ParseManually(string json)
        {
            var pics = new List<Picture>();
            var meta = new WallhavenMeta() { current_page = 1, last_page = 1, per_page = 24, total = 0 };

            // Extract meta: "current_page": 1, "last_page": 4315 etc
            try
            {
                var cpMatch = System.Text.RegularExpressions.Regex.Match(json, "\"current_page\"\\s*:\\s*(\\d+)");
                if (cpMatch.Success) meta.current_page = int.Parse(cpMatch.Groups[1].Value);
                var lpMatch = System.Text.RegularExpressions.Regex.Match(json, "\"last_page\"\\s*:\\s*(\\d+)");
                if (lpMatch.Success) meta.last_page = int.Parse(lpMatch.Groups[1].Value);
                var ppMatch = System.Text.RegularExpressions.Regex.Match(json, "\"per_page\"\\s*:\\s*(\\d+)");
                if (ppMatch.Success) meta.per_page = int.Parse(ppMatch.Groups[1].Value);
                var totMatch = System.Text.RegularExpressions.Regex.Match(json, "\"total\"\\s*:\\s*(\\d+)");
                if (totMatch.Success) meta.total = int.Parse(totMatch.Groups[1].Value);
                var seedMatch = System.Text.RegularExpressions.Regex.Match(json, "\"seed\"\\s*:\\s*\"([^\"]*)\"");
                if (seedMatch.Success) meta.seed = seedMatch.Groups[1].Value;
                else
                {
                    var seedNull = System.Text.RegularExpressions.Regex.Match(json, "\"seed\"\\s*:\\s*null");
                    if (!seedNull.Success)
                    {
                        // seed may be in meta or top level for random
                        var seedAlt = System.Text.RegularExpressions.Regex.Match(json, "\"seed\"\\s*:\\s*\"([^\"]+)\"");
                        if (seedAlt.Success) meta.seed = seedAlt.Groups[1].Value;
                    }
                }
            }
            catch { }

            // SECURITY FIX: Avoid cross-contamination of IDs (BUG-06) and ReDoS
            // Instead of scanning all "id" and looking ahead 2500 chars (which mixes tag ids with wallpaper paths),
            // we extract the data array and split into individual wallpaper JSON objects using brace counting.
            try
            {
                // Find data array: "data": [ ... ]
                int dataIdx = json.IndexOf("\"data\"");
                if (dataIdx < 0) return Tuple.Create(pics, meta);
                int arrayStart = json.IndexOf('[', dataIdx);
                if (arrayStart < 0) return Tuple.Create(pics, meta);
                int arrayEnd = -1;
                int depth = 0;
                bool inString = false;
                bool escape = false;
                for (int i = arrayStart; i < json.Length; i++)
                {
                    char c = json[i];
                    if (escape) { escape = false; continue; }
                    if (c == '\\') { escape = true; continue; }
                    if (c == '"') { inString = !inString; continue; }
                    if (inString) continue;
                    if (c == '[') depth++;
                    else if (c == ']')
                    {
                        depth--;
                        if (depth == 0) { arrayEnd = i; break; }
                    }
                }
                if (arrayEnd < 0) return Tuple.Create(pics, meta);

                string dataArray = json.Substring(arrayStart + 1, arrayEnd - arrayStart - 1);

                // Split dataArray into objects using brace counting
                var objects = new List<string>();
                int objDepth = 0;
                int objStart = -1;
                inString = false;
                escape = false;
                for (int i = 0; i < dataArray.Length; i++)
                {
                    char c = dataArray[i];
                    if (escape) { escape = false; continue; }
                    if (c == '\\') { escape = true; continue; }
                    if (c == '"') { inString = !inString; continue; }
                    if (inString) continue;
                    if (c == '{')
                    {
                        if (objDepth == 0) objStart = i;
                        objDepth++;
                    }
                    else if (c == '}')
                    {
                        objDepth--;
                        if (objDepth == 0 && objStart >= 0)
                        {
                            objects.Add(dataArray.Substring(objStart, i - objStart + 1));
                            objStart = -1;
                        }
                    }
                }

                // Simplified regexes without catastrophic backtracking (no nested quantifiers)
                var idRegex = new System.Text.RegularExpressions.Regex("\"id\"\\s*:\\s*\"([^\"]+)\"", System.Text.RegularExpressions.RegexOptions.Compiled);
                var pathRegex = new System.Text.RegularExpressions.Regex("\"path\"\\s*:\\s*\"([^\"]+)\"", System.Text.RegularExpressions.RegexOptions.Compiled);
                var thumbRegex = new System.Text.RegularExpressions.Regex("\"large\"\\s*:\\s*\"([^\"]+)\"", System.Text.RegularExpressions.RegexOptions.Compiled);
                var urlRegex = new System.Text.RegularExpressions.Regex("\"url\"\\s*:\\s*\"([^\"]+)\"", System.Text.RegularExpressions.RegexOptions.Compiled);

                foreach (var obj in objects)
                {
                    var idMatch = idRegex.Match(obj);
                    var pathMatch = pathRegex.Match(obj);
                    if (!idMatch.Success || !pathMatch.Success) continue;

                    string id = idMatch.Groups[1].Value;
                    string path = pathMatch.Groups[1].Value.Replace("\\/", "/");

                    if (!path.Contains("wallhaven.cc")) continue;

                    // Sanitize id (prevent traversal)
                    string safeId = System.Text.RegularExpressions.Regex.Replace(id, @"[^a-zA-Z0-9_\-]", "");
                    if (string.IsNullOrEmpty(safeId)) continue;

                    var thumbMatch = thumbRegex.Match(obj);
                    string thumb = thumbMatch.Success ? thumbMatch.Groups[1].Value.Replace("\\/", "/") : "";

                    var urlMatch = urlRegex.Match(obj);
                    string pageUrl = urlMatch.Success ? urlMatch.Groups[1].Value.Replace("\\/", "/") : $"https://wallhaven.cc/w/{safeId}";

                    var p = new Picture() { Id = safeId, Url = path };
                    if (!string.IsNullOrEmpty(thumb))
                    {
                        // Validate thumb is http/https
                        if (thumb.StartsWith("http"))
                            p.Properties.Add(Picture.StandardProperties.Thumbnail, thumb);
                    }
                    p.Properties.Add(Picture.StandardProperties.Referrer, pageUrl);
                    p.Properties.Add(Picture.StandardProperties.BanImageKey, safeId);
                    p.Properties.Add(Picture.StandardProperties.ProviderLabel, "Wallhaven");

                    pics.Add(p);
                }

                pics = pics.GroupBy(x => x.Id).Select(g => g.First()).ToList();
            }
            catch (Exception ex)
            {
                Log.Logger.Write($"Wallhaven manual parse error: {ex.Message}", Log.LoggerLevels.Errors);
            }

            return Tuple.Create(pics, meta);
        }

        private class WallhavenMeta
        {
            public int current_page = 1;
            public int last_page = 1;
            public int per_page = 24;
            public int total = 0;
            public string seed = null;
        }
        #endregion
    }
}
