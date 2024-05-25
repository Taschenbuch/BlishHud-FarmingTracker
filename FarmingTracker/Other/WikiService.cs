using System.Diagnostics;
using System;
using Blish_HUD;
using Gw2Sharp.WebApi;

namespace FarmingTracker
{
    public class WikiService
    {
        public static void OpenWikiSearchInDefaultBrowser(string wikiSearchTerm)
        {
            var languageString = GetWikiLanguageString();
            var percentEscapedSearchTerm = Uri.EscapeDataString(wikiSearchTerm);
            var url = $"https://wiki-{languageString}.guildwars2.com/index.php?&search={percentEscapedSearchTerm}";
            OpenUrlInDefaultBrowser(url);
        }

        public static void OpenWikiIdQueryInDefaultBrowser(int apiId)
        {
            var url = $"https://wiki.guildwars2.com/wiki/Special:RunQuery/Search_by_id?title=Special%3ARunQuery%2FSearch_by_id&pfRunQueryFormName=Search+by+id&Search+by+id%5Bid%5D={apiId}&Search+by+id%5Bcontext%5D=&wpRunQuery=&pf_free_text=";
            OpenUrlInDefaultBrowser(url);
        }

        private static string GetWikiLanguageString()
        {
            switch (GameService.Overlay.UserLocale.Value)
            {
                case Locale.Spanish:
                    return "es";
                case Locale.German:
                    return "de";
                case Locale.French:
                    return "fr";
                // Korean and chinese do not their own wikis.
                // The wiki search will work for the items chat codes. Because those are language independent.
                // It wont work for the localized names of currencies.
                case Locale.Korean: 
                case Locale.Chinese:
                case Locale.English:
                default:
                    return "en";
            }
        }

        private static void OpenUrlInDefaultBrowser(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception e)
            {
                Module.Logger.Error(e, "Failed to open url in default browser.");
            }
        }
    }
}
