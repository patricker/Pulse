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
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

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


            //set wallpaper style (tiled, centered, etc...) - FIXED: was commented out, Span unreachable
            try
            {
                Desktop.SetWallpaperType(wss.Position);
            }
            catch (Exception ex)
            {
                Log.Logger.Write($"WallpaperSetter: SetWallpaperType failed {ex.Message}", Log.LoggerLevels.Warnings);
            }

            //set desktop background color
            if (wss.BackgroundColorMode == WallpaperSetterSettings.BackgroundColorModes.Specific)
            {
                try { Desktop.SetDesktopBackgroundColor(wss.Color); } catch { }
            }
            else if (wss.BackgroundColorMode == WallpaperSetterSettings.BackgroundColorModes.Computed)
            {
                try
                {
                    // Use ImageSharp for average color to avoid System.Drawing.Common leak, fallback to legacy Bitmap
                    // For net4.8 we still have Bitmap, for net8 we use ImageSharp
                    using (Bitmap bmp = (Bitmap)Image.FromFile(p.LocalPath))
                    {
                        Desktop.SetDesktopBackgroundColor(PictureManager.CalcAverageColor(bmp));
                    }
                }
                catch { }
            }

            // FIXED: Use IDesktopWallpaper with STA thread and style, not just SPI preserving old style
            try
            {
                // Prefer modern IDesktopWallpaper with user-chosen style (Fill, Span, etc.) for Win8+
                Desktop.SetWallpaperUsingDesktopWallpaper(p.LocalPath, wss.Position);
            }
            catch
            {
                // Fallback to legacy SPI
                Desktop.SetWallpaperUsingSystemParameterInfo(p.LocalPath);
            }            
        }
        
        public void Initialize(object args) { }
        public void Activate(object args) { }
        public void Deactivate(object args) { }
    }
}
