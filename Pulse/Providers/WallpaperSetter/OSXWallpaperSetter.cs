using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pulse.Base;
using System.Diagnostics;

namespace WallpaperSetter
{
    [System.ComponentModel.Description("Desktop Wallpaper")]
    [ProviderPlatform(PlatformID.MacOSX, 0, 0)]
    [ProviderPlatform(PlatformID.Unix, 0, 0)]
    public class OSXWallpaperSetter : IOutputProvider
    {
        public void ProcessPicture(Picture p, string config)
        {

            string applScript =
@"set theUnixPath to POSIX file ""{0}"" as text 
tell application ""Finder"" 
set desktop picture to {{theUnixPath}} as alias 
end tell";

            MonoDevelop.MacInterop.AppleScript.Run(string.Format(applScript, p.LocalPath));
        }

        public void Activate(object args) { }
        public void Deactivate(object args) { }
        public void Initialize(object args) { }

        public void ProcessPictures(PictureList pl, string config)
        {
            if (pl.Pictures.Any()) ProcessPicture(pl.Pictures.First(), config);
        }

        public OutputProviderMode Mode { get { return OutputProviderMode.Single; } }
    }
}