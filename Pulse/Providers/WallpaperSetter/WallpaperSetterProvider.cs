using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pulse.Base;
using Pulse.Base.Providers;
using Microsoft.Win32;
using Pulse.Base.WinAPI;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;

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
            //SetWallpaperType(wss.Position);

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
                    WinAPI.SetSysColors(1, aiElements, new WinAPI.COLORREF(PictureManager.CalcAverageColor(bmp)));
                }
            }

            Desktop.SetWallpaperUsingActiveDesktop(p.LocalPath);
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

        
        public void Initialize(object args) { }
        public void Activate(object args) { }
        public void Deactivate(object args) { }
    }
}
