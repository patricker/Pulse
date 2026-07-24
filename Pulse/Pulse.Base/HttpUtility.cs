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

            public CookieAwareWebClient() {
                Cookies = new CookieContainer();
                UserAgent = "Mozilla/5.0 (Pulse; +https://github.com/patricker/Pulse)";
                // SECURITY FIX: Only TLS 1.2, no SSL3/TLS1.0 (was enabling Ssl3 = POODLE)
                // Also don't mutate globally in constructor - but we keep for compat, only Tls12
                try
                {
                    // 3072 = Tls12, 768 = Tls11 (deprecated but better than Ssl3)
                    // Only enable Tls12
                    ServicePointManager.SecurityProtocol |= (SecurityProtocolType)3072;
                    // For .NET 4.8, Tls12 is default, so this is safe. Remove Ssl3/Tls10.
                }
                catch { }
            }

            public CookieAwareWebClient(CookieContainer cookies)
            {
                Cookies = cookies;
                UserAgent = "Mozilla/5.0 (Pulse; +https://github.com/patricker/Pulse)";
                try
                {
                    ServicePointManager.SecurityProtocol |= (SecurityProtocolType)3072;
                }
                catch { }
            }

            protected override WebRequest GetWebRequest(Uri address)
            {
                var request = base.GetWebRequest(address);
                var httpRequest = request as HttpWebRequest;
                if (httpRequest != null)
                {
                    httpRequest.CookieContainer = Cookies;
                    httpRequest.UserAgent = UserAgent;
                    httpRequest.Accept = "application/json, text/html, */*";
                    // Set timeout
                    httpRequest.Timeout = 15000;

                    if (!string.IsNullOrEmpty(Referrer) && string.IsNullOrEmpty(httpRequest.Referer))
                        httpRequest.Referer = Referrer;
                }
                // Also set header for WebClient's underlying headers
                if (string.IsNullOrEmpty(this.Headers[HttpRequestHeader.UserAgent]))
                    this.Headers.Add(HttpRequestHeader.UserAgent, UserAgent);

                return request;
            }
        }
    }
}
