using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

namespace Pulse.Base
{
    public class PictureDownload : IDisposable
    {
        public delegate void PictureDownloadEvent(PictureDownload sender);

        public event PictureDownloadEvent PictureDownloaded;
        public event PictureDownloadEvent PictureDownloading;
        public event PictureDownloadEvent PictureDownloadingAborted;
        public event PictureDownloadEvent PictureDownloadProgressChanged;
        public event PictureDownloadEvent PictureDownloadStatusChanged;

        public Picture Picture { get; private set; }
        public int DownloadProgress { get; private set; }
        public DownloadStatus Status
        {
            get { return _status; }
            private set { 
                _status = value;
                if (PictureDownloadStatusChanged != null)
                    PictureDownloadStatusChanged(this);
            }
        }
        public Exception LastError { get; private set; }
        public int Priority { get; set; }
        public int FailureCount { get { return _failureCount; } private set { _failureCount = value; } }

        private HttpUtility.CookieAwareWebClient _client = new HttpUtility.CookieAwareWebClient();
        private int _failureCount = 0;
        private string _tempDownloadPath = string.Empty;
        private DownloadStatus _status;

        public enum DownloadStatus
        {            
            Downloading = 1,
            Error = 10,
            Stopped = 20,
            Complete = 100,
            Cancelled = 500            
        }

        public PictureDownload(Picture p)
        {
            if (string.IsNullOrEmpty(p.LocalPath)) throw new ArgumentNullException("The Local Path of the picture must be defined before downloading it");

            this.Picture = p;
            //set default priority
            this.Priority = 999999999;

            //set default status of Stopped
            Status = DownloadStatus.Stopped;

            //setup webclient events
            _client.DownloadFileCompleted += new System.ComponentModel.AsyncCompletedEventHandler(_client_DownloadFileCompleted);
            _client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(_client_DownloadProgressChanged);
        }

        public void StartDownload()
        {
            // SECURITY FIX: Validate URL scheme http/https only, block file:// and private IPs (SSRF)
            try
            {
                if (!Uri.TryCreate(Picture.Url, UriKind.Absolute, out var uri) ||
                    (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
                {
                    Log.Logger.Write($"PictureDownload blocked non-http URL: {Picture.Url}", Log.LoggerLevels.Warnings);
                    Status = DownloadStatus.Error;
                    LastError = new InvalidOperationException("Only http/https URLs allowed");
                    if (PictureDownloadingAborted != null) PictureDownloadingAborted(this);
                    return;
                }

                // Block private IPs and known SSRF targets
                string host = uri.Host.ToLowerInvariant();
                if (host == "localhost" || host == "127.0.0.1" || host == "::1" ||
                    host.StartsWith("192.168.") || host.StartsWith("10.") ||
                    host.StartsWith("172.16.") || host.StartsWith("172.17.") || host.StartsWith("172.18.") ||
                    host.StartsWith("172.19.") || host.StartsWith("172.20.") || host.StartsWith("172.21.") ||
                    host.StartsWith("172.22.") || host.StartsWith("172.23.") || host.StartsWith("172.24.") ||
                    host.StartsWith("172.25.") || host.StartsWith("172.26.") || host.StartsWith("172.27.") ||
                    host.StartsWith("172.28.") || host.StartsWith("172.29.") || host.StartsWith("172.30.") ||
                    host.StartsWith("172.31.") || host == "0.0.0.0" || host == "169.254.169.254" ||
                    host.StartsWith("169.254.") || host.StartsWith("fc00:") || host.StartsWith("fe80:"))
                {
                    Log.Logger.Write($"PictureDownload blocked private IP host: {host} URL: {Picture.Url}", Log.LoggerLevels.Warnings);
                    Status = DownloadStatus.Error;
                    LastError = new InvalidOperationException("Private IP blocked");
                    if (PictureDownloadingAborted != null) PictureDownloadingAborted(this);
                    return;
                }

                // Extension whitelist: only allow image extensions, force .jpg if suspicious
                string ext = Path.GetExtension(uri.AbsolutePath).ToLowerInvariant();
                var allowedExts = new HashSet<string> { ".jpg", ".jpeg", ".png", ".bmp", ".webp" };
                if (!allowedExts.Contains(ext))
                {
                    // If extension is .dll/.exe etc, block and force .jpg via CalculateLocalPath will handle, but log warning
                    Log.Logger.Write($"PictureDownload: URL has non-image extension {ext}, will be forced to .jpg via CalculateLocalPath", Log.LoggerLevels.Warnings);
                }

                // Decimal/octal IP check: 2130706433 = 127.0.0.1, 0x7f.0.0.1, 0177.0.0.1
                if (System.Text.RegularExpressions.Regex.IsMatch(host, @"^\d+$") ||
                    host.StartsWith("0x") || host.StartsWith("0o") ||
                    System.Text.RegularExpressions.Regex.IsMatch(host, @"^0[0-7]+\."))
                {
                    Log.Logger.Write($"PictureDownload blocked obfuscated IP host: {host}", Log.LoggerLevels.Warnings);
                    Status = DownloadStatus.Error;
                    LastError = new InvalidOperationException("Obfuscated IP blocked");
                    if (PictureDownloadingAborted != null) PictureDownloadingAborted(this);
                    return;
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Write($"PictureDownload validation failed for {Picture.Url}: {ex.Message}", Log.LoggerLevels.Warnings);
                Status = DownloadStatus.Error;
                LastError = ex;
                if (PictureDownloadingAborted != null) PictureDownloadingAborted(this);
                return;
            }

            //check if we have a referrer URL and assign it
            if (Picture.Properties.ContainsKey(Picture.StandardProperties.Referrer))
                _client.Referrer = Picture.Properties[Picture.StandardProperties.Referrer];

            if (!this.Picture.IsGood)
            {
                //generate a temp path to save the file to
                _tempDownloadPath = Path.Combine(Path.GetTempPath(), "PulseTemp_" + Path.GetRandomFileName());
                //download the file async
                try
                {
                    _client.DownloadFileAsync(new Uri(Picture.Url), _tempDownloadPath, Picture);
                }
                catch (Exception ex)
                {
                    Log.Logger.Write($"PictureDownload failed to start for {Picture.Url}: {ex.Message}", Log.LoggerLevels.Warnings);
                    Status = DownloadStatus.Error;
                    LastError = ex;
                    if (PictureDownloadingAborted != null) PictureDownloadingAborted(this);
                    return;
                }

                //set status to downloading
                Status = DownloadStatus.Downloading;
                //call picture downloading event
                if (PictureDownloading != null) PictureDownloading(this);
            }
            else
            {
                //if picture is already in good shape then don't worry about it
                MarkAsComplete();
            }
        }

        public void CancelDownload()
        {
            //if the client is busy then cancel, else just set cancelled
            if (_client.IsBusy)
                _client.CancelAsync();
            else
                Status = DownloadStatus.Cancelled;
        }

        void deleteTempFile()
        {
            try
            {
                //delete temp file (this fails sometimes so I put it in a try/catch)
                File.Delete(_tempDownloadPath);
            }
            catch { }
        }

        void _client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            DownloadProgress = e.ProgressPercentage;
            //call picture download progress changed
            if (PictureDownloadProgressChanged != null) PictureDownloadProgressChanged(this);
        }

        void _client_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            //if this was cancelled by the user then don't retry.  Set progress to 0 and downloading flag to false
            if (e.Cancelled) { 
                Status = DownloadStatus.Cancelled;
                DownloadProgress = 0;
                //try to delete the partially downloaded file
                deleteTempFile();

                //call aborted event
                if (PictureDownloadingAborted != null) PictureDownloadingAborted(this);

                return; 
            }
            
            //if this download errored check our retry count and initiate it again
            if(e.Error != null) {
                LastError = e.Error;

                HandleErrorRetry();
                //stop processing on error (retry is handled in above function)
                return;
            }

            //move the temporary file to it's final destination
            try
            {
                // SECURITY FIX: Check for symlink/junction in destination path before copy
                string destDir = Path.GetDirectoryName(Picture.LocalPath) ?? "";
                if (!string.IsNullOrEmpty(destDir) && Settings.IsReparsePoint(destDir))
                {
                    Log.Logger.Write($"PictureDownload blocked reparse point dest dir {destDir} for {Picture.LocalPath}", Log.LoggerLevels.Warnings);
                    HandleErrorRetry();
                    return;
                }

                string finalPath = Settings.GetFinalPath(Picture.LocalPath);
                if (string.IsNullOrEmpty(finalPath))
                {
                    Log.Logger.Write($"PictureDownload blocked unsafe final path {Picture.LocalPath}", Log.LoggerLevels.Warnings);
                    HandleErrorRetry();
                    return;
                }

                // Ensure extension still whitelisted after CalculateLocalPath (should be .jpg)
                string ext = Path.GetExtension(finalPath).ToLowerInvariant();
                var allowed = new HashSet<string> { ".jpg", ".jpeg", ".png", ".webp", ".bmp" };
                if (!allowed.Contains(ext))
                {
                    Log.Logger.Write($"PictureDownload blocked non-image extension {ext} for {Picture.LocalPath}", Log.LoggerLevels.Warnings);
                    HandleErrorRetry();
                    return;
                }

                File.Copy(_tempDownloadPath, finalPath, true);

                //mark the file as complete
                MarkAsComplete();
            }
            catch (Exception ex)
            {
                Log.Logger.Write($"PictureDownload copy failed for {Picture.LocalPath}: {ex.Message}", Log.LoggerLevels.Warnings);
                HandleErrorRetry();
            }

            deleteTempFile();
        }

        private void HandleErrorRetry()
        {
            //try delete temp file on error/retry
            deleteTempFile();

            _failureCount++;
            Status = DownloadStatus.Error;
            //call download aborted
            if (PictureDownloadingAborted != null) PictureDownloadingAborted(this);
        }

        private void MarkAsComplete()
        {
            //if we aren't cancelled and didn't error then we were successful
            Status = DownloadStatus.Complete;

            //call download complete
            if (PictureDownloaded != null) PictureDownloaded(this);
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}
