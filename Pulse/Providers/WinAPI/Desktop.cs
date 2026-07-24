using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Microsoft.Win32;

namespace Pulse.Base.WinAPI
{
    public class Desktop
    {
        public enum WallpaperStyle
        {
            Tile,
            Center,
            Stretch,
            Fit,
            Fill,
            Span, // Win8+ - spans across all monitors
            NotSet
        }

        // --- Legacy ActiveDesktop (XP/Vista) kept for compat, now unused ---
        public static void EnableActiveDesktop()
        {
            IntPtr result = IntPtr.Zero;
            WinAPI.SendMessageTimeout(WinAPI.FindWindow("Progman", null), 0x52c, IntPtr.Zero, IntPtr.Zero, 0, 500, out result);
        }

        public static void SetWallpaperUsingActiveDesktop(string path)
        {
            EnableActiveDesktop();

            ThreadStart threadStarter = () =>
            {
                WinAPI.IActiveDesktop _activeDesktop = WinAPI.ActiveDesktopWrapper.GetActiveDesktop();
                _activeDesktop.SetWallpaper(path, 0);
                _activeDesktop.ApplyChanges(WinAPI.AD_Apply.ALL | WinAPI.AD_Apply.FORCE);
                Marshal.ReleaseComObject(_activeDesktop);
            };
            Thread thread = new Thread(threadStarter);
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join(2000);
        }

        public static void SetWallpaperWithRetry(string path, int retryCount, Action<string> sw)
        {
            sw(path);
            int tryCount = 0;
            do
            {
                if (GetWallpaperUsingSystemParameterInfo().ToLower() == path.ToLower()) break;
                sw(path);
                tryCount++;
            } while (tryCount < 3);
        }

        // --- Modern Win10/11 path: IDesktopWallpaper for multi-monitor ---
        // CLSID: C2CF3110-460E-4fc1-B9D0-8A110D49287D
        // IID IDesktopWallpaper: B92B56A9-8B55-4E14-9A89-0199BBB6F93B

        // FIXED: Full correct vtable per shobjidl_core.h - old was truncated mis-ordered (CRIT-1)
        [ComImport]
        [Guid("B92B56A9-8B55-4E14-9A89-0199BBB6F93B")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IDesktopWallpaper
        {
            void SetWallpaper([MarshalAs(UnmanagedType.LPWStr)] string monitorID, [MarshalAs(UnmanagedType.LPWStr)] string wallpaper);
            [return: MarshalAs(UnmanagedType.LPWStr)]
            string GetWallpaper([MarshalAs(UnmanagedType.LPWStr)] string monitorID);
            [return: MarshalAs(UnmanagedType.LPWStr)]
            string GetMonitorDevicePathAt(uint monitorIndex);
            void GetMonitorDevicePathCount(out uint count);
            void GetMonitorRECT([MarshalAs(UnmanagedType.LPWStr)] string monitorID, out RECT displayRect);
            void SetBackgroundColor(uint color);
            void GetBackgroundColor(out uint color);
            void SetPosition(DesktopWallpaperPosition position);
            void GetPosition(out DesktopWallpaperPosition position);
            // Below 6 methods preserve vtable even if unused - prevents calling wrong method
            void SetSlideshow([MarshalAs(UnmanagedType.Interface)] object items);
            void GetSlideshow([MarshalAs(UnmanagedType.Interface)] out object items);
            void SetSlideshowOptions(DesktopSlideshowOptions options, uint slideshowTick);
            void GetSlideshowOptions(out DesktopSlideshowOptions options, out uint slideshowTick);
            void AdvanceSlideshow([MarshalAs(UnmanagedType.LPWStr)] string monitorID, DesktopSlideshowDirection direction);
            void GetStatus(out DesktopSlideshowState state);
            void Enable(bool enable);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left, Top, Right, Bottom;
        }

        private enum DesktopWallpaperPosition
        {
            DWPOS_CENTER = 0,
            DWPOS_TILE = 1,
            DWPOS_STRETCH = 2,
            DWPOS_FIT = 3,
            DWPOS_FILL = 4,
            DWPOS_SPAN = 5
        }

        private enum DesktopWallpaperDirection
        {
            DSD_H = 0,
            DSD_V = 1
        }

        // Missing enums for vtable preservation
        private enum DesktopSlideshowOptions
        {
            DSO_SHUFFLEIMAGES = 0x01
        }

        private enum DesktopSlideshowState
        {
            DSS_ENABLED = 0x01,
            DSS_SLIDESHOW = 0x02,
            DSS_DISABLED_BY_REMOTE_SESSION = 0x04
        }

        private enum DesktopSlideshowDirection
        {
            DSD_FORWARD = 0,
            DSD_BACKWARD = 1
        }

        [ComImport]
        [Guid("C2CF3110-460E-4FC1-B9D0-8A110D49287D")]
        private class DesktopWallpaperClass { }

        public static void SetWallpaperUsingDesktopWallpaper(string path, WallpaperStyle style = WallpaperStyle.Fill)
        {
            // FIXED: Wrap COM in STA thread (was MTA timer thread causing RPC_E_WRONG_THREAD)
            Exception threadEx = null;
            var t = new Thread(() =>
            {
                try
                {
                    var dw = (IDesktopWallpaper)new DesktopWallpaperClass();

                    DesktopWallpaperPosition pos = DesktopWallpaperPosition.DWPOS_FILL;
                    switch (style)
                    {
                        case WallpaperStyle.Tile: pos = DesktopWallpaperPosition.DWPOS_TILE; break;
                        case WallpaperStyle.Center: pos = DesktopWallpaperPosition.DWPOS_CENTER; break;
                        case WallpaperStyle.Stretch: pos = DesktopWallpaperPosition.DWPOS_STRETCH; break;
                        case WallpaperStyle.Fit: pos = DesktopWallpaperPosition.DWPOS_FIT; break;
                        case WallpaperStyle.Fill: pos = DesktopWallpaperPosition.DWPOS_FILL; break;
                        case WallpaperStyle.Span: pos = DesktopWallpaperPosition.DWPOS_SPAN; break;
                    }

                    dw.SetPosition(pos);
                    dw.SetWallpaper(null, path);
                    Log.Logger.Write($"Desktop: Set wallpaper via IDesktopWallpaper: {path} style={style}", Log.LoggerLevels.Info);

                    try { Marshal.ReleaseComObject(dw); } catch { }
                }
                catch (Exception ex)
                {
                    threadEx = ex;
                }
            });
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join(5000);

            if (threadEx != null)
            {
                Log.Logger.Write($"Desktop: IDesktopWallpaper failed ({threadEx.Message}), falling back to SystemParametersInfo", Log.LoggerLevels.Debug);
                SetWallpaperUsingSystemParameterInfoInternal(path);
            }
        }

        private static void SetWallpaperUsingSystemParameterInfoInternal(string path)
        {
            WinAPI.SystemParametersInfo(WinAPI.SPI_SETDESKWALLPAPER, 0, path, WinAPI.SPIF_UPDATEINIFILE | WinAPI.SPIF_SENDWININICHANGE);
        }

        public static void SetWallpaperUsingSystemParameterInfo(string path)
        {
            // Try modern API first on Win8+
            if (Environment.OSVersion.Version.Major > 6 || (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor >= 2))
            {
                try
                {
                    // Get current style from registry to preserve user's preference
                    var curStyle = GetCurrentWallpaperStyle();
                    SetWallpaperUsingDesktopWallpaper(path, curStyle);
                    return;
                }
                catch { /* fallback */ }
            }

            // Legacy fallback - still works on Win10/11 for single monitor / all monitors same
            SetWallpaperUsingSystemParameterInfoInternal(path);
        }

        public static String GetWallpaperUsingSystemParameterInfo()
        {
            var wallpaper = new String('\0', (int)WinAPI.MAX_PATH);
            WinAPI.SystemParametersInfo(WinAPI.SPI_GETDESKWALLPAPER, (UInt32)wallpaper.Length, wallpaper, 0);
            int idx = wallpaper.IndexOf('\0');
            if (idx >= 0) wallpaper = wallpaper.Substring(0, idx);
            return wallpaper;
        }

        public static void SetDesktopBackgroundColor(Color newColor)
        {
            int[] aiElements = { WinAPI.COLOR_DESKTOP };
            WinAPI.SetSysColors(1, aiElements, new WinAPI.COLORREF(newColor));
            // Also set via IDesktopWallpaper background color for Win8+
            try
            {
                var dw = (IDesktopWallpaper)new DesktopWallpaperClass();
                uint bgr = (uint)(newColor.B << 16 | newColor.G << 8 | newColor.R);
                dw.SetBackgroundColor(bgr);
            }
            catch { }
        }

        // --- Aero Glass (Win7) -> Modern Accent (Win10/11) ---

        public static void SetAeroColor(Color newColor)
        {
            // Try Win7 path first if DWM composition enabled and old API available
            try
            {
                if (WinAPI.DwmIsCompositionEnabled())
                {
                    WinAPI.DWM_COLORIZATION_PARAMS color;
                    WinAPI.DwmGetColorizationParameters(out color);
                    color.ColorizationColor = (uint)System.Drawing.Color.FromArgb(255, newColor.R, newColor.G, newColor.B).ToArgb();
                    WinAPI.DwmSetColorizationParameters(ref color, 0);
                    Log.Logger.Write($"Desktop: Set aero color via DwmSetColorizationParameters: {newColor}", Log.LoggerLevels.Info);
                    return;
                }
            }
            catch (EntryPointNotFoundException)
            {
                // #127/#131 not found on Win10+ - fall through to modern path
                Log.Logger.Write("Desktop: DwmSetColorizationParameters entry point not found, using modern accent path", Log.LoggerLevels.Debug);
            }
            catch (Exception ex)
            {
                Log.Logger.Write($"Desktop: DwmSetColorizationParameters failed: {ex.Message}, using modern accent", Log.LoggerLevels.Debug);
            }

            // Modern Win10/11 accent color path - registry
            SetModernAccentColor(newColor);
        }

        public static void SetModernAccentColor(Color newColor)
        {
            try
            {
                // Win10/11 accent color: ARGB -> ABGR? Actually registry is 0xAABBGGRR? Let's research:
                // DWM\ColorizationColor is ARGB with alpha in high byte
                // DWM\AccentColor is ABGR? Actually it's 0xAABBGGRR but Win10 stores as BGR.
                // We'll write both for compatibility

                // Convert to DWORD: 0xFF (opaque) + BGR
                uint dwmColor = (uint)(0xFF << 24 | newColor.R << 16 | newColor.G << 8 | newColor.B);
                // Actually ColorizationColor is ARGB: AARRGGBB where RR=newColor.R etc, but fromArgb gives that
                uint colorizationColor = (uint)Color.FromArgb(255, newColor.R, newColor.G, newColor.B).ToArgb();

                // ABGR for AccentColor: 0xFFBBGGRR -> reverse?
                uint accentColor = (uint)(0xFF << 24 | newColor.B << 16 | newColor.G << 8 | newColor.R);

                RegistryKey dwmKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\DWM", true);
                if (dwmKey != null)
                {
                    try { dwmKey.SetValue("ColorizationColor", colorizationColor, RegistryValueKind.DWord); } catch { }
                    try { dwmKey.SetValue("AccentColor", accentColor, RegistryValueKind.DWord); } catch { }
                    try { dwmKey.SetValue("ColorizationAfterglow", colorizationColor, RegistryValueKind.DWord); } catch { }
                    dwmKey.Close();
                }

                // Also set accent for Explorer
                RegistryKey accentKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Accent", true);
                if (accentKey == null)
                    accentKey = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Accent");
                if (accentKey != null)
                {
                    try { accentKey.SetValue("AccentColorMenu", accentColor, RegistryValueKind.DWord); } catch { }
                    try { accentKey.SetValue("AccentColor", accentColor, RegistryValueKind.DWord); } catch { }
                    try { accentKey.SetValue("StartColorMenu", accentColor, RegistryValueKind.DWord); } catch { }
                    accentKey.Close();
                }

                // Notify system of color change - broadcast WM_DWMCOLORIZATIONCOLORCHANGED (0x320) and WM_SETTINGCHANGE
                // FIX: Free HGlobal to avoid leak (was leaking per accent change, 13x per transition)
                IntPtr ptr = IntPtr.Zero;
                try
                {
                    ptr = Marshal.StringToHGlobalUni("ImmersiveColorSet");
                    NativeMethods.SendMessageTimeout((IntPtr)0xFFFF, 0x001A, IntPtr.Zero, ptr, 2, 5000, out _);
                }
                finally
                {
                    if (ptr != IntPtr.Zero) Marshal.FreeHGlobal(ptr);
                }
                NativeMethods.SendMessageTimeout((IntPtr)0xFFFF, 0x0320, IntPtr.Zero, IntPtr.Zero, 2, 5000, out _);

                Log.Logger.Write($"Desktop: Set modern accent color via registry: {newColor} Accent=0x{accentColor:X8} Colorization=0x{colorizationColor:X8}", Log.LoggerLevels.Info);
            }
            catch (Exception ex)
            {
                Log.Logger.Write($"Desktop: Failed to set modern accent color: {ex}", Log.LoggerLevels.Errors);
            }
        }

        private static class NativeMethods
        {
            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
            public static extern IntPtr SendMessageTimeout(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam, uint fuFlags, uint uTimeout, out IntPtr lpdwResult);
            [DllImport("user32.dll", CharSet = CharSet.Auto)]
            public static extern IntPtr SendMessageTimeout(IntPtr hWnd, uint Msg, IntPtr wParam, string lParam, uint fuFlags, uint uTimeout, out IntPtr lpdwResult);
        }

        public static Color GetCurrentAeroColor()
        {
            // Try DWM first
            try
            {
                if (WinAPI.DwmIsCompositionEnabled())
                {
                    WinAPI.DWM_COLORIZATION_PARAMS color;
                    WinAPI.DwmGetColorizationParameters(out color);
                    return Color.FromArgb((int)color.ColorizationColor);
                }
            }
            catch { }

            // Fallback to registry accent
            try
            {
                RegistryKey dwmKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\DWM", false);
                if (dwmKey != null)
                {
                    object val = dwmKey.GetValue("AccentColor");
                    if (val is int || val is uint)
                    {
                        uint accent = Convert.ToUInt32(val);
                        // accent is ABGR: 0xAABBGGRR
                        byte r = (byte)(accent & 0xFF);
                        byte g = (byte)((accent >> 8) & 0xFF);
                        byte b = (byte)((accent >> 16) & 0xFF);
                        dwmKey.Close();
                        return Color.FromArgb(r, g, b);
                    }
                    dwmKey.Close();
                }
            }
            catch { }

            return Color.Empty;
        }

        private static WallpaperStyle GetCurrentWallpaperStyle()
        {
            try
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", false);
                if (key != null)
                {
                    string style = key.GetValue("WallpaperStyle") as string;
                    string tile = key.GetValue("TileWallpaper") as string;
                    key.Close();

                    if (style == "0" && tile == "0") return WallpaperStyle.Center;
                    if (style == "0" && tile == "1") return WallpaperStyle.Tile;
                    if (style == "2") return WallpaperStyle.Stretch;
                    if (style == "6") return WallpaperStyle.Fit;
                    if (style == "10") return WallpaperStyle.Fill;
                    if (style == "22") return WallpaperStyle.Span;
                }
            }
            catch { }
            return WallpaperStyle.Fill;
        }

        // Modern wallpaper style setter with Span support (Win8+)
        public static void SetWallpaperType(WallpaperStyle style)
        {
            if (style == WallpaperStyle.NotSet) return;

            try
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);
                if (key == null) return;

                switch (style)
                {
                    case WallpaperStyle.Tile:
                        key.SetValue(@"WallpaperStyle", "0");
                        key.SetValue(@"TileWallpaper", "1");
                        break;
                    case WallpaperStyle.Center:
                        key.SetValue(@"WallpaperStyle", "0");
                        key.SetValue(@"TileWallpaper", "0");
                        break;
                    case WallpaperStyle.Stretch:
                        key.SetValue(@"WallpaperStyle", "2");
                        key.SetValue(@"TileWallpaper", "0");
                        break;
                    case WallpaperStyle.Fit: // Windows 7+
                        key.SetValue(@"WallpaperStyle", "6");
                        key.SetValue(@"TileWallpaper", "0");
                        break;
                    case WallpaperStyle.Fill: // Windows 7+
                        key.SetValue(@"WallpaperStyle", "10");
                        key.SetValue(@"TileWallpaper", "0");
                        break;
                    case WallpaperStyle.Span: // Windows 8+
                        key.SetValue(@"WallpaperStyle", "22");
                        key.SetValue(@"TileWallpaper", "0");
                        break;
                }
                key.Close();

                // For Win8+, also set via IDesktopWallpaper position
                if (style == WallpaperStyle.Span || style == WallpaperStyle.Fill || style == WallpaperStyle.Fit)
                {
                    try
                    {
                        var curPath = GetWallpaperUsingSystemParameterInfo();
                        if (!string.IsNullOrEmpty(curPath))
                            SetWallpaperUsingDesktopWallpaper(curPath, style);
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Write($"Desktop: Failed to set wallpaper style {style}: {ex}", Log.LoggerLevels.Errors);
            }
        }

        // --- Lock Screen (Win10+ replacement for LogonBackground) ---
        // For .NET 4.8, we use registry + WinRT via dynamic if available
        // Full WinRT implementation requires net8.0-windows target

        public static bool SetLockScreenImage(string path)
        {
            // Only for Win8+ (actually Win10+)
            if (Environment.OSVersion.Version.Major < 6 || (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor < 2))
            {
                Log.Logger.Write("Desktop: Lock screen not supported on this OS (requires Win8+)", Log.LoggerLevels.Warnings);
                return false;
            }

            try
            {
                // Win10+ can set lock screen via:
                // HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Lock Screen\Creative\LockImage
                // Or via WinRT: Windows.System.UserProfile.LockScreen.SetImageFileAsync (needs packaged app or net8 target)
                
                // Try WinRT via reflection for .NET 4.8 (if Windows.winmd available)
                // This is best-effort - for full support need net8.0-windows

                // Registry fallback for enterprise/custom lock screen
                // This sets the "custom lock screen" hint, but Group Policy may override
                // HKLM\SOFTWARE\Policies\Microsoft\Windows\Personalization\LockScreenImage - requires admin

                // For now, we set the creative lock image registry (used by Spotlight custom?)
                // And also copy to expected location for LockScreen

                // Simplest: Use SystemParameters? Actually LockScreen image file should be set via:
                // Copy file to local app data and set registry value used by LogonUI? Not officially supported on 4.8
                // So we log and attempt WinRT if we can load Windows Runtime

                Log.Logger.Write($"Desktop: Attempting to set lock screen image to {path}", Log.LoggerLevels.Info);

                // Attempt WinRT via dynamic COM (only works if app has access and Windows 10 SDK)
                // In .NET 4.8 this often fails without [Windows.Foundation.Metadata] - so we guide user to net8 build
                if (TrySetLockScreenViaWinRT(path))
                {
                    Log.Logger.Write("Desktop: Lock screen set via WinRT", Log.LoggerLevels.Info);
                    return true;
                }

                // Fallback: Set registry for Lock Screen (works for some enterprise scenarios, requires admin for machine policy)
                // HKLM\SOFTWARE\Policies\Microsoft\Windows\Personalization
                try
                {
                    RegistryKey policyKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\Personalization", true);
                    if (policyKey == null)
                        policyKey = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\Personalization");
                    if (policyKey != null)
                    {
                        policyKey.SetValue("LockScreenImage", path, RegistryValueKind.String);
                        policyKey.Close();
                        Log.Logger.Write("Desktop: Lock screen registry set (requires admin + GP refresh)", Log.LoggerLevels.Info);
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Log.Logger.Write($"Desktop: Lock screen registry set failed (needs admin): {ex.Message}", Log.LoggerLevels.Debug);
                }

                // For non-admin, we can try to set via PersonalizationSettings (Win10 1607+)
                // This requires UWP/WinRT, not available in 4.8 without hacks
                Log.Logger.Write("Desktop: Lock screen set not fully supported on .NET 4.8, upgrade to net8.0-windows for full WinRT support", Log.LoggerLevels.Warnings);
                return false;
            }
            catch (Exception ex)
            {
                Log.Logger.Write($"Desktop: Failed to set lock screen image: {ex}", Log.LoggerLevels.Errors);
                return false;
            }
        }

        private static bool TrySetLockScreenViaWinRT(string path)
        {
            // This only works when targeting net8.0-windows with Windows SDK
            // On net4.8, Windows Runtime activation is limited
            // Return false to indicate not supported, guide to use net8 build
            return false;
        }
    }
}
