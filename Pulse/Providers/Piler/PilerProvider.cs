using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pulse.Base;
using System.Drawing;
using Pulse.Base.WinAPI;

namespace Piler
{
    public class PilerProvider : IOutputProvider
    {
        private static Random _rand = new Random();

        public void ProcessPicture(PictureBatch pb, string config)
        {
            List<Picture> pics = pb.GetPictures(20);

            //for starters lets use an existing image as the backdrop
            Picture pBackdrop = pics.First();
            Image bmpBackdrop = Image.FromFile(pBackdrop.LocalPath);
            Graphics g = Graphics.FromImage(bmpBackdrop);

            //now get 4 or 5 other pics and strew them about
            foreach (Picture p in pics.Skip(1))
            {
                Picture pPile = p;
                Bitmap bmpPile = rotateImage((Bitmap)Bitmap.FromFile(pPile.LocalPath), _rand.Next(-70, 70));
                Graphics gPile = Graphics.FromImage(bmpPile);

                //set rotation center point
                gPile.TranslateTransform((float)bmpPile.Width / 2, (float)bmpPile.Height / 2);
                //rotate the image between -70 and +70 degrees
                gPile.RotateTransform(_rand.Next(-70, 70));

                //picka random x,y coordinate to draw the image, shrink to 25%
                g.DrawImage(bmpPile, new Rectangle(_rand.Next(50, bmpBackdrop.Width - 50),
                                                    _rand.Next(50, bmpBackdrop.Height - 50),
                                                    Convert.ToInt32(bmpBackdrop.Width * .1),
                                                    Convert.ToInt32(bmpBackdrop.Height * .1)));
            }
            string savePath = System.IO.Path.Combine(Settings.CurrentSettings.CachePath,
                                            string.Format("{0}.jpg", Guid.NewGuid()));
            bmpBackdrop.Save(savePath, System.Drawing.Imaging.ImageFormat.Jpeg);

            WinAPI.SystemParametersInfo(WinAPI.SPI_SETDESKWALLPAPER, 0, savePath, WinAPI.SPIF_UPDATEINIFILE | WinAPI.SPIF_SENDWININICHANGE);
        }

        //this block of code came from
        // :http://www.switchonthecode.com/tutorials/csharp-tutorial-image-editing-rotate
        private Bitmap rotateImage(Bitmap b, float angle)
        {
            //create a new empty bitmap to hold rotated image
            Bitmap returnBitmap = new Bitmap(b.Width, b.Height);
            //make a graphics object from the empty bitmap
            Graphics g = Graphics.FromImage(returnBitmap);
            //move rotation point to center of image
            g.TranslateTransform((float)b.Width / 2, (float)b.Height / 2);
            //rotate
            g.RotateTransform(angle);
            //move image back
            g.TranslateTransform(-(float)b.Width / 2, -(float)b.Height / 2);
            //draw passed in image onto graphics object
            g.DrawImage(b, new Point(0, 0));
            return returnBitmap;
        }

        public void Activate(object args) { }
        public void Deactivate(object args) { }
        public void Initialize(object args) { }
    }
}
