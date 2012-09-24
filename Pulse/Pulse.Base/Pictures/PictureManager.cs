using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Drawing.Drawing2D;

namespace Pulse.Base
{
    public class PictureManager
    {
        #region "helpers"
        public static Pair<int, int> PrimaryScreenResolution { 
            get {
                var rect = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
                return new Pair<int, int>(rect.Width, rect.Height);
            }
        }

        public static List<Rectangle> ScreenResolutions
        {
            get
            {
                List<Rectangle> rects = new List<Rectangle>();

                rects.AddRange(from c in System.Windows.Forms.Screen.AllScreens select c.Bounds);

                return rects;
            }
        }

        public static Rectangle TotalScreenResolution
        {
            get
            {
                List<Rectangle> rects = ScreenResolutions;

                Rectangle rect = new Rectangle(0,0,0,0);

                foreach (Rectangle r in rects)
                {
                    rect = Rectangle.Union(rect, r);
                }

                return rect;
            }
        }

        public static void ShrinkImage(string imgPath, string outPath, int destWidth, int destHeight, int quality)
        {
            using (Image img = ShrinkImage(imgPath, destWidth, destHeight))
            {
                //-----write out Thumbnail to the output stream------        
                //get jpeg image coded info so we can use it when saving
                ImageCodecInfo ici = ImageCodecInfo.GetImageEncoders().Where(c => c.MimeType == "image/jpeg").First();

                EncoderParameters epParameters = new EncoderParameters(1);
                epParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);

                //save image to file
                img.Save(outPath, ici, epParameters);
            }
        }
        
        //Patricker - This code came from a stackoverflow answer I posted a while back.  I converted it to C# from VB.Net and from
        // asp.net to windows.  Link: http://stackoverflow.com/questions/4436209/asp-net-version-of-timthumb-php-class/4506072#4506072
        /// <summary>
        /// 
        /// </summary>
        /// <param name="imgPath">Path to image to load</param>
        /// <param name="destWidth">Desired width (0 to base off of aspect ratio and specified height)</param>
        /// <param name="destHeight">Desired Height (0 to base off of aspect ratio and specified width)</param>
        /// <remarks>If both height and width are 0 or non zero if the aspect ratio of the image
        /// does not match the aspect ratio based on width/height specifed then the image will skew. 
        /// (if both height/width are zero then screen resolution is used)</remarks>
        public static Image ShrinkImage(string imgPath, int destWidth, int destHeight)
        {
            //load image from file path
            using (FileStream fs = File.OpenRead(imgPath))
            {
                using (Image img = Bitmap.FromStream(fs))
                {
                    double origRatio = (double)Math.Min(img.Width, img.Height) / (double)Math.Max(img.Width, img.Height);

                    //---Calculate thumbnail sizes---
                    double destRatio = 0;

                    //if both width and height are 0 then use defaults (Screen resolution)
                    if (destWidth == 0 & destHeight == 0)
                    {
                        var scResolution = PrimaryScreenResolution;
                        destWidth = scResolution.First;
                        destHeight = scResolution.Second;

                    }
                    else if (destWidth > 0 & destHeight > 0)
                    {
                        //do nothing, we have both sizes already
                    }
                    else if (destWidth > 0)
                    {
                        destHeight = (int)Math.Floor((double)img.Height * (destWidth / img.Width));
                    }
                    else if (destHeight > 0)
                    {
                        destWidth = (int)Math.Floor((double)img.Width * ((double)destHeight / (double)img.Height));
                    }

                    destRatio = (double)Math.Min(destWidth, destHeight) / (double)Math.Max(destWidth, destHeight);

                    //calculate source image sizes (rectangle) to get pixel data from        
                    int sourceWidth = img.Width;
                    int sourceHeight = img.Height;

                    int sourceX = 0;
                    int sourceY = 0;

                    double cmpx = (double)img.Width / (double)destWidth;
                    double cmpy = (double)img.Height / (double)destHeight;

                    //selection is based on the smallest dimension
                    if (cmpx > cmpy)
                    {
                        sourceWidth = (int)((double)img.Width / cmpx * cmpy);
                        sourceX = (int)(((double)img.Width - ((double)img.Width / cmpx * cmpy)) / 2);
                    }
                    else if (cmpy > cmpx)
                    {
                        sourceHeight = (int)((double)img.Height / cmpy * cmpx);
                        sourceY = (int)(((double)img.Height - ((double)img.Height / cmpy * cmpx)) / 2);
                    }

                    //---create the new image---
                    Bitmap bmpThumb = new Bitmap(destWidth, destHeight);
                    var g = Graphics.FromImage(bmpThumb);
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.CompositingQuality = CompositingQuality.HighQuality;
                    g.SmoothingMode = SmoothingMode.AntiAlias;

                    g.DrawImage(img, new Rectangle(0, 0, destWidth, destHeight), new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight), GraphicsUnit.Pixel);

                    return bmpThumb;
                }
            }
        }
        #endregion

        public PictureList GetPictureList(PictureSearch ps)
        {
            PictureList Pictures = null;

            if (ps == null || ps.SearchProvider == null || ps.SearchProvider.Instance == null) return Pictures;

            Pictures = ps.SearchProvider.SearchResults;
            var loadedFromFile = false;
            var fPath = Path.Combine(ps.SaveFolder, "CACHE_" + ps.GetSearchHash().ToString() + "_" + ps.SearchProvider.Instance.GetType().ToString() + ".xml");

            if (Pictures == null)
            {

                Pictures = LoadCachedSearch(ps, fPath);
                loadedFromFile = Pictures != null;
            }
            else
            {
                loadedFromFile = false;
            }
            
            //if we have no pictures to work with try and get them
            if (Pictures == null || Pictures.Pictures.Count == 0)
            {
                Pictures = ((IInputProvider)ps.SearchProvider.Instance).GetPictures(ps);
                Pictures.SearchSettingsHash = ps.GetSearchHash();
                loadedFromFile = false;
            }
            
            //cache the picture list to file
            if (!loadedFromFile)
            {
                Pictures.Save(fPath);
            } 

            //return whatever list of pictures was found
            return Pictures;
        }

        public PictureList LoadCachedSearch(PictureSearch ps, string cachePath)
        {
            PictureList result = null;

            //check if we should load from file
            if (File.Exists(cachePath))
            {
                try
                {
                    result = PictureList.LoadFromFile(cachePath);
                }
                catch (Exception ex)
                {
                    Log.Logger.Write(string.Format("Error loading picture cache from file, cache will not be used. Exception details: {0}", ex.ToString()), Log.LoggerLevels.Errors);
                }
            }

            return result;
        }
    }
}
