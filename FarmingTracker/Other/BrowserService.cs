using System.Diagnostics;
using System;
using Blish_HUD;
using Gw2Sharp.WebApi;

namespace FarmingTracker
{
    public class BrowserService
    {
        public static void OpenUrlInDefaultBrowser(string url)
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

        public static string GetGw2EfficiencyLanguageString()
        {
            return GameService.Overlay.UserLocale.Value switch
            {
                Locale.Spanish => "es",
                Locale.German => "de",
                Locale.French => "fr",
                _ => "en",
            };
        }
    }
}
