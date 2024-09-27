using Blish_HUD.Settings;
using System.Collections.Generic;

namespace FarmingTracker
{
    public static class MigrateBlishSettingsService
    {
        // Changelog:
        // version 2
        // - added UrsusOblige to CurrencyFilter
        // - added CustomProfitIsSet to SellMethodFilter
        public static void MigrateSettings(
            SettingEntry<int> settingsVersionSetting,
            SettingEntry<List<CurrencyFilter>> currencyFilterSetting,
            SettingEntry<List<SellMethodFilter>> sellMethodFilterSetting)
        {
            var hasToMigrateFrom1To2 = settingsVersionSetting.Value == 1;
            if (hasToMigrateFrom1To2)
            {
                currencyFilterSetting.Value.Add(CurrencyFilter.UrsusOblige);
                sellMethodFilterSetting.Value.Add(SellMethodFilter.CustomProfitIsSet);
                settingsVersionSetting.Value = 2;
                Module.Logger.Info($"Migrated blish settings version 1 to 2.");
            }
        }
    }
}
