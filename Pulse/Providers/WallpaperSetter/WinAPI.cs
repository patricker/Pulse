using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Pulse.Base
{
    public class WinAPI
    {
        public static UInt32 SPIF_UPDATEINIFILE = 0x1;
        public static UInt32 SPI_SETDESKWALLPAPER = 20;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern Int32 SystemParametersInfo(UInt32 uiAction, UInt32 uiParam, String pvParam, UInt32 fWinIni);

        //[DllImport("dwmapi.dll", EntryPoint = "#127", PreserveSig = false)]
        //public static extern void DwmGetColorizationParameters(out DWM_COLORIZATION_PARAMS parameters);

        //[DllImport("dwmapi.dll", PreserveSig = false)]
        //public static extern bool DwmIsCompositionEnabled();

        //[DllImport("dwmapi.dll", EntryPoint = "#131", PreserveSig = false)]
        //public static extern void DwmSetColorizationParameters(ref DWM_COLORIZATION_PARAMS parameters, long uUnknown);

        //public struct DWM_COLORIZATION_PARAMS
        //{
        //    public long Color1;
        //    public long Color2;
        //    public long Intensity;
        //    public long Unknown1;
        //    public long Unknown2;
        //    public long Unknown3;
        //    public long Opaque;
        //}

        //[StructLayout(LayoutKind.Sequential)]
        //public struct Margins
        //{
        //    public int cxLeftWidth;      // width of left border that retains its size
        //    public int cxRightWidth;     // width of right border that retains its size
        //    public int cyTopHeight;      // height of top border that retains its size
        //    public int cyBottomHeight;   // height of bottom border that retains its size
        //};

        //public struct BbStruct //Blur Behind Structure
        //{
        //    public BbFlags Flags;
        //    public bool Enable;
        //    public IntPtr Region;
        //    public bool TransitionOnMaximized;
        //}

        //[Flags]
        //public enum BbFlags : byte //Blur Behind Flags
        //{
        //    DwmBbEnable = 1,
        //    DwmBbBlurregion = 2,
        //    DwmBbTransitiononmaximized = 4,
        //};

        //[DllImport("DwmApi.dll")]
        //public static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref Margins pMarInset);

        //[DllImport("dwmapi.dll")]
        //public static extern int DwmEnableBlurBehindWindow(IntPtr hWnd, ref BbStruct blurBehind);

        //[DllImport("dwmapi.dll")]
        //public static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        //[DllImport("dwmapi.dll")]
        //public static extern int DwmIsCompositionEnabled(out bool enabled);

        //[DllImport("kernel32.dll")]
        //public static extern bool SetProcessWorkingSetSize(IntPtr handle, int minimumWorkingSetSize, int maximumWorkingSetSize);
    }
}
