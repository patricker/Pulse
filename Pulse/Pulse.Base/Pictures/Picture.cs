using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Drawing;
using System.Net;

namespace Pulse.Base
{
    public class Picture
    {
        /// <summary>
        /// Id is the file name, without extension, used to save the file when downloaded from the internet
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// The full URL to the picture, used for downloading and banning
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// The local path to the saved file
        /// </summary>
        public string LocalPath { get; set; }

        /// <summary>
        /// Used for tracking which provider instance returned this picture
        /// </summary>
        public Guid ProviderInstance { get; set; }

        /// <summary>
        /// may be used for storing other properties of the file such as dimensions, thumbnail url, etc...
        /// </summary>
        public SerializableDictionary<string, string> Properties { get; set; }

        private Image _cachedThumb = null;

        public Picture()
        {
            Properties = new SerializableDictionary<string, string>();
        }

        public string CalculateLocalPath(string baseFolder)
        {
            // SECURITY FIX: Sanitize Id to prevent path traversal (CRIT-1)
            // Old regex allowed ../../Providers/evil -> DLL plant RCE via Assembly.LoadFrom
            string safeId = Id ?? "unknown";
            // Only allow alphanumeric, dash, underscore - strip everything else
            safeId = System.Text.RegularExpressions.Regex.Replace(safeId, @"[^a-zA-Z0-9_\-]", "");
            if (string.IsNullOrEmpty(safeId)) safeId = Guid.NewGuid().ToString("N");
            if (safeId.Length > 50) safeId = safeId.Substring(0, 50);

            string ext = ".jpg";
            try
            {
                if (!string.IsNullOrEmpty(Url) && Uri.TryCreate(Url, UriKind.Absolute, out var uri))
                {
                    // SECURITY: Only http/https allowed for wallpaper URLs
                    if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
                        ext = ".jpg";
                    else
                    {
                        string parsedExt = Path.GetExtension(uri.AbsolutePath);
                        if (!string.IsNullOrEmpty(parsedExt) && parsedExt.Length <= 5)
                            ext = parsedExt;
                    }
                }
            }
            catch { ext = ".jpg"; }

            string combined = Path.Combine(baseFolder, safeId + ext);
            // Canonicalize and ensure inside baseFolder
            try
            {
                string fullBase = Path.GetFullPath(baseFolder);
                string fullCombined = Path.GetFullPath(combined);
                if (!fullCombined.StartsWith(fullBase, StringComparison.OrdinalIgnoreCase))
                {
                    // Traversal attempt - fallback to safe location inside base
                    combined = Path.Combine(fullBase, safeId + ext);
                }
            }
            catch { }

            return combined;
        }

        /// <summary>
        /// Does the file appear to be present and in good shape (size > 0kb)
        /// </summary>
        public bool IsGood
        {
            get
            {
                if (string.IsNullOrEmpty(LocalPath)) return false;

                FileInfo fi = new FileInfo(LocalPath);
                return (fi.Exists && fi.Length > 0);
            }
        }

        public Image GetThumbnail()
        {
            if (_cachedThumb == null)
            {
                // SECURITY FIX: Block file:// and private IPs, add timeout, proper dispose
                try
                {
                    Properties = Properties ?? new SerializableDictionary<string, string>();

                    string thumbUrl = null;
                    if (Properties.ContainsKey(StandardProperties.Thumbnail))
                        thumbUrl = Properties[Picture.StandardProperties.Thumbnail];

                    // Validate URL scheme - only http/https allowed
                    string targetUrl = thumbUrl ?? Url;
                    if (string.IsNullOrEmpty(targetUrl)) return null;

                    if (Uri.TryCreate(targetUrl, UriKind.Absolute, out var uri))
                    {
                        if (uri.Scheme == Uri.UriSchemeFile)
                        {
                            // SECURITY: file:// blocked for thumbnails (SSRF)
                            Log.Logger.Write($"GetThumbnail blocked file:// URL: {targetUrl}", Log.LoggerLevels.Warnings);
                            if (Properties.ContainsKey(StandardProperties.Thumbnail))
                            {
                                // Try Url as fallback if thumb was file://
                                targetUrl = Url;
                                if (!Uri.TryCreate(targetUrl, UriKind.Absolute, out uri) || uri.Scheme == Uri.UriSchemeFile)
                                    return null;
                            }
                            else
                            {
                                return null;
                            }
                        }

                        // Block private IPs for SSRF (basic check)
                        if (uri.Host == "localhost" || uri.Host == "127.0.0.1" || uri.Host.StartsWith("192.168.") || uri.Host.StartsWith("10.") || uri.Host == "169.254.169.254")
                        {
                            Log.Logger.Write($"GetThumbnail blocked private IP URL: {targetUrl}", Log.LoggerLevels.Warnings);
                            return null;
                        }

                        if (uri.Scheme == Uri.UriSchemeFile)
                        {
                            _cachedThumb = PictureManager.ShrinkImage(uri.LocalPath, 0, 150);
                        }
                        else
                        {
                            using (var client = new HttpUtility.CookieAwareWebClient())
                            {
                                client.UserAgent = "Pulse/2.0 (Thumbnail)";
                                using (var stream = client.OpenRead(targetUrl))
                                {
                                    if (stream != null)
                                    {
                                        using (var ms = new MemoryStream())
                                        {
                                            stream.CopyTo(ms);
                                            ms.Position = 0;
                                            using (var img = Image.FromStream(ms))
                                            {
                                                _cachedThumb = PictureManager.ShrinkImage(img, 0, 150);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Logger.Write($"GetThumbnail failed for {Url}: {ex.Message}", Log.LoggerLevels.Warnings);
                }
            }

            return _cachedThumb;
        }

        public ActiveProviderInfo ActiveProviderInfo
        {
            get {
                var a = Settings.CurrentSettings.ProviderSettings.Where(api => api.Key == ProviderInstance);
                if (a.Any()) return a.First().Value;

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
