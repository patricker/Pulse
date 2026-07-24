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
using Pulse.Base.WinAPI;

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
            }
        }

        public void DisableOEMBackground()
        {
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey("Software", RegistryKeyPermissionCheck.ReadWriteSubTree).OpenSubKey("Microsoft").OpenSubKey("Windows")
                .OpenSubKey("CurrentVersion").OpenSubKey("Authentication").OpenSubKey("LogonUI").OpenSubKey("Background", true))
            {
                key.DeleteValue("OEMBackground", false);
            }
        }

        public void SetAccessRules()
        {
            // SECURITY FIX: Don't grant FullControl to current user over System32 subtree (CRITICAL)
            // Old code gave FullControl, allowing DLL hijack via LogonUI SYSTEM reading background
            // For Win7 OEMBackground, SYSTEM needs Read, user needs Write only to backgrounds folder
            // Minimal: grant current user Write + Read, not FullControl, and only on backgrounds folder
            try
            {
                DirectorySecurity dirSec = Directory.GetAccessControl(_oobeBackground);
                // Only ReadAndExecute + Write, not FullControl
                dirSec.AddAccessRule(new FileSystemAccessRule(
                    Environment.UserDomainName + "\\" + Environment.UserName,
                    FileSystemRights.ReadAndExecute | FileSystemRights.Write,
                    InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                    PropagationFlags.None,
                    AccessControlType.Allow));
                Directory.SetAccessControl(_oobeBackground, dirSec);
            }
            catch (Exception ex)
            {
                Log.Logger.Write($"SetAccessRules failed: {ex.Message}", Log.LoggerLevels.Warnings);
            }
        }

        public void CreateDirs()
        {
            IntPtr oldVal = IntPtr.Zero;
            bool disabled = false;
            try
            {
                disabled = WinAPI.Wow64DisableWow64FsRedirection(ref oldVal);
                if (!Directory.Exists(_oobeInfo))
                    Directory.CreateDirectory(_oobeInfo);
                if (!Directory.Exists(_oobeBackground))
                    Directory.CreateDirectory(_oobeBackground);
                SetAccessRules();
            }
            finally
            {
                if (disabled)
                {
                    try { WinAPI.Wow64RevertWow64FsRedirection(oldVal); } catch { }
                }
            }
        }

        public void SetNewPicture(Picture p)
        {
            // Windows 10/11 path: try modern lock screen API first
            if (Environment.OSVersion.Version.Major >= 10 || (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor >= 2))
            {
                bool modernSuccess = false;
                try
                {
                    modernSuccess = Pulse.Base.WinAPI.Desktop.SetLockScreenImage(p.LocalPath);
                    if (modernSuccess)
                    {
                        Log.Logger.Write($"OEMBackgroundManager: Set lock screen via modern API: {p.LocalPath}", Log.LoggerLevels.Info);
                        return; // Success - no need for OEM path
                    }
                    else
                    {
                        Log.Logger.Write($"OEMBackgroundManager: Modern lock screen failed (needs admin for HKLM policy or net8 WinRT build). " +
                                         $"On Win10/11, lock screen requires admin for machine policy OR net8 build with UserProfilePersonalizationSettings. " +
                                         $"File: {p.LocalPath}. OEMBackground (oobe) is dead on Win8+ and will not be attempted.", Log.LoggerLevels.Warnings);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Log.Logger.Write($"OEMBackgroundManager: Modern lock screen attempt failed: {ex.Message}", Log.LoggerLevels.Warnings);
                    return;
                }
            }

            IntPtr oldVal2 = IntPtr.Zero;
            bool disabled2 = false;
            try
            {
                disabled2 = WinAPI.Wow64DisableWow64FsRedirection(ref oldVal2);

                var fiOriginal = new FileInfo(p.LocalPath);

                if (fiOriginal.Exists && fiOriginal.Length > 0)
                {
                    var outPutFile = Path.Combine(_oobeBackground, "backgroundDefault.jpg");

                    try { File.Delete(outPutFile); }
                    catch (Exception ex)
                    {
                        Log.Logger.Write($"Error deleting existing Logon Background at '{outPutFile}': {ex.Message}", Log.LoggerLevels.Errors);
                    }

                    using (FileStream fs = File.OpenRead(p.LocalPath))
                    {
                        using (Image img = Image.FromStream(fs))
                        {
                            // FIX: Use invariant culture and tolerance compare (old used ToString culture dependent)
                            double ratio = (double)img.Width / (double)img.Height;
                            double[] validRatios = { 1.25, 1.33, 1.60, 1.67, 1.77 };
                            bool isValid = validRatios.Any(v => Math.Abs(v - ratio) < 0.02);

                            if (!isValid)
                            {
                                PictureManager.ShrinkImage(p.LocalPath, outPutFile, 0, 0, 90);
                            }
                            else
                            {
                                img.Save(outPutFile);
                            }
                        }
                    }

                    var fiNewFile = new FileInfo(outPutFile);
                    if (fiNewFile.Length / 1024 >= 245)
                    {
                        PictureManager.ReduceQuality(outPutFile, outPutFile, 90);
                    }
                }
            }
            finally
            {
                if (disabled2)
                {
                    try { WinAPI.Wow64RevertWow64FsRedirection(oldVal2); } catch { }
                }
            }
        }
    }
}
