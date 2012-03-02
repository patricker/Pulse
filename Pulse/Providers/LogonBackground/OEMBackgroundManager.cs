using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.AccessControl;
using Microsoft.Win32;
using Pulse.Base;
using System.Drawing.Imaging;
using System.Drawing;
using System.Runtime.InteropServices;

namespace LogonBackground
{
    public class OEMBackgroundManager
    {
        private string _oobeBackground = Path.Combine(Environment.SystemDirectory, "oobe\\info\\backgrounds");
        private string _oobeInfo = Path.Combine(Environment.SystemDirectory, "oobe\\info");

        public void EnableOEMBackground()
        {
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey("Software").OpenSubKey("Microsoft").OpenSubKey("Windows")
                .OpenSubKey("CurrentVersion").OpenSubKey("Authentication").OpenSubKey("LogonUI").OpenSubKey("Background", true))
            {
                key.SetValue("OEMBackground", 1, RegistryValueKind.DWord);
                key.Close();
            }
        }

        public void DisableOEMBackground()
        {
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey("Software", RegistryKeyPermissionCheck.ReadWriteSubTree).OpenSubKey("Microsoft").OpenSubKey("Windows")
                .OpenSubKey("CurrentVersion").OpenSubKey("Authentication").OpenSubKey("LogonUI").OpenSubKey("Background", true))
            {
                key.DeleteValue("OEMBackground", false);
                key.Close();
            }
        }

        public void SetAccessRules()
        {
            DirectorySecurity dirSec = Directory.GetAccessControl(_oobeBackground);
            dirSec.AddAccessRule(new FileSystemAccessRule(Environment.UserDomainName + "\\" + Environment.UserName, FileSystemRights.FullControl, AccessControlType.Allow));
            Directory.SetAccessControl(_oobeBackground, dirSec);
        }

        public void CreateDirs()
        {
            //make sure that file system redirection is disabled, otherwise the image shows up somewhere.
            // thanks to (http://stackoverflow.com/questions/6617530/could-not-find-a-part-of-the-path-c-windows-system32-oobe-info-background) for this
            IntPtr ptr = new IntPtr();
            bool isWow64FsRedirectionDisabled = Wow64DisableWow64FsRedirection(ref ptr);

            if (!Directory.Exists(_oobeInfo))
                Directory.CreateDirectory(_oobeInfo);
            if (!Directory.Exists(_oobeBackground))
                Directory.CreateDirectory(_oobeBackground);
            SetAccessRules();
        }

        public void SetNewPicture(Picture p)
        {
            //make sure that file system redirection is disabled, otherwise the image shows up somewhere else.
            // thanks to (http://stackoverflow.com/questions/6617530/could-not-find-a-part-of-the-path-c-windows-system32-oobe-info-background) for this
            IntPtr ptr = new IntPtr();
            bool isWow64FsRedirectionDisabled = Wow64DisableWow64FsRedirection(ref ptr);

            var fiOriginal = new FileInfo(p.LocalPath);
            
            if (fiOriginal.Exists && fiOriginal.Length > 0)
            {
                var outPutFile = Path.Combine(_oobeBackground, "backgroundDefault.jpg");

                //delete any existing files
                try { File.Delete(outPutFile); }
                catch { }

                //check if image resolution is a valid aspect ratio, if not try and fix it
                Image img = Image.FromFile(p.LocalPath);

                var strRatio = Math.Round((double)img.Width / (double)img.Height, 3).ToString();
                var aspectRatio = Convert.ToDouble(strRatio.Length <= 4 ? strRatio : strRatio.Substring(0, 4));

                //aspect ratio list came from link: http://social.technet.microsoft.com/Forums/en-US/w7itproui/thread/b52689fb-c733-4229-8a10-4aa32d527832/
                // this list may not be complete
                List<double> validRatios = new List<double>();
                validRatios.Add(1.25);
                validRatios.Add(1.33);
                validRatios.Add(1.60);
                validRatios.Add(1.67);
                validRatios.Add(1.77);                              

                //if not a valid aspect ratio, adjust image
                if (!validRatios.Contains(aspectRatio))
                {
                    PictureManager.ShrinkImage(p.LocalPath, outPutFile, 0, 0, 90);
                }
                else
                {
                    //save image to output file
                    img.Save(outPutFile);
                }
                
                //File.Copy(p.LocalPath, outPutFile, true);
                var fiNewFile = new FileInfo(outPutFile);

                //fix picture if it's to big
                if (fiNewFile.Length / 1024 >= 245)
                {
                    //reduce quality until we are under 245kb
                    ReduceQuality(outPutFile, outPutFile, 90);
                }
            }
        }

        public static void ReduceQuality(string file, string destFile, int quality)
        {
            // we get the image/jpeg encoder using linq
            ImageCodecInfo iciJpegCodec = (from c in ImageCodecInfo.GetImageEncoders() where c.MimeType == "image/jpeg" select c).SingleOrDefault();

            // Store the quality parameter in the list of encoder parameters
            EncoderParameters epParameters = new EncoderParameters(1);
            epParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);

            //original image ms
            MemoryStream ms = new MemoryStream(File.ReadAllBytes(file));

            //we use htis to keep track of the current size of the image, default to int.max to make sure we always run
            var fSize = int.MaxValue;

            using (Image newImage = Image.FromStream(ms))
            {
                //check if the image we generated is larger then 245kb, if it is reduce quality by 10 and try again
                while (fSize >= 245)
                {
                    // Save the new file at tshe selected path with the specified encoder parameters, and reuse the same file name
                    newImage.Save(destFile, iciJpegCodec, epParameters);

                    //get output size in kb
                    fSize = (int)(new FileInfo(destFile).Length / 1024);

                    //reduce quality by 10, this will only affect the output if while loop continues
                    quality -= 10;
                    epParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
                }
            }
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool Wow64DisableWow64FsRedirection(ref IntPtr ptr);

    }
}
