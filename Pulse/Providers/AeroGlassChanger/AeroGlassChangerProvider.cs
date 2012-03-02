using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Threading;

namespace AeroGlassChanger
{
    //Most of this code came from http://aura.codeplex.com/ (Aura project)
    [System.ComponentModel.Description("Aero Glass Color Sync")]
    public class AeroGlassChangerProvider : Pulse.Base.IOutputProvider
    {
        public void ProcessPicture(Pulse.Base.Picture p)
        {
            ManualResetEvent mre = new ManualResetEvent(false);

            //get color to start with
            Color currentAero = GetCurrentAeroColor();
            //get final color
            Color endAeroColor = CalcAverageColor((Bitmap)Bitmap.FromFile(p.LocalPath));

            //build transition
            Color[] transitionColors = CalcColorTransition(currentAero, endAeroColor, 7);

            //build timer
            System.Timers.Timer t = new System.Timers.Timer(100);

            int currentStep = 0;

            t.Elapsed += delegate(object sender, System.Timers.ElapsedEventArgs e)
            {
                //set to next color
                SetDwmColor(transitionColors[currentStep]);

                //increment steps and check if we should stop the timer
                currentStep++;
                if (currentStep == 7) { mre.Set(); t.Stop(); }
            };

            t.Start();

            mre.WaitOne();
        }

        public static Color CalcAverageColor(System.Drawing.Bitmap image)
        {
            var bmp = new System.Drawing.Bitmap(1, 1);
            var orig = image;
            using (var g = System.Drawing.Graphics.FromImage(bmp))
            {
                // the Interpolation mode needs to be set to 
                // HighQualityBilinear or HighQualityBicubic or this method
                // doesn't work at all.  With either setting, the results are
                // slightly different from the averaging method.
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(orig, new System.Drawing.Rectangle(0, 0, 1, 1));
            }
            var pixel = bmp.GetPixel(0, 0);
            orig.Dispose();
            bmp.Dispose();
            // pixel will contain average values for entire orig Bitmap
            return Color.FromArgb(pixel.R, pixel.G, pixel.B);
        }

        public static Color[] CalcColorTransition(Color from, Color to, int steps)
        {
            Bitmap img = new Bitmap(1, steps);
            Rectangle rect = new Rectangle(0,0,1,steps);

            LinearGradientBrush myLinearGradientBrush = new LinearGradientBrush(
               rect,
               from,
               to,
               LinearGradientMode.Vertical);

            var g = Graphics.FromImage(img);

            g.FillRectangle(myLinearGradientBrush, rect);

            Color[] colors = new Color[steps];

            for (int p = 0; p < steps; p++)
            {
                colors[p] = img.GetPixel(0, p);
            }

            return colors;

        }

        public static void SetDwmColor(System.Drawing.Color newColor)
        {
            if (DwmIsCompositionEnabled())
            {
                DWM_COLORIZATION_PARAMS color;
                //get the current color
                DwmGetColorizationParameters(out color);
                //set new color to transition too
                color.ColorizationColor = (uint)System.Drawing.Color.FromArgb(255, newColor.R, newColor.G, newColor.B).ToArgb();
                //transition
                DwmSetColorizationParameters(ref color, 0);
            }
        }

        public static Color GetCurrentAeroColor()
        {
            if (DwmIsCompositionEnabled())
            {
                DWM_COLORIZATION_PARAMS color;
                //get the current color
                DwmGetColorizationParameters(out color);

                Color c = Color.FromArgb((int)color.ColorizationColor);

                return c;
            }

            return Color.Empty;
        }

        #region WinAPI

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


        public void Activate(object args)
        {
            
        }

        public void Deactivate(object args)
        {
            
        }

        public void Initialize()
        {
            
        }
    }
}
