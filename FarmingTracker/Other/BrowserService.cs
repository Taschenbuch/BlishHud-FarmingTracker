using System.Diagnostics;
using System;

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
    }
}
