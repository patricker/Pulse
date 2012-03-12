using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pulse.Base;

namespace WallpaperSetter
{
    [System.ComponentModel.Description("Desktop Wallpaper")]
    public class WallpaperSetterProvider : IOutputProvider
    {
        public void Initialize()
        {

        }

        public void Activate(object args)
        {

        }

        public void ProcessPicture(Picture p)
        {
            //set the wallpaper to the new image
            WinAPI.SystemParametersInfo(WinAPI.SPI_SETDESKWALLPAPER, 0, p.LocalPath, WinAPI.SPIF_UPDATEINIFILE | WinAPI.SPIF_SENDWININICHANGE);
        }       

        public void Deactivate(object args)
        {
            
        }
    }
}
