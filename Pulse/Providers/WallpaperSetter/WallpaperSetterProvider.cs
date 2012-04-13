using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pulse.Base;

namespace WallpaperSetter
{
    [System.ComponentModel.Description("Desktop Wallpaper")]
    [ProviderPlatform(PlatformID.Win32NT, 6, 0)]
    public class WallpaperSetterProvider : IOutputProvider
    {
        public void ProcessPicture(Picture p)
        {
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

        public String GetWallpaper()
        {
            var wallpaper = new String('\0', WinAPI.MAX_PATH);
            WinAPI.SystemParametersInfo(WinAPI.SPI_GETDESKWALLPAPER, (UInt32)wallpaper.Length, wallpaper, 0);
            wallpaper = wallpaper.Substring(0, wallpaper.IndexOf('\0'));
            return wallpaper;
        }

        public void Initialize() { }
        public void Activate(object args) { }
        public void Deactivate(object args) { }
    }
}
