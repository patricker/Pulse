using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Pulse.Base;
using Pulse.Base.Providers;

namespace BingWallpaper
{
    [System.ComponentModel.Description("Bing Wallpaper (daily, no key needed)")]
    [ProviderIcon(typeof(Properties.Resources), "bing")]
    public class Provider : IInputProvider
    {
        // FIX: Use SocketsHttpHandler with PooledConnectionLifetime for DNS refresh, static reuse avoids exhaustion
        private static readonly HttpClient _http = new HttpClient(new SocketsHttpHandler
        {
            AutomaticDecompression = System.Net.DecompressionMethods.All,
            PooledConnectionLifetime = TimeSpan.FromMinutes(5)
        });

        static Provider()
        {
            _http.DefaultRequestHeaders.UserAgent.ParseAdd("Pulse/2.0 (BingWallpaper; +https://github.com/patricker/Pulse)");
        }

        private const string BingApi = "https://www.bing.com/HPImageArchive.aspx?format=js&idx=0&n=8&mkt=en-US";

        public void Initialize(object args) { }

        public void Activate(object args) { }
        public void Deactivate(object args) { }

        public PictureList GetPictures(PictureSearch ps)
        {
            // FIX: Avoid deadlock on UI STA thread via Task.Run
            return Task.Run(() => GetPicturesAsync(ps)).GetAwaiter().GetResult();
        }

        public async Task<PictureList> GetPicturesAsync(PictureSearch ps)
        {
            var result = new PictureList() { FetchDate = DateTime.Now };

            try
            {
                string json = await _http.GetStringAsync(BingApi).ConfigureAwait(false);
                using var doc = JsonDocument.Parse(json);

                if (!doc.RootElement.TryGetProperty("images", out var images))
                    return result;

                int max = ps.MaxPictureCount > 0 ? ps.MaxPictureCount : int.MaxValue;
                int count = 0;

                foreach (var img in images.EnumerateArray())
                {
                    if (count >= max) break;

                    string urlBase = img.GetProperty("url").GetString() ?? "";
                    string url = urlBase.StartsWith("http") ? urlBase : "https://www.bing.com" + urlBase;
                    // Get UDH (Ultra high def) version: replace 1920x1080 with UDH
                    string udhUrl = url.Replace("1920x1080", "UHD").Replace("_1920x1080", "_UHD");

                    string copyright = img.TryGetProperty("copyright", out var cp) ? cp.GetString() ?? "" : "";
                    string title = img.TryGetProperty("title", out var t) ? t.GetString() ?? "" : "";

                    string id = System.IO.Path.GetFileNameWithoutExtension(udhUrl);
                    if (string.IsNullOrEmpty(id)) id = Guid.NewGuid().ToString();
                    if (id.Length > 50) id = id.Substring(0, 50);

                    var banned = ps.BannedURLs ?? new List<string>();
                    if (banned.Contains(udhUrl) || banned.Contains(url))
                        continue;

                    var pic = new Picture()
                    {
                        Id = id,
                        Url = udhUrl
                    };
                    pic.Properties.Add(Picture.StandardProperties.Thumbnail, url);
                    pic.Properties.Add(Picture.StandardProperties.Referrer, "https://www.bing.com/");
                    pic.Properties.Add(Picture.StandardProperties.BanImageKey, id);
                    pic.Properties.Add(Picture.StandardProperties.ProviderLabel, "Bing");
                    if (!string.IsNullOrEmpty(copyright))
                        pic.Properties.Add("copyright", copyright);
                    if (!string.IsNullOrEmpty(title))
                        pic.Properties.Add("title", title);

                    result.Pictures.Add(pic);
                    count++;
                }

                Log.Logger.Write($"BingWallpaper: Got {result.Pictures.Count} images", Log.LoggerLevels.Info);
            }
            catch (Exception ex)
            {
                Log.Logger.Write($"BingWallpaper API error: {ex}", Log.LoggerLevels.Errors);
            }

            return result;
        }
    }

    // Minimal Resources stub - cached bitmap to avoid GDI leak
    internal class Properties
    {
        internal class Resources
        {
            private static readonly Lazy<System.Drawing.Bitmap> _bing = new(() => new System.Drawing.Bitmap(16, 16));
            public static System.Drawing.Bitmap bing => _bing.Value;
        }
    }
}
