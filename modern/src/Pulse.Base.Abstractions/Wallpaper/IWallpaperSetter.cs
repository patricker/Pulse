using System;
using System.Threading.Tasks;

namespace Pulse.Base.Wallpaper
{
    public enum WallpaperStyle
    {
        Tile,
        Center,
        Stretch,
        Fit,
        Fill,
        Span
    }

    public interface IWallpaperSetter
    {
        bool IsSupported { get; }
        Task<bool> SetWallpaperAsync(string imagePath, WallpaperStyle style = WallpaperStyle.Fill);
        Task<string?> GetWallpaperAsync();
    }

    public static class WallpaperSetterFactory
    {
        public static IWallpaperSetter Create()
        {
            if (OperatingSystem.IsWindows())
                return new WindowsWallpaperSetter();
            else if (OperatingSystem.IsMacOS())
                return new MacWallpaperSetter();
            else if (OperatingSystem.IsLinux())
                return new LinuxWallpaperSetter();
            else
                return new NullWallpaperSetter();
        }
    }

    // Fallback that does nothing
    public class NullWallpaperSetter : IWallpaperSetter
    {
        public bool IsSupported => false;
        public Task<bool> SetWallpaperAsync(string imagePath, WallpaperStyle style = WallpaperStyle.Fill) => Task.FromResult(false);
        public Task<string?> GetWallpaperAsync() => Task.FromResult<string?>(null);
    }

    // Windows implementation using IDesktopWallpaper + SystemParametersInfo
    public class WindowsWallpaperSetter : IWallpaperSetter
    {
        public bool IsSupported => OperatingSystem.IsWindows();

        public async Task<bool> SetWallpaperAsync(string imagePath, WallpaperStyle style = WallpaperStyle.Fill)
        {
            return await Task.Run(() =>
            {
                try
                {
                    // Prefer IDesktopWallpaper for Win8+ (multi-monitor, Span)
                    // This is the same logic as modern Desktop.cs but simplified for cross-platform abstraction
                    if (Environment.OSVersion.Version.Major >= 10 || (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor >= 2))
                    {
                        // Try IDesktopWallpaper via dynamic to avoid compile-time Windows dependency in net8.0
                        try
                        {
                            var desktopType = Type.GetTypeFromCLSID(new Guid("C2CF3110-460E-4FC1-B9D0-8A110D49287D"));
                            dynamic? dw = Activator.CreateInstance(desktopType!);
                            if (dw != null)
                            {
                                // Map style
                                int pos = style switch
                                {
                                    WallpaperStyle.Tile => 1,
                                    WallpaperStyle.Center => 0,
                                    WallpaperStyle.Stretch => 2,
                                    WallpaperStyle.Fit => 3,
                                    WallpaperStyle.Fill => 4,
                                    WallpaperStyle.Span => 5,
                                    _ => 4
                                };
                                dw.SetPosition(pos);
                                dw.SetWallpaper(null, imagePath);
                                return true;
                            }
                        }
                        catch { /* fallback */ }
                    }

                    // Fallback to SystemParametersInfo SPI_SETDESKWALLPAPER = 20
                    return NativeMethods.SystemParametersInfo(20, 0, imagePath, 0x01 | 0x02);
                }
                catch { return false; }
            });
        }

        public async Task<string?> GetWallpaperAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    var sb = new System.Text.StringBuilder(260);
                    NativeMethods.SystemParametersInfo(0x73, (uint)sb.Capacity, sb, 0);
                    string s = sb.ToString();
                    int idx = s.IndexOf('\0');
                    if (idx >= 0) s = s.Substring(0, idx);
                    return s;
                }
                catch { return null; }
            });
        }

        private static class NativeMethods
        {
            [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
            public static extern bool SystemParametersInfo(uint uiAction, uint uiParam, string pvParam, uint fWinIni);
        }
    }

    public class MacWallpaperSetter : IWallpaperSetter
    {
        public bool IsSupported => OperatingSystem.IsMacOS();

        public async Task<bool> SetWallpaperAsync(string imagePath, WallpaperStyle style = WallpaperStyle.Fill)
        {
            return await Task.Run(() =>
            {
                try
                {
                    // macOS: osascript tell Finder to set desktop picture
                    var psi = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "osascript",
                        Arguments = $"-e 'tell application \"Finder\" to set desktop picture to POSIX file \"{imagePath}\"'",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false
                    };
                    using var proc = System.Diagnostics.Process.Start(psi);
                    proc?.WaitForExit(5000);
                    return proc?.ExitCode == 0;
                }
                catch { return false; }
            });
        }

        public Task<string?> GetWallpaperAsync() => Task.FromResult<string?>(null);
    }

    public class LinuxWallpaperSetter : IWallpaperSetter
    {
        public bool IsSupported => OperatingSystem.IsLinux();

        public async Task<bool> SetWallpaperAsync(string imagePath, WallpaperStyle style = WallpaperStyle.Fill)
        {
            return await Task.Run(() =>
            {
                try
                {
                    // Try GNOME
                    var psi = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "gsettings",
                        Arguments = $"set org.gnome.desktop.background picture-uri \"file://{imagePath}\"",
                        RedirectStandardOutput = true,
                        UseShellExecute = false
                    };
                    using (var p = System.Diagnostics.Process.Start(psi))
                    {
                        p?.WaitForExit(2000);
                        if (p?.ExitCode == 0) return true;
                    }

                    // Try cinnamon
                    psi.Arguments = $"set org.cinnamon.desktop.background picture-uri \"file://{imagePath}\"";
                    using (var p = System.Diagnostics.Process.Start(psi))
                    {
                        p?.WaitForExit(2000);
                        if (p?.ExitCode == 0) return true;
                    }

                    // Try feh for i3wm
                    psi.FileName = "feh";
                    psi.Arguments = $"--bg-fill \"{imagePath}\"";
                    using (var p = System.Diagnostics.Process.Start(psi))
                    {
                        p?.WaitForExit(2000);
                        return p?.ExitCode == 0;
                    }
                }
                catch { return false; }
            });
        }

        public Task<string?> GetWallpaperAsync() => Task.FromResult<string?>(null);
    }
}
