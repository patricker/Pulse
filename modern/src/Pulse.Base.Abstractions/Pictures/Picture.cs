using System;
using System.Collections.Generic;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Pulse.Base
{
    public class Picture
    {
        public string Id { get; set; } = "";
        public string Url { get; set; } = "";
        public string LocalPath { get; set; } = "";
        public Guid ProviderInstance { get; set; }
        public SerializableDictionary<string, string> Properties { get; set; } = new();

        private Image<Rgba32>? _cachedThumbSharp = null;

        public Picture()
        {
            Properties = new SerializableDictionary<string, string>();
        }

        public string CalculateLocalPath(string baseFolder)
        {
            string safeId = Id ?? "unknown";
            safeId = System.Text.RegularExpressions.Regex.Replace(safeId, @"[^a-zA-Z0-9_\-]", "");
            if (string.IsNullOrEmpty(safeId)) safeId = Guid.NewGuid().ToString("N");
            if (safeId.Length > 50) safeId = safeId.Substring(0, 50);

            string ext = ".jpg";
            try
            {
                if (!string.IsNullOrEmpty(Url) && Uri.TryCreate(Url, UriKind.Absolute, out var uri))
                {
                    if (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
                    {
                        string parsedExt = Path.GetExtension(uri.AbsolutePath).ToLowerInvariant();
                        if (parsedExt == ".jpg" || parsedExt == ".jpeg" || parsedExt == ".png" || parsedExt == ".webp")
                            ext = parsedExt;
                    }
                }
            }
            catch { ext = ".jpg"; }

            string combined = Path.Combine(baseFolder, safeId + ext);
            try
            {
                string fullBase = Path.GetFullPath(baseFolder);
                string fullCombined = Path.GetFullPath(combined);
                if (!fullCombined.StartsWith(fullBase, StringComparison.OrdinalIgnoreCase))
                    combined = Path.Combine(fullBase, safeId + ext);
            }
            catch { }

            return combined;
        }

        public bool IsGood
        {
            get
            {
                if (string.IsNullOrEmpty(LocalPath)) return false;
                var fi = new FileInfo(LocalPath);
                return fi.Exists && fi.Length > 0;
            }
        }

        public Image<Rgba32>? GetThumbnailSharp()
        {
            if (_cachedThumbSharp != null) return _cachedThumbSharp;

            try
            {
                Properties ??= new SerializableDictionary<string, string>();

                string? thumbUrl = Properties.ContainsKey(StandardProperties.Thumbnail) ? Properties[StandardProperties.Thumbnail] : null;
                string targetUrl = thumbUrl ?? Url;
                if (string.IsNullOrEmpty(targetUrl)) return null;

                if (!Uri.TryCreate(targetUrl, UriKind.Absolute, out var uri)) return null;
                if (uri.Scheme == Uri.UriSchemeFile) return null;
                if (uri.Host == "localhost" || uri.Host == "127.0.0.1" || uri.Host == "::1") return null;

                using (var http = new System.Net.Http.HttpClient())
                {
                    http.DefaultRequestHeaders.UserAgent.ParseAdd("Pulse/2.0 (Thumbnail)");
                    byte[] data = http.GetByteArrayAsync(targetUrl).GetAwaiter().GetResult();
                    if (data == null || data.Length == 0) return null;

                    var img = Image.Load<Rgba32>(data);
                    int destHeight = 150;
                    int destWidth = (int)Math.Round((double)img.Width * destHeight / img.Height);
                    if (destWidth <= 0) destWidth = 150;

                    var thumb = img.Clone();
                    thumb.Mutate(x => x.Resize(destWidth, destHeight, KnownResamplers.Bilinear));
                    _cachedThumbSharp = thumb;
                    return thumb;
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Write($"GetThumbnailSharp failed for {Url}: {ex.Message}", Log.LoggerLevels.Warnings);
                return null;
            }
        }

        public static class StandardProperties
        {
            public static readonly string Thumbnail = "thumb";
            public static readonly string Referrer = "referrer";
            public static readonly string BanImageKey = "banImageKey";
            public static readonly string ProviderLabel = "providerLabel";
        }
    }
}
