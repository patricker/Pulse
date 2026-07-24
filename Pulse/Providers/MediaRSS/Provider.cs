using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pulse.Base;
using Pulse.Base.Providers;
using System.Text.RegularExpressions;
using System.Windows;
using System.ComponentModel;
using System.Xml;
using System.Xml.Linq;

namespace MediaRSSProvider
{
    [ProviderConfigurationClass(typeof(MediaRSSImageSearchSettings))]
    [System.ComponentModel.Description("MediaRSS Feed")]
    [ProviderIcon(typeof(Properties.Resources),"rss")]
    public class Provider : IInputProvider
    {        
        public Provider()
        {
        }

        public void Initialize(object args)
        {
            //nothing to do here
        }

        public void Activate(object args) { }
        public void Deactivate(object args) { }


        public PictureList GetPictures(PictureSearch ps)
        {
            var result = new PictureList() { FetchDate = DateTime.Now };
            MediaRSSImageSearchSettings mrssiss = string.IsNullOrEmpty(ps.SearchProvider.ProviderConfig) ?
                new MediaRSSImageSearchSettings() : MediaRSSImageSearchSettings.LoadFromXML(ps.SearchProvider.ProviderConfig);

            // Ensure TLS 1.2
            try { System.Net.ServicePointManager.SecurityProtocol |= (System.Net.SecurityProtocolType)3072; } catch { }

            // SECURITY FIX: Validate URL scheme, block file:// and private IPs, remove XXE fallback
            string rssUrl = mrssiss.MediaRSSURL ?? "";
            if (string.IsNullOrEmpty(rssUrl))
            {
                Log.Logger.Write("MediaRSS: Empty URL", Log.LoggerLevels.Warnings);
                return result;
            }

            if (!Uri.TryCreate(rssUrl, UriKind.Absolute, out var rssUri) || (rssUri.Scheme != Uri.UriSchemeHttp && rssUri.Scheme != Uri.UriSchemeHttps))
            {
                Log.Logger.Write($"MediaRSS: Blocked non-http URL {rssUrl}", Log.LoggerLevels.Warnings);
                return result;
            }

            // Basic SSRF private IP check
            string host = rssUri.Host.ToLower();
            if (host == "localhost" || host == "127.0.0.1" || host.StartsWith("192.168.") || host.StartsWith("10.") || host == "169.254.169.254" || host == "::1")
            {
                Log.Logger.Write($"MediaRSS: Blocked private IP host {host}", Log.LoggerLevels.Warnings);
                return result;
            }

            XDocument feedXML = null;
            try
            {
                using (var wc = new Pulse.Base.HttpUtility.CookieAwareWebClient())
                {
                    wc.UserAgent = "Pulse/2.0 (MediaRSS; +https://github.com/patricker/Pulse)";
                    string xmlStr = wc.DownloadString(rssUrl);

                    // SECURITY: Parse with DtdProcessing.Prohibit and XmlResolver null to prevent XXE
                    var settings = new XmlReaderSettings()
                    {
                        DtdProcessing = DtdProcessing.Prohibit,
                        XmlResolver = null,
                        MaxCharactersFromEntities = 1024,
                        Async = false
                    };
                    using (var sr = new System.IO.StringReader(xmlStr))
                    using (var xr = XmlReader.Create(sr, settings))
                    {
                        feedXML = XDocument.Load(xr);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Write($"MediaRSS: Failed to load/parse RSS {rssUrl}: {ex.Message}", Log.LoggerLevels.Errors);
                return result;
            }

            XNamespace media = XNamespace.Get("http://search.yahoo.com/mrss/");

            try
            {
                var banned = ps.BannedURLs ?? new List<string>();

                var feeds = from feed in feedXML.Descendants("item")
                            let content = feed.Descendants(media + "content")
                            let thumbElem = feed.Descendants(media + "thumbnail").FirstOrDefault() ?? feed.Element(media + "thumbnail")
                            let thumb = thumbElem != null ? thumbElem.Attribute("url")?.Value ?? "" : ""
                            // Accept when medium missing or medium==image or type is image
                            let img = content.FirstOrDefault(x =>
                                (x.Attribute("medium") == null || x.Attribute("medium").Value == "image") ||
                                (x.Attribute("type") != null && x.Attribute("type").Value.StartsWith("image")))
                            let url = img != null ? (img.Attribute("url")?.Value ?? thumb) : thumb
                            where !string.IsNullOrEmpty(url) && (url.StartsWith("http://") || url.StartsWith("https://"))
                            let safeUrl = url.Split('?')[0] // strip query for id
                            let id = System.IO.Path.GetFileNameWithoutExtension(safeUrl)
                            select new Picture()
                            {
                                Url = url,
                                Id = string.IsNullOrEmpty(id) ? Guid.NewGuid().ToString("N").Substring(0, 20) : (id.Length > 50 ? id.Substring(0, 50) : id),
                                Properties = new SerializableDictionary<string, string>(Picture.StandardProperties.Thumbnail, thumb)
                            };

                int max = ps.MaxPictureCount > 0 ? ps.MaxPictureCount : 100; // cap int.MaxValue
                max = Math.Min(max, 200);
                result.Pictures.AddRange(
                        feeds.Where(x => !banned.Contains(x.Url))
                        .Take(max));
            }
            catch (Exception ex)
            {
                Log.Logger.Write($"MediaRSS: Failed to parse feed {mrssiss.MediaRSSURL}: {ex}", Log.LoggerLevels.Errors);
            }

            return result;
        }
    }
}
