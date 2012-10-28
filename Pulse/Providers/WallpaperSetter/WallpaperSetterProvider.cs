using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pulse.Base;
using Pulse.Base.Providers;
using Microsoft.Win32;
using Pulse.Base.WinAPI;
using System.Drawing;

namespace WallpaperSetter
{
    [System.ComponentModel.Description("Desktop Wallpaper")]
    [ProviderPlatform(PlatformID.Win32NT, 6, 0)]
    [ProviderConfigurationClass(typeof(WallpaperSetterSettings))]
    public class WallpaperSetterProvider : IOutputProvider
    {
        public void ProcessPicture(PictureBatch pb, string config)
        {
            List<Picture> lp = pb.GetPictures(1);
            if (!lp.Any()) return;
            Picture p = lp.First();

            //deserialize configuration
            WallpaperSetterSettings wss = null;

            if (!string.IsNullOrEmpty(config)) { wss = WallpaperSetterSettings.LoadFromXML(config); }
            else wss = new WallpaperSetterSettings();

            //set wallpaper style (tiled, centered, etc...)
            SetWallpaperType(wss.Position);

            //set desktop background color
            //Code came roughly form http://www.tek-tips.com/viewthread.cfm?qid=1449619
            if (wss.BackgroundColorMode == WallpaperSetterSettings.BackgroundColorModes.Specific)
            {
                int[] aiElements = { WinAPI.COLOR_DESKTOP };
                WinAPI.SetSysColors(1, aiElements, new WinAPI.COLORREF(wss.Color));
            }
            else if (wss.BackgroundColorMode == WallpaperSetterSettings.BackgroundColorModes.Computed)
            {
                using(Bitmap bmp = (Bitmap)Image.FromFile(p.LocalPath)) {
                    int[] aiElements = { WinAPI.COLOR_DESKTOP };
                    WinAPI.SetSysColors(1, aiElements, new WinAPI.COLORREF(CalcAverageColor(bmp)));
                }
            }

            //set the wallpaper to the new image
            WinAPI.SystemParametersInfo(WinAPI.SPI_SETDESKWALLPAPER, 0, p.LocalPath, WinAPI.SPIF_UPDATEINIFILE | WinAPI.SPIF_SENDWININICHANGE);
            //check if really set and retry up to 3 times
            int tryCount = 0;
            do
            {
                //if matching, break
                if (GetWallpaper().ToLower() == p.LocalPath.ToLower()) break;

                WinAPI.SystemParametersInfo(WinAPI.SPI_SETDESKWALLPAPER, 0, p.LocalPath, WinAPI.SPIF_UPDATEINIFILE | WinAPI.SPIF_SENDWININICHANGE);

                tryCount++;
            } while (tryCount < 3);
        }

        //hey... code you look an awfull lot like the block that lives in AeroGlassChangerProvider...
        public static Color CalcAverageColor(System.Drawing.Bitmap image)
        {
            var bmp = new System.Drawing.Bitmap(1, 1);
            var orig = image;
            using (var g = System.Drawing.Graphics.FromImage(bmp))
            {
                // the Interpolation mode needs to be set to 
                // HighQualityBilinear or HighQualityBicubic or this method
                // doesn't work at all.  With either setting, the results are
                // slightly different from the averaging method.
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(orig, new System.Drawing.Rectangle(0, 0, 1, 1));
            }
            var pixel = bmp.GetPixel(0, 0);
            orig.Dispose();
            bmp.Dispose();
            // pixel will contain average values for entire orig Bitmap
            return Color.FromArgb(pixel.R, pixel.G, pixel.B);
        }

        private void SetWallpaperType(WallpaperSetterSettings.PicturePositions position)
        {
            if (position == WallpaperSetterSettings.PicturePositions.NotSet) return;

            //vars
            string wallpaperStyle = "0";
            string tileWallpaper = "0";

            // Set to the appropriate background position
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);

            switch (position)
            {
                case WallpaperSetterSettings.PicturePositions.Fill:
                    wallpaperStyle = "10";
                    tileWallpaper = "0";
                    break;
                case WallpaperSetterSettings.PicturePositions.Fit:
                    wallpaperStyle = "6";
                    tileWallpaper = "0";
                    break;
                case WallpaperSetterSettings.PicturePositions.Stretch:
                    wallpaperStyle = "2";
                    tileWallpaper = "0";
                    break;
                case WallpaperSetterSettings.PicturePositions.Tile:
                    wallpaperStyle = "0";
                    tileWallpaper = "1";
                    break;
                case WallpaperSetterSettings.PicturePositions.Center:
                    wallpaperStyle = "0";
                    tileWallpaper = "0";
                    break;
                default:
                    break;
            }


            key.SetValue(@"WallpaperStyle", wallpaperStyle);
            key.SetValue(@"TileWallpaper", tileWallpaper);
        }

        public String GetWallpaper()
        {
            var wallpaper = new String('\0', WinAPI.MAX_PATH);
            WinAPI.SystemParametersInfo(WinAPI.SPI_GETDESKWALLPAPER, (UInt32)wallpaper.Length, wallpaper, 0);
            wallpaper = wallpaper.Substring(0, wallpaper.IndexOf('\0'));
            return wallpaper;
        }

        public void Initialize(object args) { }
        public void Activate(object args) { }
        public void Deactivate(object args) { }

    }
}
