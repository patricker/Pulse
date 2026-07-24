using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;

namespace Pulse.Base
{
    /// <summary>
    /// Modern PictureManager using single library ImageSharp for all platforms.
    /// No System.Drawing.Common dependency - uses SixLabors.ImageSharp for all image ops.
    /// This replaces the legacy GDI+ based PictureManager that used Bitmap/Graphics/Matrix.
    /// </summary>
    public partial class PictureManager
    {
        #region Screen resolution helpers (still uses WinForms Screen for Windows, but no System.Drawing.Common)

        public static Pair<int, int> PrimaryScreenResolution
        {
            get
            {
                try
                {
                    var rect = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
                    return new Pair<int, int>(rect.Width, rect.Height);
                }
                catch
                {
                    // Fallback for headless or non-Windows
                    return new Pair<int, int>(1920, 1080);
                }
            }
        }

        public static List<Rectangle> ScreenResolutions
        {
            get
            {
                var list = new List<Rectangle>();
                try
                {
                    list.AddRange(from c in System.Windows.Forms.Screen.AllScreens select c.Bounds);
                }
                catch
                {
                    list.Add(new Rectangle(0, 0, 1920, 1080));
                }
                return list;
            }
        }

        public static Rectangle TotalScreenResolution
        {
            get
            {
                var rects = ScreenResolutions;
                Rectangle rect = new Rectangle(0, 0, 0, 0);
                foreach (Rectangle r in rects)
                {
                    rect = Rectangle.Union(rect, r);
                }
                return rect;
            }
        }

        #endregion

        #region ImageSharp-based implementations

        public static void ShrinkImage(string imgPath, string outPath, int destWidth, int destHeight, int quality)
        {
            using (var img = SixLabors.ImageSharp.Image.Load<Rgba32>(imgPath))
            {
                var resized = ShrinkImageInternal(img, destWidth, destHeight);
                SaveWithQuality(resized, outPath, quality);
                if (!ReferenceEquals(resized, img))
                    resized.Dispose();
            }
        }

        public static Image<Rgba32> ShrinkImage(string imgPath, int destWidth, int destHeight)
        {
            var img = SixLabors.ImageSharp.Image.Load<Rgba32>(imgPath);
            return ShrinkImageInternal(img, destWidth, destHeight);
        }

        public static Image<Rgba32> ShrinkImage(SixLabors.ImageSharp.Image<Rgba32> img, int destWidth, int destHeight)
        {
            return ShrinkImageInternal(img, destWidth, destHeight);
        }

        // Backward compat overloads that take System.Drawing.Image (for legacy callers still using old API during transition)
        // These will convert to ImageSharp, process, then convert back to Bitmap for compat - but we want to avoid System.Drawing
        // For pure ImageSharp path, use the Rgba32 overloads above.

        private static Image<Rgba32> ShrinkImageInternal(Image<Rgba32> img, int destWidth, int destHeight)
        {
            // Handle 0,0 = use primary screen resolution
            if (destWidth == 0 && destHeight == 0)
            {
                var sc = PrimaryScreenResolution;
                destWidth = sc.First;
                destHeight = sc.Second;
            }
            else if (destWidth > 0 && destHeight == 0)
            {
                destHeight = (int)Math.Round((double)img.Height * destWidth / img.Width);
            }
            else if (destWidth == 0 && destHeight > 0)
            {
                destWidth = (int)Math.Round((double)img.Width * destHeight / img.Height);
            }

            if (destWidth <= 0) destWidth = img.Width;
            if (destHeight <= 0) destHeight = img.Height;

            // Calculate source crop to maintain aspect and fill destination (center crop)
            double origRatio = (double)img.Width / img.Height;
            double destRatio = (double)destWidth / destHeight;

            int sourceWidth = img.Width;
            int sourceHeight = img.Height;
            int sourceX = 0;
            int sourceY = 0;

            if (destRatio > origRatio)
            {
                // Destination is wider than source - crop height
                sourceHeight = (int)(img.Width / destRatio);
                sourceY = (img.Height - sourceHeight) / 2;
            }
            else if (destRatio < origRatio)
            {
                // Destination taller - crop width
                sourceWidth = (int)(img.Height * destRatio);
                sourceX = (img.Width - sourceWidth) / 2;
            }

            var clone = img.Clone();
            clone.Mutate(x =>
            {
                // Crop to fill aspect
                x.Crop(new SixLabors.ImageSharp.Rectangle(sourceX, sourceY, sourceWidth, sourceHeight));
                // Resize to dest
                x.Resize(new ResizeOptions
                {
                    Size = new SixLabors.ImageSharp.Size(destWidth, destHeight),
                    Mode = ResizeMode.Stretch,
                    Sampler = KnownResamplers.Lanczos3
                });
            });

            return clone;
        }

        public static Image<Rgba32> RotateImage(Image<Rgba32> bmpSrc, float theta)
        {
            // ImageSharp rotate handles bounding box automatically
            var clone = bmpSrc.Clone();
            clone.Mutate(x => x.Rotate(theta));
            return clone;
        }

        public static Color CalcAverageColor(Image<Rgba32> image)
        {
            // Resize to 1x1 and get pixel - no gamma correction, simple average
            using (var small = image.Clone())
            {
                small.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new SixLabors.ImageSharp.Size(1, 1),
                    Mode = ResizeMode.Stretch,
                    Sampler = KnownResamplers.Lanczos3
                }));

                Rgba32 pixel = small[0, 0];
                // Convert to System.Drawing.Color for compat with old API that expects System.Drawing.Color
                // System.Drawing.Color is in System.Drawing.Primitives, not Common, so okay
                return Color.FromArgb(pixel.R, pixel.G, pixel.B);
            }
        }

        // Note: System.Drawing.Bitmap overload removed in ImageSharp-only build
        // Use Image<Rgba32> overload for all new code

        public static Image<Rgba32> AppendBorder(Image<Rgba32> original, int borderWidth, Color borderColor)
        {
            // Convert System.Drawing.Color to Rgba32
            var rgba = new Rgba32(borderColor.R, borderColor.G, borderColor.B, borderColor.A);

            int newWidth = original.Width + borderWidth * 2;
            int newHeight = original.Height + borderWidth * 2;

            var newImage = new Image<Rgba32>(newWidth, newHeight);
            newImage.Mutate(x =>
            {
                x.BackgroundColor(rgba);
                x.DrawImage(original, new SixLabors.ImageSharp.Point(borderWidth, borderWidth), 1f);
            });

            return newImage;
        }

        public static void ReduceQuality(string file, string destFile, int quality)
        {
            try
            {
                using (var img = SixLabors.ImageSharp.Image.Load(file))
                {
                    // Ensure quality 0-100
                    quality = Math.Max(0, Math.Min(100, quality));
                    int currentQuality = quality;
                    var encoder = new JpegEncoder { Quality = currentQuality };

                    // Try to get under 245KB, reducing quality by 10 each iteration
                    long maxBytes = 245 * 1024;
                    int attempts = 0;
                    do
                    {
                        using (var fs = new FileStream(destFile, FileMode.Create, FileAccess.Write))
                        {
                            img.Save(fs, encoder);
                        }

                        var fi = new FileInfo(destFile);
                        if (fi.Length < maxBytes) break;

                        currentQuality -= 10;
                        if (currentQuality <= 0) break;

                        encoder = new JpegEncoder { Quality = currentQuality };
                        attempts++;
                    } while (attempts < 10 && currentQuality > 0);
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Write($"ReduceQuality failed for {file}: {ex.Message}", Log.LoggerLevels.Errors);
            }
        }

        private static void SaveWithQuality(Image<Rgba32> img, string outPath, int quality)
        {
            quality = Math.Max(0, Math.Min(100, quality));
            string ext = Path.GetExtension(outPath).ToLowerInvariant();

            if (ext == ".png")
            {
                img.SaveAsPng(outPath);
            }
            else
            {
                var encoder = new JpegEncoder { Quality = quality };
                img.Save(outPath, encoder);
            }
        }

        #endregion

        // Legacy GetPictureList, LoadCachedSearch remain in legacy file, but we need to ensure they work
        // For modern build, we exclude legacy PictureManager.cs and keep only this ImageSharp version plus legacy helpers
        // So we need to duplicate GetPictureList logic here or ensure legacy file is excluded except helpers

        public PictureList GetPictureList(PictureSearch ps)
        {
            PictureList Pictures = null;

            if (ps == null || ps.SearchProvider == null || ps.SearchProvider.Instance == null) return Pictures;
            Pictures = ps.SearchProvider.SearchResults;

            var loadedFromFile = false;
            var fPath = Path.Combine(ps.SaveFolder, "CACHE_" + ps.GetSearchHash().ToString() + "_" + ps.SearchProvider.Instance.GetType().ToString() + ".xml");

            if (Pictures == null)
            {
                Pictures = LoadCachedSearch(ps, fPath);
                loadedFromFile = Pictures != null;
            }
            else
            {
                loadedFromFile = false;
            }

            if (Pictures == null || Pictures.Pictures.Count == 0 || Pictures.ExpirationDate < DateTime.Now)
            {
                Pictures = ((IInputProvider)ps.SearchProvider.Instance).GetPictures(ps);
                Pictures.SearchSettingsHash = ps.GetSearchHash();
                loadedFromFile = false;
            }

            if (!loadedFromFile)
            {
                Pictures.Pictures.ForEach(x => x.ProviderInstance = ps.SearchProvider.ProviderInstanceID);
                try { Pictures.Save(fPath); } catch { }
            }

            return Pictures;
        }

        public PictureList LoadCachedSearch(PictureSearch ps, string cachePath)
        {
            PictureList result = null;
            if (File.Exists(cachePath))
            {
                try
                {
                    result = PictureList.LoadFromFile(cachePath);
                }
                catch (Exception ex)
                {
                    Log.Logger.Write($"Error loading picture cache from file, cache will not be used. {ex}", Log.LoggerLevels.Errors);
                }
            }
            return result;
        }
    }
}
