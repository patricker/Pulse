using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Pulse.Base
{
    public class Translator
    {
        private readonly WebClient webClient;

        public string Result { get; set; }

        public event EventHandler Translated;

        public Translator()
        {
            webClient = new WebClient();
            webClient.Encoding = System.Text.Encoding.GetEncoding("koi8-r");
            webClient.DownloadStringCompleted += WebClientDownloadStringCompleted;
        }

        public void TranslateText(string input, string languagePair)
        {
            var url = String.Format("http://www.google.com/translate_t?hl=en&text={0}&langpair={1}", input, languagePair);
            webClient.DownloadStringAsync(new Uri(url));
        }

        public void WebClientDownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            ((WebClient) sender).DownloadStringCompleted -= WebClientDownloadStringCompleted;
            var result = e.Result;
            result = result.Substring(result.IndexOf("<span title=\"") + "<span title=\"".Length);
            result = result.Substring(result.IndexOf(">") + 1);
            result = result.Substring(0, result.IndexOf("</span>"));
            Result = result.Trim();
            //Result = ConvertToWin1251(Result);

            if (Translated != null)
                Translated(null, EventArgs.Empty);
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
