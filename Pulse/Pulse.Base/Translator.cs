using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Pulse.Base
{
    public class Translator
    {
        //public Translator()
        //{
        //    webClient = new WebClient();
        //    webClient.Encoding = System.Text.Encoding.GetEncoding("koi8-r");
        //    //webClient.DownloadStringCompleted += WebClientDownloadStringCompleted;
        //}

        public static string TranslateText(string input, string languagePair)
        {
            var webClient = new WebClient();
            webClient.Encoding = System.Text.Encoding.GetEncoding("koi8-r");
            var url = String.Format("http://www.google.com/translate_t?hl=en&text={0}&langpair={1}", input, languagePair);
            var result = webClient.DownloadString(new Uri(url));
            result = result.Substring(result.IndexOf("<span title=\"") + "<span title=\"".Length);
            result = result.Substring(result.IndexOf(">") + 1);
            result = result.Substring(0, result.IndexOf("</span>"));
            return result.Trim();
        }

        public static string ConvertToWin1251(string input)
        {
            Encoding utf8 = Encoding.GetEncoding(1251); 
            Encoding win1251 = Encoding.UTF8;

            byte[] utf8Bytes = win1251.GetBytes(input);
            byte[] win1251Bytes = Encoding.Convert(win1251, utf8, utf8Bytes);
            return win1251.GetString(win1251Bytes);
        }
    }
}
