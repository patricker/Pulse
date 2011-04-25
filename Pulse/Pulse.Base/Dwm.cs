﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Pulse.Base
{
    public class Dwm
    {
        public static bool IsGlassAvailable()
        {
            return (System.Environment.OSVersion.Version.Major >= 6 && System.Environment.OSVersion.Version.Build >= 5600) && File.Exists(System.Environment.SystemDirectory + @"\dwmapi.dll");
        }

        public static bool IsGlassEnabled()
        {
            bool result;
            WinAPI.DwmIsCompositionEnabled(out result);
            return result;
        }

        public static void RemoveFromAeroPeek(IntPtr hwnd)
        {
            if (IsGlassAvailable() && System.Environment.OSVersion.Version.Major == 6 &&
                System.Environment.OSVersion.Version.Minor == 1)
            {
                var attrValue = 1; // True
                WinAPI.DwmSetWindowAttribute(hwnd, 12, ref attrValue, sizeof(int));
            }
        }

        public static void ExtendGlassFrame(IntPtr hwnd, ref WinAPI.Margins margins)
        {
            if (IsGlassAvailable())
                WinAPI.DwmExtendFrameIntoClientArea(hwnd, ref margins);
        }

        public static void MakeGlassRegion(ref IntPtr handle, IntPtr rgn)
        {
            if (IsGlassAvailable() && rgn != IntPtr.Zero)
            {
                var bb = new WinAPI.BbStruct
                {
                    Enable = true,
                    Flags = WinAPI.BbFlags.DwmBbEnable | WinAPI.BbFlags.DwmBbBlurregion,
                    Region = rgn
                };
                WinAPI.DwmEnableBlurBehindWindow(handle, ref bb);
            }
        }

        public static void RemoveGlassRegion(ref IntPtr handle)
        {
            if (IsGlassAvailable())
            {
                var bb = new WinAPI.BbStruct
                {
                    Enable = false,
                    Flags = WinAPI.BbFlags.DwmBbEnable | WinAPI.BbFlags.DwmBbBlurregion,
                    Region = IntPtr.Zero
                };
                WinAPI.DwmEnableBlurBehindWindow(handle, ref bb);
            }
        }
    }
}
