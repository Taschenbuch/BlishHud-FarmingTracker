using System.Diagnostics;
using System;

namespace FarmingTracker
{
    public class WikiService
    {
        public static void OpenWikiSearchInDefaultBrowser(string wikiSearchTerm)
        {
            var url = $"https://wiki.guildwars2.com/index.php?&search={Uri.EscapeDataString(wikiSearchTerm)}";
            OpenUrlInDefaultBrowser(url);
        }

        public static void OpenWikiIdQueryInDefaultBrowser(int apiId)
        {
            var url = $"https://wiki.guildwars2.com/wiki/Special:RunQuery/Search_by_id?title=Special%3ARunQuery%2FSearch_by_id&pfRunQueryFormName=Search+by+id&Search+by+id%5Bid%5D={apiId}&Search+by+id%5Bcontext%5D=&wpRunQuery=&pf_free_text=";
            OpenUrlInDefaultBrowser(url);
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
