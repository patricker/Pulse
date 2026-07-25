using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Threading;
using Pulse.Base;
using Pulse.Base.WinAPI;
using System.IO;
using Pulse.Base.Providers;

namespace AeroGlassChanger
{
    //Most of this code came from http://aura.codeplex.com/ (Aura project)
    // Windows 10/11 update: Original DwmSetColorizationParameters (#131) removed after Win8.
    // Now uses modern accent color via registry (HKCU\Software\Microsoft\Windows\DWM\AccentColor)
    // plus fallback to old API on Win7/8.
    [System.ComponentModel.Description("Aero Glass Color Sync (Win7-11, Accent for Win10/11)")]
    [ProviderPlatform(PlatformID.Win32NT, 6, 1)] // windows 7
    [ProviderPlatform(PlatformID.Win32NT, 6, 2)] // windows 8
    [ProviderPlatform(PlatformID.Win32NT, 6, 3)] // windows 8.1 (reports 6.3)
    [ProviderPlatform(PlatformID.Win32NT, 10, 0)] // windows 10 + 11 (same GUID with manifest)
    [ProviderRunsAsyncAttribute(true)]
    public class AeroGlassChangerProvider : Pulse.Base.IOutputProvider
    {
        public void ProcessPicture(Pulse.Base.PictureBatch pb, string config)
        {
            List<Picture> lp = pb.GetPictures(1);
            if (!lp.Any()) return;
            Picture p = lp.First();

            if (string.IsNullOrEmpty(p.LocalPath) || !File.Exists(p.LocalPath)) return;

            Color endAeroColor;

            try
            {
                using (MemoryStream ms = new MemoryStream(File.ReadAllBytes(p.LocalPath)))
                {
                    using (Bitmap bmp = (Bitmap)Bitmap.FromStream(ms))
                    {
                        endAeroColor = PictureManager.CalcAverageColor(bmp);
                    }
                }
            }
            catch { return; }

            // FIXED: Throttle broadcast storm on Win10+ (was 13 broadcasts in 1.3s causing flicker)
            // Win10+ accent via registry only needs final color, not animated transition
            if (Environment.OSVersion.Version.Major >= 10)
            {
                // Single step on Win10/11 - no timer, single registry write + single broadcast
                try
                {
                    Desktop.SetAeroColor(endAeroColor);
                    Log.Logger.Write($"AeroGlass: Set accent color directly for Win10+ (no animation) to {endAeroColor}", Log.LoggerLevels.Info);
                }
                catch (Exception ex)
                {
                    Log.Logger.Write($"AeroGlass: Failed to set accent {ex.Message}", Log.LoggerLevels.Warnings);
                }
                return;
            }

            // Win7/8: animated transition with timer
            ManualResetEvent mre = new ManualResetEvent(false);
            int stepCount = 13;
            Color currentAero = Desktop.GetCurrentAeroColor();
            Color[] transitionColors = CalcColorTransition(currentAero, endAeroColor, stepCount);

            System.Timers.Timer t = new System.Timers.Timer(100);
            int currentStep = 0;

            t.Elapsed += delegate(object sender, System.Timers.ElapsedEventArgs e)
            {
                if (currentStep >= stepCount) { mre.Set(); t.Stop(); return; }
                try { Desktop.SetAeroColor(transitionColors[currentStep]); } catch { }
                currentStep++;
                if (currentStep >= stepCount) { mre.Set(); t.Stop(); }
            };

            t.Start();
            mre.WaitOne(5000); // timeout 5s to avoid hang
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


        public void Activate(object args) { }

        public void Deactivate(object args) { }

        public void Initialize(object args) { }
    }
}
