using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Pulse.Base;
using Pulse.Base.WinAPI;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace AeroGlassChanger
{
    [System.ComponentModel.Description("Aero Glass Color Sync (Win7-11, Accent for Win10/11)")]
    [ProviderPlatform(PlatformID.Win32NT, 6, 1)]
    [ProviderPlatform(PlatformID.Win32NT, 6, 2)]
    [ProviderPlatform(PlatformID.Win32NT, 6, 3)]
    [ProviderPlatform(PlatformID.Win32NT, 10, 0)]
    [ProviderRunsAsync(true)]
    public class AeroGlassChangerProvider : IOutputProvider
    {
        public void ProcessPicture(PictureBatch pb, string config)
        {
            var pics = pb.GetPictures(1);
            if (!pics.Any()) return;

            var p = pics.First();
            if (string.IsNullOrEmpty(p.LocalPath) || !File.Exists(p.LocalPath)) return;

            try
            {
                Color endColor;
                using (var img = Image.Load<Rgba32>(p.LocalPath))
                {
                    // Calculate average color via ImageSharp 1x1
                    using (var small = img.Clone())
                    {
                        small.Mutate(x => x.Resize(1, 1, KnownResamplers.Lanczos3));
                        var pixel = small[0, 0];
                        endColor = System.Drawing.Color.FromArgb(pixel.R, pixel.G, pixel.B);
                    }
                }

                // For Win10+, single step (no broadcast storm)
                if (Environment.OSVersion.Version.Major >= 10)
                {
                    Desktop.SetAeroColor(endColor);
                }
                else
                {
                    // Win7/8: animate with steps
                    var current = Desktop.GetCurrentAeroColor();
                    var transition = CalcColorTransition(current, endColor, 13);
                    int step = 0;
                    var timer = new System.Timers.Timer(100);
                    timer.Elapsed += (s, e) =>
                    {
                        if (step >= transition.Length) { timer.Stop(); return; }
                        Desktop.SetAeroColor(transition[step]);
                        step++;
                        if (step >= transition.Length) timer.Stop();
                    };
                    timer.Start();
                    System.Threading.Thread.Sleep(1500);
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Write($"AeroGlassChanger: {ex.Message}", Log.LoggerLevels.Errors);
            }
        }

        public static System.Drawing.Color[] CalcColorTransition(System.Drawing.Color from, System.Drawing.Color to, int steps)
        {
            var colors = new System.Drawing.Color[steps];
            for (int i = 0; i < steps; i++)
            {
                float t = (float)i / (steps - 1);
                int r = (int)(from.R + (to.R - from.R) * t);
                int g = (int)(from.G + (to.G - from.G) * t);
                int b = (int)(from.B + (to.B - from.B) * t);
                colors[i] = System.Drawing.Color.FromArgb(r, g, b);
            }
            return colors;
        }

        public void Activate(object args) { }
        public void Deactivate(object args) { }
        public void Initialize(object args) { }
    }
}
