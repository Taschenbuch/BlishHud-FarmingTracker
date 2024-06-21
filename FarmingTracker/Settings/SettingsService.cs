using Blish_HUD;
using Blish_HUD.Controls;
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

            AutomaticResetSetting = settings.DefineSetting(
                "automatic reset",
                AutomaticReset.OnModuleStart,
                () => "automatic reset",
                () => "Change when all farmed items are reset. 'Never' means you have to use the 'Reset' button.\n" +
                "Using something else than 'on module start' helps to not lose your farming data when gw2, blish or your pc crashes.");

            WindowVisibilityKeyBindingSetting = settings.DefineSetting(
                "window visibility key binding",
                new KeyBinding(ModifierKeys.Ctrl | ModifierKeys.Alt | ModifierKeys.Shift, Keys.F),
                () => "show/hide window",
                () => "Double-click to change the key binding. Will show or hide the farming tracker window.");

            RarityIconBorderIsVisibleSetting = settings.DefineSetting(
                "rarity icon border is visible",
                true,
                () => "rarity colored border",
                () => "Show a border in rarity color around item icons.");

            CountBackgroundColorSetting = settings.DefineSetting(
                "count background color",
                ColorType.Black,
                () => "background color",
                () => "Change item/currency count background color. It is not visible when count background opacity slider is set to full transparency.");

            CountBackgroundOpacitySetting = settings.DefineSetting(
                "count background opacity",
                0,
                () => "background opacity",
                () => "Change item/currency count background opacity / transparency.");
            
            CountBackgroundOpacitySetting.SetRange(0, 255);

            PositiveCountTextColorSetting = settings.DefineSetting(
               "positive count text color",
               ColorType.White,
               () => "positive text color",
               () => "Change item/currency count text color for positive counts (>0).");

            NegativeCountTextColorSetting = settings.DefineSetting(
               "negative count text color",
               ColorType.White,
               () => "negative text color",
               () => "Change item/currency count text color for negative counts (<0).");

            CountFontSizeSetting = settings.DefineSetting(
               "count font size",
               ContentService.FontSize.Size20,
               () => "text size",
               () => "Change item/currency count font size.");

            CountHoritzontalAlignmentSetting = settings.DefineSetting(
               "count horizontal alignment",
               HorizontalAlignment.Right,
               () => "horizontal alignment",
               () => "Change item/currency count horizontal alignment. Dont use 'center'. It can cut off at both ends.");

            StatIconSizeSetting = settings.DefineSetting(
                "stat icon size",
                StatIconSize.M,
                () => "size",
                () => "Change item/currency icon size.");

            NegativeCountIconOpacitySetting = settings.DefineSetting(
                "negative count icon opacity",
                77,
                () => "negative count opacity",
                () => "Change item/currency icon opacity / transparency for negative counts.");

            NegativeCountIconOpacitySetting.SetRange(0, 255);

            IsFakeDrfServerUsedSetting = settings.DefineSetting(
                "is fake drf server used",
                false,
                () => "fake drf server",
                () => "fake drf server");

            var internalSettings = settings.AddSubCollection("internal settings (not visible in UI)");
            // sort
            SortByWithDirectionListSetting = internalSettings.DefineSetting("sort by list", new List<SortByWithDirection>(new[] { SortByWithDirection.PositiveAndNegativeCount_Descending, SortByWithDirection.ApiId_Ascending }));
            RemoveUnknownEnumValues(SortByWithDirectionListSetting);
            // filter
            CountFilterSetting = internalSettings.DefineSetting("count stat filter", new List<CountFilter>(Constants.ALL_COUNTS));
            RemoveUnknownEnumValues(CountFilterSetting);
            
            SellMethodFilterSetting = internalSettings.DefineSetting("sellable item filter", new List<SellMethodFilter>(Constants.ALL_SELL_METHODS));
            RemoveUnknownEnumValues(SellMethodFilterSetting);
            
            RarityStatsFilterSetting = internalSettings.DefineSetting("rarity item filter", new List<ItemRarity>(Constants.ALL_ITEM_RARITIES));
            RemoveUnknownEnumValues(RarityStatsFilterSetting);
            
            TypeStatsFilterSetting = internalSettings.DefineSetting("type item filter", new List<ItemType>(Constants.ALL_ITEM_TYPES));
            RemoveUnknownEnumValues(TypeStatsFilterSetting);
            
            FlagStatsFilterSetting = internalSettings.DefineSetting("flag item filter", new List<ItemFlag>(Constants.ALL_ITEM_FLAGS));
            RemoveUnknownEnumValues(FlagStatsFilterSetting);
            
            CurrencyFilterSetting = internalSettings.DefineSetting("currency filter", new List<CurrencyFilter>(Constants.ALL_CURRENCIES));
            RemoveUnknownEnumValues(CurrencyFilterSetting);
            
            KnownByApiFilterSetting = internalSettings.DefineSetting("known by api filter", new List<KnownByApiFilter>(Constants.ALL_KNOWN_BY_API));
            RemoveUnknownEnumValues(KnownByApiFilterSetting);
        }

        // prevents that there are more selected filterElements than total filterElements = checkboxes. Otherwise filter icon may always say list is filtered.
        private static void RemoveUnknownEnumValues<T>(SettingEntry<List<T>> ListSetting) where T : System.Enum
        {
            var elements = new List<T>(ListSetting.Value); // otherwise foreach wont work

            foreach (var element in elements)
                if (FilterService.IsUnknownFilterElement<T>((int)(object)element))
                    ListSetting.Value.Remove(element);
        }

        public SettingEntry<string> DrfTokenSetting { get; }
        public SettingEntry<AutomaticReset> AutomaticResetSetting { get; }
        public SettingEntry<KeyBinding> WindowVisibilityKeyBindingSetting { get; }
        public SettingEntry<bool> RarityIconBorderIsVisibleSetting { get; }
        public SettingEntry<ColorType> PositiveCountTextColorSetting { get; }
        public SettingEntry<ColorType> NegativeCountTextColorSetting { get; }
        public SettingEntry<ColorType> CountBackgroundColorSetting { get; }
        public SettingEntry<int> CountBackgroundOpacitySetting { get; }
        public SettingEntry<ContentService.FontSize> CountFontSizeSetting { get; }
        public SettingEntry<HorizontalAlignment> CountHoritzontalAlignmentSetting { get; }
        public SettingEntry<StatIconSize> StatIconSizeSetting { get; }
        public SettingEntry<int> NegativeCountIconOpacitySetting { get; }
        public SettingEntry<bool> IsFakeDrfServerUsedSetting { get; }
        public SettingEntry<List<SortByWithDirection>> SortByWithDirectionListSetting { get; }
        public SettingEntry<List<CountFilter>> CountFilterSetting { get; }
        public SettingEntry<List<SellMethodFilter>> SellMethodFilterSetting { get; }
        public SettingEntry<List<ItemRarity>> RarityStatsFilterSetting { get; }
        public SettingEntry<List<ItemType>> TypeStatsFilterSetting { get; }
        public SettingEntry<List<ItemFlag>> FlagStatsFilterSetting { get; }
        public SettingEntry<List<CurrencyFilter>> CurrencyFilterSetting { get; }
        public SettingEntry<List<KnownByApiFilter>> KnownByApiFilterSetting { get; }
    }
}