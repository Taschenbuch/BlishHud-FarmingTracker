using Blish_HUD.Input;
using Blish_HUD.Settings;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace FarmingTracker
{
    public class SettingService // singular because Setting"s"Service already exists in Blish
    {
        public SettingService(SettingCollection settings)
        {
            DrfTokenSetting = settings.DefineSetting(
                "drf token",
                string.Empty,
                () => "DRF Token/Key", 
                () => "Create an account on https://drf.rs/ and link it with your GW2 Account(s).\n" +
                "Open the drf.rs settings page. Click on 'Regenerate Token'.\n" +
                "Copy the token. Paste the token with CTRL + V into this input.");

            WindowVisibilityKeyBindingSetting = settings.DefineSetting(
                "window visibility key binding",
                new KeyBinding(ModifierKeys.Ctrl | ModifierKeys.Alt | ModifierKeys.Shift, Keys.F),
                () => "show/hide window",
                () => "Double-click to change the key binding. Will show or hide the farming tracker window.");

            var internalSettings = settings.AddSubCollection("internal settings (not visible in UI)");
            RarityStatsFilterSetting = internalSettings.DefineSetting("rarity item filter", new List<ItemRarity>(Constants.ALL_ITEM_RARITIES));
            TypeStatsFilterSetting = internalSettings.DefineSetting("type item filter", new List<ItemType>(Constants.ALL_ITEM_TYPES));
            FlagStatsFilterSetting = internalSettings.DefineSetting("flag item filter", new List<ItemFlag>(Constants.ALL_ITEM_FLAGS));
        }

        public SettingEntry<string> DrfTokenSetting { get; }
        public SettingEntry<KeyBinding> WindowVisibilityKeyBindingSetting { get; }
        public SettingEntry<List<ItemRarity>> RarityStatsFilterSetting { get; }
        public SettingEntry<List<ItemType>> TypeStatsFilterSetting { get; }
        public SettingEntry<List<ItemFlag>> FlagStatsFilterSetting { get; }
    }
}