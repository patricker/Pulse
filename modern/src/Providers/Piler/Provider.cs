using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Pulse.Base;
using Pulse.Base.Wallpaper;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;

namespace Piler
{
    /// <summary>
    /// Modern Piler using ImageSharp only - no System.Drawing.Common
    /// Creates collage with backdrop + 4 rotated images with white border
    /// </summary>
    public class PilerProvider : IOutputProvider
    {
        private static readonly ThreadLocal<Random> _rand = new(() => new Random(Guid.NewGuid().GetHashCode()));
        private static Random Rand => _rand.Value!;

        public void ProcessPicture(PictureBatch pb, string config)
        {
            var pics = pb.GetPictures(5);
            if (!pics.Any()) return;

            string cachePath = Settings.CurrentSettings?.CachePath ?? Path.GetTempPath();
            try { Directory.CreateDirectory(cachePath); } catch { cachePath = Path.GetTempPath(); }

            var pBackdrop = pics.First();
            if (string.IsNullOrEmpty(pBackdrop.LocalPath) || !File.Exists(pBackdrop.LocalPath))
            {
                Log.Logger.Write($"Piler: Backdrop missing {pBackdrop.LocalPath}", Log.LoggerLevels.Warnings);
                return;
            }

            string savePath = Path.Combine(cachePath, $"{Guid.NewGuid()}.jpg");

            try
            {
                using (var backdrop = Image.Load<Rgba32>(pBackdrop.LocalPath))
                {
                    if (backdrop.Width < 100 || backdrop.Height < 100)
                    {
                        Log.Logger.Write($"Piler: Backdrop too small {backdrop.Width}x{backdrop.Height}", Log.LoggerLevels.Warnings);
                        return;
                    }

                    var existing = new List<Rectangle>();

                    foreach (var p in pics.Skip(1))
                    {
                        if (string.IsNullOrEmpty(p.LocalPath) || !File.Exists(p.LocalPath)) continue;

                        try
                        {
                            using (var src = Image.Load<Rgba32>(p.LocalPath))
                            {
                                // Add white border 25px
                                int border = 25;
                                int borderedW = src.Width + border * 2;
                                int borderedH = src.Height + border * 2;

                                using (var withBorder = new Image<Rgba32>(borderedW, borderedH))
                                {
                                    withBorder.Mutate(x =>
                                    {
                                        x.BackgroundColor(Color.White);
                                        x.DrawImage(src, new Point(border, border), 1f);
                                    });

                                    // Rotate -30..30 degrees
                                    float angle = Rand.Next(-30, 30);
                                    withBorder.Mutate(x => x.Rotate(angle));

                                    // Placement logic with maxAttempts to avoid infinite loop
                                    var r = Rectangle.Empty;
                                    int attempts = 0;
                                    const int maxAttempts = 100;

                                    int minX = 50;
                                    int maxX = Math.Max(minX + 1, backdrop.Width - 50);
                                    int minY = 50;
                                    int maxY = Math.Max(minY + 1, backdrop.Height - 50);
                                    int w = (int)(backdrop.Width * 0.1);
                                    int h = (int)(backdrop.Height * 0.1);

                                    // Scale rotated image to target size for placement check
                                    // Use actual rotated size after rotation
                                    int actualW = withBorder.Width;
                                    int actualH = withBorder.Height;
                                    // If rotated bigger than target, shrink to target
                                    if (actualW > w || actualH > h)
                                    {
                                        float scale = Math.Min((float)w / actualW, (float)h / actualH);
                                        actualW = (int)(actualW * scale);
                                        actualH = (int)(actualH * scale);
                                        withBorder.Mutate(x => x.Resize(actualW, actualH));
                                    }
                                    else
                                    {
                                        actualW = w;
                                        actualH = h;
                                    }

                                    while (r == Rectangle.Empty && attempts < maxAttempts)
                                    {
                                        var tmp = new Rectangle(Rand.Next(minX, maxX), Rand.Next(minY, maxY), actualW, actualH);
                                        if (!existing.Any(ex => ex.IntersectsWith(tmp)))
                                        {
                                            r = tmp;
                                            existing.Add(r);
                                        }
                                        attempts++;
                                    }

                                    if (r == Rectangle.Empty)
                                    {
                                        r = new Rectangle(Rand.Next(minX, maxX), Rand.Next(minY, maxY), actualW, actualH);
                                    }

                                    // Draw onto backdrop
                                    backdrop.Mutate(x => x.DrawImage(withBorder, new Point(r.X, r.Y), 1f));
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Logger.Write($"Piler: Failed pile image {p.LocalPath}: {ex.Message}", Log.LoggerLevels.Warnings);
                        }
                    }

                    // Save with quality 90 (not default 75)
                    var encoder = new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder { Quality = 90 };
                    backdrop.Save(savePath, encoder);
                }

                // Set wallpaper via cross-platform abstraction
                try
                {
                    var setter = Pulse.Base.Wallpaper.WallpaperSetterFactory.Create();
                    if (setter.IsSupported)
                    {
                        setter.SetWallpaperAsync(savePath, Pulse.Base.Wallpaper.WallpaperStyle.Fill).GetAwaiter().GetResult();
                    }
                    else
                    {
                        // Fallback to legacy Desktop for Windows builds that still reference it
                        Desktop.SetWallpaperUsingSystemParameterInfo(savePath);
                    }
                }
                catch
                {
                    try { Desktop.SetWallpaperUsingSystemParameterInfo(savePath); } catch { }
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Write($"Piler: Failed to create collage: {ex.Message}", Log.LoggerLevels.Errors);
            }
        }

        public void Activate(object args) { }
        public void Deactivate(object args) { }
        public void Initialize(object args) { }
    }
}
