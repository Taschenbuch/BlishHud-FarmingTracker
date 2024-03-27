using Blish_HUD.Settings;

namespace FarmingTracker
{
    public class SettingService // singular because Setting"s"Service already exists in Blish
    {
        public SettingService(SettingCollection settings)
        {
            DrfApiToken = settings.DefineSetting(
                "drf api token",
                string.Empty,
                () => "DRF API Token/Key", 
                () => "Create an account on https://drf.rs/ and link it with your GW2 Account(s).\n" +
                "Open the drf.rs settings page. Click on 'Regenerate Token'.\n" +
                "Copy the token. Paste the token with CTRL + V into this input.");
        }

        public SettingEntry<string> DrfApiToken { get; }
    }
}