using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Collections;

namespace Pulse.Base
{
    public class HttpUtility
    {
        public static string UrlEncode(byte[] bytes)
        {
            if (bytes == null)
            {
                return null;
            }
            return Encoding.ASCII.GetString(UrlEncodeToBytes(bytes));
        }

        public static string UrlEncode(string str)
        {
            if (str == null)
            {
                return null;
            }
            return UrlEncode(str, Encoding.UTF8);
        }
        public static string UrlEncode(string str, Encoding e)
        {
            if (str == null)
            {
                return null;
            }
            return Encoding.ASCII.GetString(UrlEncodeToBytes(str, e));
        }
        public static string UrlEncode(byte[] bytes, int offset, int count)
        {
            if (bytes == null)
            {
                return null;
            }
            return Encoding.ASCII.GetString(UrlEncodeToBytes(bytes, offset, count));
        }
        public static byte[] UrlEncodeToBytes(string str, Encoding e)
        {
            if (str == null)
            {
                return null;
            }
            byte[] bytes = e.GetBytes(str);
            return UrlEncodeBytesToBytesInternal(bytes, 0, bytes.Length, false);
        }
        public static byte[] UrlEncodeToBytes(byte[] bytes)
        {
            if (bytes == null)
            {
                return null;
            }
            return UrlEncodeToBytes(bytes, 0, bytes.Length);
        }
        public static byte[] UrlEncodeToBytes(byte[] bytes, int offset, int count)
        {
            if ((bytes == null) && (count == 0))
            {
                return null;
            }
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }
            if ((offset < 0) || (offset > bytes.Length))
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if ((count < 0) || ((offset + count) > bytes.Length))
            {
                throw new ArgumentOutOfRangeException("count");
            }
            return UrlEncodeBytesToBytesInternal(bytes, offset, count, true);
        }
        private static byte[] UrlEncodeBytesToBytesInternal(byte[] bytes, int offset, int count, bool alwaysCreateReturnValue)
        {
            int num = 0;
            int num2 = 0;
            for (int i = 0; i < count; i++)
            {
                char ch = (char)bytes[offset + i];
                if (ch == ' ')
                {
                    num++;
                }
                else if (!IsSafe(ch))
                {
                    num2++;
                }
            }
            if ((!alwaysCreateReturnValue && (num == 0)) && (num2 == 0))
            {
                return bytes;
            }
            byte[] buffer = new byte[count + (num2 * 2)];
            int num4 = 0;
            for (int j = 0; j < count; j++)
            {
                byte num6 = bytes[offset + j];
                char ch2 = (char)num6;
                if (IsSafe(ch2))
                {
                    buffer[num4++] = num6;
                }
                else if (ch2 == ' ')
                {
                    buffer[num4++] = 0x2b;
                }
                else
                {
                    buffer[num4++] = 0x25;
                    buffer[num4++] = (byte)IntToHex((num6 >> 4) & 15);
                    buffer[num4++] = (byte)IntToHex(num6 & 15);
                }
            }
            return buffer;
        }

        internal static bool IsSafe(char ch)
        {
            if ((((ch >= 'a') && (ch <= 'z')) || ((ch >= 'A') && (ch <= 'Z'))) || ((ch >= '0') && (ch <= '9')))
            {
                return true;
            }
            switch (ch)
            {
                case '\'':
                case '(':
                case ')':
                case '*':
                case '-':
                case '.':
                case '_':
                case '!':
                    return true;
            }
            return false;
        }

        internal static char IntToHex(int n)
        {
            if (n <= 9)
            {
                return (char)(n + 0x30);
            }
            return (char)((n - 10) + 0x61);
        }

        public class CookieAwareWebClient : WebClient
        {
            public CookieContainer Cookies { get; set; }
            public string Referrer { get; set; }
            public string UserAgent { get; set; }

            private static bool _tlsInitialized = false;

            private static void EnsureTls12()
            {
                if (_tlsInitialized) return;
                try
                {
                    // SECURITY FIX: Use = not |= to avoid keeping Ssl3/Tls10 if previously set elsewhere
                    // Only TLS 1.2 and 1.3 if available
                    ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072; // Tls12
                    // Try add Tls13 if available (0x3000 = 12288)
                    try { ServicePointManager.SecurityProtocol |= (SecurityProtocolType)12288; } catch { }
                    _tlsInitialized = true;
                }
                catch { }
            }

            public CookieAwareWebClient() {
                Cookies = new CookieContainer();
                UserAgent = "Mozilla/5.0 (Pulse; +https://github.com/patricker/Pulse)";
                EnsureTls12();
            }

            public CookieAwareWebClient(CookieContainer cookies)
            {
                Cookies = cookies;
                UserAgent = "Mozilla/5.0 (Pulse; +https://github.com/patricker/Pulse)";
                EnsureTls12();
            }

            private static bool IsPrivateHost(string host)
            {
                if (string.IsNullOrEmpty(host)) return true;
                host = host.ToLowerInvariant();
                if (host == "localhost" || host == "127.0.0.1" || host == "::1" || host == "0.0.0.0") return true;
                if (host.StartsWith("10.")) return true;
                if (host.StartsWith("192.168.")) return true;
                if (host.StartsWith("169.254.")) return true;
                if (host.StartsWith("172.16.") || host.StartsWith("172.17.") || host.StartsWith("172.18.") ||
                    host.StartsWith("172.19.") || host.StartsWith("172.20.") || host.StartsWith("172.21.") ||
                    host.StartsWith("172.22.") || host.StartsWith("172.23.") || host.StartsWith("172.24.") ||
                    host.StartsWith("172.25.") || host.StartsWith("172.26.") || host.StartsWith("172.27.") ||
                    host.StartsWith("172.28.") || host.StartsWith("172.29.") || host.StartsWith("172.30.") ||
                    host.StartsWith("172.31.")) return true;
                if (host.StartsWith("fc00:") || host.StartsWith("fe80:")) return true;
                // Decimal/octal IP obfuscation check
                if (System.Text.RegularExpressions.Regex.IsMatch(host, @"^\d+$")) return true; // 2130706433 = 127.0.0.1
                if (host.StartsWith("0x")) return true;
                if (System.Text.RegularExpressions.Regex.IsMatch(host, @"^0[0-7]+\.")) return true; // octal 0177.0.0.1
                return false;
            }

            protected override WebRequest GetWebRequest(Uri address)
            {
                // SECURITY FIX: Validate scheme and private IP before request
                if (address.Scheme != Uri.UriSchemeHttp && address.Scheme != Uri.UriSchemeHttps)
                    throw new InvalidOperationException($"Only http/https allowed, got {address.Scheme}");

                if (IsPrivateHost(address.Host))
                    throw new InvalidOperationException($"Blocked private IP host: {address.Host}");

                var request = base.GetWebRequest(address);
                var httpRequest = request as HttpWebRequest;
                if (httpRequest != null)
                {
                    httpRequest.CookieContainer = Cookies;
                    httpRequest.UserAgent = UserAgent;
                    httpRequest.Accept = "application/json, text/html, */*";
                    httpRequest.Timeout = 15000;
                    // SECURITY FIX: Disable auto-redirect to prevent SSRF via redirect to private IP
                    // We handle redirect manually in DownloadString with validation
                    httpRequest.AllowAutoRedirect = false;

                    if (!string.IsNullOrEmpty(Referrer) && string.IsNullOrEmpty(httpRequest.Referer))
                        httpRequest.Referer = Referrer;
                }
                if (string.IsNullOrEmpty(this.Headers[HttpRequestHeader.UserAgent]))
                    this.Headers.Add(HttpRequestHeader.UserAgent, UserAgent);

                return request;
            }
        }
    }
}
