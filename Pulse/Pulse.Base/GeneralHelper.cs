using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Security.Cryptography;

namespace Pulse.Base
{
    public class GeneralHelper
    {
        public static IWebProxy Proxy;

        public static string GetWebPageContent(string url)
        {
            if (string.IsNullOrEmpty(url))
                return null;

            var webClient = new WebClient();
            if (Proxy != null)
            {
                webClient.Proxy = Proxy;
            }

            webClient.Headers["User-Agent"] = "Mozilla/4.0 (Compatible; Windows NT 5.1; MSIE 8.0) (compatible; MSIE 8.0; Windows NT 5.1;)";

            try
            {
                //тупо, но пока ничего лучше в голову не пришло
                var reader = new StreamReader(webClient.OpenRead(url));
                var line = reader.ReadLine(); //read the first line with encoding name
                if (string.IsNullOrEmpty(line))
                    return null;

                if (line.Contains("encoding"))
                {
                    var encoding = line.Substring(line.IndexOf("encoding") + 10); //parse encoding name from the first line of xml
                    encoding = encoding.Substring(0, encoding.IndexOf('"'));
                    reader.Close();

                    reader = new StreamReader(webClient.OpenRead(url), Encoding.GetEncoding(encoding)); //reopen stream with right encoding
                    line = "";
                }

                return line + reader.ReadToEnd();
            }
            catch (Exception ex)
            {
                return String.Empty;
            }
        }

        private static byte[] s_aditionalEntropy = { 2,1,5,8,2,3,1,8,2,9  };

        public static string Protect(string data)
        {
            byte[] bytes = Encoding.Default.GetBytes(data);

            byte[] protectedBytes = Protect(bytes);

            return Convert.ToBase64String(protectedBytes);
        }

        public static byte[] Protect(byte[] data)
        {
            try
            {
                // Encrypt the data using DataProtectionScope.CurrentUser. The result can be decrypted
                //  only by the same current user.
                return ProtectedData.Protect(data, s_aditionalEntropy, DataProtectionScope.CurrentUser);
            }
            catch
            {
                return null;
            }
        }

        public static string Unprotect(string data)
        {
            byte[] bytes = Convert.FromBase64String(data);

            byte[] unprotectedBytes = Unprotect(bytes);

            return Encoding.Default.GetString(unprotectedBytes);
        }

        public static byte[] Unprotect(byte[] data)
        {
            try
            {
                //Decrypt the data using DataProtectionScope.CurrentUser.
                return ProtectedData.Unprotect(data, s_aditionalEntropy, DataProtectionScope.CurrentUser);
            }
            catch
            {
                return null;
            }
        }
    }
}
