using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pulse.Base;
using System.Drawing;
using Pulse.Base.WinAPI;

namespace Piler
{
    public class PilerProvider : IOutputProvider
    {
        // FIX: Thread-safe Random + prevent infinite placement loop
        private static readonly ThreadLocal<Random> _rand = new ThreadLocal<Random>(() => new Random(Guid.NewGuid().GetHashCode()));
        private static Random Rand => _rand.Value;

        public void ProcessPicture(PictureBatch pb, string config)
        {
            List<Picture> pics = pb.GetPictures(5);

            if (!pics.Any()) return;

            // FIX: Validate cache path and backdrop existence
            string cachePath = Settings.CurrentSettings?.CachePath ?? Path.GetTempPath();
            try { Directory.CreateDirectory(cachePath); } catch { cachePath = Path.GetTempPath(); }

            Picture pBackdrop = pics.First();
            if (string.IsNullOrEmpty(pBackdrop.LocalPath) || !System.IO.File.Exists(pBackdrop.LocalPath))
            {
                Log.Logger.Write($"Piler: Backdrop file missing {pBackdrop.LocalPath}", Log.LoggerLevels.Warnings);
                return;
            }

            string savePath = Path.Combine(cachePath, $"{Guid.NewGuid()}.jpg");

            List<Rectangle> existingImages = new List<Rectangle>();

            using (Image bmpBackdrop = Image.FromFile(pBackdrop.LocalPath))
            {
                // FIX: Ensure backdrop large enough to place images
                if (bmpBackdrop.Width < 100 || bmpBackdrop.Height < 100)
                {
                    Log.Logger.Write($"Piler: Backdrop too small {bmpBackdrop.Width}x{bmpBackdrop.Height}", Log.LoggerLevels.Warnings);
                    return;
                }

                using (Graphics g = Graphics.FromImage(bmpBackdrop))
                {

                //now get 4 or 5 other pics and strew them about
                foreach (Picture p in pics.Skip(1))
                {
                    if (string.IsNullOrEmpty(p.LocalPath) || !System.IO.File.Exists(p.LocalPath))
                        continue;

                    Picture pPile = p;
                    try
                    {
                        using (Bitmap bmpToRotate = (Bitmap)Bitmap.FromFile(pPile.LocalPath))
                        {
                            using (Bitmap bmpWithBorder = PictureManager.AppendBorder(bmpToRotate, 25, Color.White))
                            {
                                using (Bitmap bmpPile = PictureManager.RotateImage(bmpWithBorder, (float)Rand.Next(-30, 30)))
                                {
                                    // FIX: Prevent ArgumentOutOfRange when backdrop small + prevent infinite loop
                                    Rectangle r = Rectangle.Empty;
                                    int attempts = 0;
                                    int maxAttempts = 100;
                                    int minX = 50;
                                    int maxX = Math.Max(minX + 1, bmpBackdrop.Width - 50);
                                    int minY = 50;
                                    int maxY = Math.Max(minY + 1, bmpBackdrop.Height - 50);
                                    int w = Convert.ToInt32(bmpBackdrop.Width * .1);
                                    int h = Convert.ToInt32(bmpBackdrop.Height * .1);

                                    while (r == Rectangle.Empty && attempts < maxAttempts)
                                    {
                                        Rectangle tmp = new Rectangle(Rand.Next(minX, maxX), Rand.Next(minY, maxY), w, h);
                                        if (!existingImages.Any(x => Rectangle.Intersect(x, tmp) != Rectangle.Empty))
                                        {
                                            r = tmp;
                                            existingImages.Add(r);
                                        }
                                        attempts++;
                                    }

                                    // If couldn't place non-overlapping after attempts, allow overlap
                                    if (r == Rectangle.Empty)
                                    {
                                        r = new Rectangle(Rand.Next(minX, maxX), Rand.Next(minY, maxY), w, h);
                                    }

                                    g.DrawImage(bmpPile, r);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Logger.Write($"Piler: Failed to process pile image {p.LocalPath}: {ex.Message}", Log.LoggerLevels.Warnings);
                    }
                }

                try
                {
                    bmpBackdrop.Save(savePath, System.Drawing.Imaging.ImageFormat.Jpeg);
                }
                catch (Exception ex)
                {
                    Log.Logger.Write($"Piler: Failed to save collage {savePath}: {ex.Message}", Log.LoggerLevels.Errors);
                    return;
                }
            }

            // Fixed: Use SystemParameterInfo instead of deprecated ActiveDesktop (removed in Win7+)
            // ActiveDesktop (IActiveDesktop) was removed after XP/Vista, so modern Windows needs SPI_SETDESKWALLPAPER
            try
            {
                Desktop.SetWallpaperUsingSystemParameterInfo(savePath);
            }
            catch
            {
                // Fallback to old method for XP/Vista compat
                try { Desktop.SetWallpaperUsingActiveDesktop(savePath); } catch { }
            }
        }

        public void Activate(object args) { }
        public void Deactivate(object args) { }
        public void Initialize(object args) { }
    }
}
