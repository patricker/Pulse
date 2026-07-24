using System;
using System.Linq;
using Pulse.Base;
using Pulse.Base.Providers;
using System.Text.RegularExpressions;
using System.Net;

namespace GoogleImages
{
    // RETIRED 2026: Google removed gbv=1 basic HTML (imgurl= regex) in 2018, now JS infinite scroll + consent + bot check.
    // This provider cannot be fixed with scraping. Options: Google Custom Search JSON API (paid, 100/day free) or Bing Image Search API.
    // Marking obsolete but keeping DLL for backward compat - returns empty list with warning.
    [Obsolete("Google Images scraping is dead since 2018. Use Wallhaven, Bing, or NASA instead. See WIN10-11-PATH.md")]
    [ProviderConfigurationClass(typeof(GoogleImageSearchSettings))]
    [System.ComponentModel.Description("Google Images (RETIRED - use Wallhaven/Bing)")]
    [ProviderIcon(typeof(Properties.Resources),"googleImages")]
    public class Provider : IInputProvider
    {
        private readonly Regex _imagesRegex2 = new Regex(@"imgurl=(?<imgurlgrp>http.*?)&amp;.*?imgrefurl=(?<imgrefgrp>http.*?)&amp;.*?src=[""'](?<thumbURL>.*?)[""'].*?>",RegexOptions.Singleline);//"(imgurl=)(?<imgurl>http.*?)[^&>]*([>&]{1})");
        private const string baseURL = "http://images.google.com/search?tbm=isch&hl=en&source=hp&biw=&bih=&gbv=1&q={0}{1}&start={2}";
        private readonly CookieContainer _cookies = new CookieContainer();

        public Provider()
        {
        }

        public void Initialize(object args)
        {
            //nothing to do here
        }

        public void Activate(object args) { }
        public void Deactivate(object args) { }


        public PictureList GetPictures(PictureSearch ps)
        {
            var result = new PictureList() { FetchDate = DateTime.Now };

            Log.Logger.Write("GoogleImages provider is RETIRED. Google removed gbv=1 HTML in 2018. Use Wallhaven (new API), Bing Image Search API, or NASA APOD instead. Returning empty. See WIN10-11-PATH.md for migration to Bing API.", Log.LoggerLevels.Warnings);

            // Optional: if you have Bing API key, you could re-implement here using Bing Search API:
            // https://api.bing.microsoft.com/v7.0/images/search?q={query}&count=50&mkt=en-us
            // Header: Ocp-Apim-Subscription-Key: YOUR_KEY
            // For now, return empty to avoid breaking Pulse.

            return result;

            /* Legacy scraping code retired - kept for reference:
            var pageIndex = ps.PageToRetrieve; ...
            */
        }

        private void SetSafeSearchSetting(GoogleImageSearchSettings.GoogleSafeSearchOptions gsso)
        {
            using (var client = new HttpUtility.CookieAwareWebClient(_cookies))
            {
                //First we need to access the preferences page so we can get the special ID
                var response = client.DownloadString("http://images.google.com/preferences?hl=en");
                //parse out signature
                var specialID = Regex.Match(response, "<input type=\"hidden\" name=\"sig\" value=\"(?<sig>.*?)\">").Groups["sig"].Value;

                //options are "on", "images", "off"
                var safeUIOption = "";
                switch(gsso) {
                    case GoogleImageSearchSettings.GoogleSafeSearchOptions.Off:
                        safeUIOption = "off";
                        break;
                    case GoogleImageSearchSettings.GoogleSafeSearchOptions.On:
                        safeUIOption = "on";
                        break;
                }
                //set prefs
                string url = string.Format("http://images.google.com/setprefs?sig={0}&hl=en&lr=lang_en&uulo=1&muul=4_20&luul=&safeui={1}&suggon=1&newwindow=0&q=",
                                specialID.Replace("=", "%3D"), safeUIOption);

                var finalResponse = client.DownloadString(url);
            }
        }
    }
}
