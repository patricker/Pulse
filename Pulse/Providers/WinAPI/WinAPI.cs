using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Pulse.Base.WinAPI
{
    public class WinAPI
    {
        #region Wallpaper Setting
        public const UInt32 SPIF_UPDATEINIFILE = 0x1;
        public const int SPIF_SENDWININICHANGE = 0x02;
        public const UInt32 SPI_SETDESKWALLPAPER = 20;
        public static readonly UInt32 SPI_GETDESKWALLPAPER = 0x73;
        public static readonly int MAX_PATH = 260;
        public const int COLOR_DESKTOP = 1;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern Int32 SystemParametersInfo(UInt32 uiAction, UInt32 uiParam, String pvParam, UInt32 fWinIni);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool PostMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam);

        [DllImport("user32.dll")]
        public static extern bool SetSysColors(int cElements, int[] lpaElements, COLORREF lpaRgbValues);

        //struct came from http://www.pinvoke.net/default.aspx/Structures/COLORREF.html
        [StructLayout(LayoutKind.Sequential)]
        public struct COLORREF
        {
            public uint ColorDWORD;

            public COLORREF(uint color)
            {
                ColorDWORD = color;
            }

            public COLORREF(System.Drawing.Color color)
            {
                ColorDWORD = (uint)color.R + (((uint)color.G) << 8) + (((uint)color.B) << 16);
            }

            public System.Drawing.Color GetColor()
            {
                return System.Drawing.Color.FromArgb((int)(0x000000FFU & ColorDWORD),
               (int)(0x0000FF00U & ColorDWORD) >> 8, (int)(0x00FF0000U & ColorDWORD) >> 16);
            }

            public void SetColor(System.Drawing.Color color)
            {
                ColorDWORD = (uint)color.R + (((uint)color.G) << 8) + (((uint)color.B) << 16);
            }
        }
        #endregion

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool Wow64DisableWow64FsRedirection(ref IntPtr ptr);

        #region AeroGlassItems

        public struct DWM_COLORIZATION_PARAMS
        {
            public UInt32 ColorizationColor;
            public UInt32 ColorizationAfterglow;
            public UInt32 ColorizationColorBalance;
            public UInt32 ColorizationAfterglowBalance;
            public UInt32 ColorizationBlurBalance;
            public UInt32 ColorizationGlassReflectionIntensity;
            public UInt32 ColorizationOpaqueBlend;
        }


        [DllImport("dwmapi.dll", EntryPoint = "#127", PreserveSig = false)]
        public static extern void DwmGetColorizationParameters(out DWM_COLORIZATION_PARAMS parameters);

        [DllImport("dwmapi.dll", PreserveSig = false)]
        public static extern bool DwmIsCompositionEnabled();

        [DllImport("dwmapi.dll", EntryPoint = "#131", PreserveSig = false)]
        public static extern void DwmSetColorizationParameters(ref DWM_COLORIZATION_PARAMS parameters, long uUnknown);

        #endregion
    }
}
