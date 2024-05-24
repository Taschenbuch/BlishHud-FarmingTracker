﻿using Blish_HUD;
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

            RarityIconBorderIsVisibleSetting = settings.DefineSetting(
                "rarity icon border is visible",
                true,
                () => "rarity colored icon border",
                () => "Show a border in rarity color around item icons.");

            CountColorSetting = settings.DefineSetting(
               "count color",
               ColorType.White,
               () => "count color",
               () => "Change item/currency count font color.");

            CountFontSizeSetting = settings.DefineSetting(
               "count font size",
               ContentService.FontSize.Size20,
               () => "count font size",
               () => "Change item/currency count font size.");

            var internalSettings = settings.AddSubCollection("internal settings (not visible in UI)");
            // sort
            SortByWithDirectionListSetting = internalSettings.DefineSetting("sort by list", new List<SortByWithDirection>(new[] { SortByWithDirection.PositiveAndNegativeCount_Descending, SortByWithDirection.ApiId_Ascending }));
            // filter
            CountFilterSetting = internalSettings.DefineSetting("count stat filter", new List<CountFilter>(Constants.ALL_COUNTS));
            SellMethodFilterSetting = internalSettings.DefineSetting("sellable item filter", new List<SellMethodFilter>(Constants.ALL_SELL_METHODS));
            RarityStatsFilterSetting = internalSettings.DefineSetting("rarity item filter", new List<ItemRarity>(Constants.ALL_ITEM_RARITIES));
            TypeStatsFilterSetting = internalSettings.DefineSetting("type item filter", new List<ItemType>(Constants.ALL_ITEM_TYPES));
            FlagStatsFilterSetting = internalSettings.DefineSetting("flag item filter", new List<ItemFlag>(Constants.ALL_ITEM_FLAGS));
            CurrencyFilterSetting = internalSettings.DefineSetting("currency filter", new List<CurrencyFilter>(Constants.ALL_CURRENCIES));

            // prevents that there are more selected filterElements than total filterElements = checkboxes. Otherwise filter icon may always say list is filtered.
            RemoveUnknownEnumValues(SortByWithDirectionListSetting);
            RemoveUnknownEnumValues(CountFilterSetting);
            RemoveUnknownEnumValues(SellMethodFilterSetting);
            RemoveUnknownEnumValues(RarityStatsFilterSetting);
            RemoveUnknownEnumValues(TypeStatsFilterSetting);
            RemoveUnknownEnumValues(FlagStatsFilterSetting);
            RemoveUnknownEnumValues(CurrencyFilterSetting);
        }

        private static void RemoveUnknownEnumValues<T>(SettingEntry<List<T>> ListSetting) where T : System.Enum
        {
            var elements = new List<T>(ListSetting.Value); // otherwise foreach wont work

            foreach (var element in elements)
                if (FilterService.IsUnknownFilterElement<T>((int)(object)element))
                    ListSetting.Value.Remove(element);
        }

        public SettingEntry<string> DrfTokenSetting { get; }
        public SettingEntry<KeyBinding> WindowVisibilityKeyBindingSetting { get; }
        public SettingEntry<bool> RarityIconBorderIsVisibleSetting { get; }
        public SettingEntry<ColorType> CountColorSetting { get; }
        public SettingEntry<ContentService.FontSize> CountFontSizeSetting { get; }
        public SettingEntry<List<SortByWithDirection>> SortByWithDirectionListSetting { get; }
        public SettingEntry<List<CountFilter>> CountFilterSetting { get; }
        public SettingEntry<List<SellMethodFilter>> SellMethodFilterSetting { get; }
        public SettingEntry<List<ItemRarity>> RarityStatsFilterSetting { get; }
        public SettingEntry<List<ItemType>> TypeStatsFilterSetting { get; }
        public SettingEntry<List<ItemFlag>> FlagStatsFilterSetting { get; }
        public SettingEntry<List<CurrencyFilter>> CurrencyFilterSetting { get; }
    }
}