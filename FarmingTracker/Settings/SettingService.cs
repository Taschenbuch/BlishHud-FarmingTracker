﻿using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Blish_HUD.Settings;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

#nullable disable
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

            MinutesUntilResetAfterModuleShutdownSetting = settings.DefineSetting(
                "number of minutes until automatic reset after module shutdown",
                30,
                () => "minutes until automatic reset after module shutdown",
                () => "Change number of minutes the module will wait after module shutdown before doing an automatic reset. " +
                "This setting can be usefull to not lose tracking progress due to PC / gw2 / blish crashes, " +
                $"but still have automatic resets similar to 'On module start' option. Or if you prefer to play in short sessions spread throughout the day");

            WindowVisibilityKeyBindingSetting = settings.DefineSetting(
                "window visibility key binding",
                new KeyBinding(ModifierKeys.Ctrl | ModifierKeys.Alt | ModifierKeys.Shift, Keys.F),
                () => "show/hide window",
                () => "Double-click to change the key binding. Will show or hide the farming tracker window.");

            IsFakeDrfServerUsedSetting = settings.DefineSetting(
                "is fake drf server used",
                false,
                () => "fake drf server",
                () => "fake drf server");

            var internalSettings = DefineHiddenSettings(settings);
            DefineCountSettings(settings);
            DefineIconSettings(settings);
            DefineProfitSettings(settings);
            DefineProfitWindowSettings(settings, internalSettings);
            DefineSortAndFilterSettings(internalSettings);
            MigrateBlishSettingsService.MigrateSettings(SettingsVersionSetting, CurrencyFilterSetting, SellMethodFilterSetting);
        }

        private void DefineSortAndFilterSettings(SettingCollection internalSettings)
        {
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


        private SettingCollection DefineHiddenSettings(SettingCollection settings)
        {
            var internalSettings = settings.AddSubCollection("internal settings (not visible in UI)");
            
            var settingsVersionSetting = internalSettings[SETTINGS_VERSION_SETTING_KEY]; 
            var isFirstVersionWhichHadNoSettingsVersionSetting = settingsVersionSetting == null;

            SettingsVersionSetting = internalSettings.DefineSetting(SETTINGS_VERSION_SETTING_KEY, 2); 
            
            if (isFirstVersionWhichHadNoSettingsVersionSetting)
                SettingsVersionSetting.Value = 1; 

            NextResetDateTimeUtcSetting = internalSettings.DefineSetting("next reset dateTimeUtc", NextAutomaticResetCalculator.UNDEFINED_RESET_DATE_TIME);
            FarmingDurationTimeSpanSetting = internalSettings.DefineSetting("farming duration time span", TimeSpan.Zero);
            return internalSettings;
        }

        private void DefineIconSettings(SettingCollection settings)
        {
            RarityIconBorderIsVisibleSetting = settings.DefineSetting(
                "rarity icon border is visible",
                true,
                () => "rarity colored border",
                () => "Show a border in rarity color around item icons.");

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
        }

        private void DefineCountSettings(SettingCollection settings)
        {
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
        }

        private void DefineProfitWindowSettings(SettingCollection settings, SettingCollection internalSettings)
        {
            IsProfitWindowVisibleSetting = settings.DefineSetting(
                "profit window is visible",
                true,
                () => "show",
                () => "Show profit window.");

            DragProfitWindowWithMouseIsEnabledSetting = settings.DefineSetting(
                "dragging profit window is allowed",
                true,
                () => DRAG_WITH_MOUSE_LABEL_TEXT,
                () => "Allow dragging the window by moving the mouse when left mouse button is pressed inside window");

            WindowAnchorSetting = settings.DefineSetting(
               "window anchor",
               WindowAnchor.TopLeft,
               () => "window anchor",
               () => "When the window content grows/shrinks the window anchor is the part of the window that will not move, while the rest of the window will. " +
               "For example for 'Top..,' the window top will stay in position while the window bottom expands. " +
               "And for 'Bottom..,' the window bottom will stay in position while the window top expands.");

            ProfitWindowCanBeClickedThroughSetting = settings.DefineSetting(
                "profit window is not capturing mouse clicks",
                true,
                () => $"mouse clickthrough (disabled when '{DRAG_WITH_MOUSE_LABEL_TEXT}' is checked)",
                () => "This allows clicking with the mouse through the window to interact with Guild Wars 2 behind the window.");

            ProfitWindowBackgroundOpacitySetting = settings.DefineSetting(
                "profit window background opacity",
                125,
                () => "background opacity",
                () => "Change background opacity / transparency.");
            ProfitWindowBackgroundOpacitySetting.SetRange(0, 255);

            ProfitWindowDisplayModeSetting = settings.DefineSetting(
                "profit window display mode",
                ProfitWindowDisplayMode.ProfitAndProfitPerHour,
                () => "display mode",
                () => "Change what should be displayed inside the profit window.");

            ProfitWindowRelativeWindowAnchorLocationSetting = internalSettings.DefineSetting("profit window relative location", new FloatPoint(0.2f, 0.2f));
        }

        private void DefineProfitSettings(SettingCollection settings)
        {
            ProfitPerHourLabelTextSetting = settings.DefineSetting(
                "profit per hour label text",
                "Profit per hour",
                () => "profit per hour label",
                () => "Change the label for profit per hour. This will affect profit display in profit window and summary tab. " +
                        "You have to click somewhere outside of this text input to see your change. " +
                        "This will not affect the value itself.");

            ProfitLabelTextSetting = settings.DefineSetting(
              "total profit label text",
              "Profit",
              () => "profit label",
              () => "Change the label for total profit. This will affect profit display in profit window and summary tab. " +
                    "You have to click somewhere outside of this text input to see your change. " +
                    "This will not affect the value itself.");
        }

        // prevents that there are more selected filterElements than total filterElements = checkboxes. Otherwise filter icon may always say list is filtered.
        private static void RemoveUnknownEnumValues<T>(SettingEntry<List<T>> ListSetting) where T : Enum
        {
            var values = new List<T>(ListSetting.Value); // otherwise foreach wont work
            var unknownValues = values.Where(e => Helper.IsUnknownEnumValue<T>((int)(object)e));

            foreach (var unknownValue in unknownValues)
                ListSetting.Value.Remove(unknownValue);
        }

        public SettingEntry<string> DrfTokenSetting { get; }
        public SettingEntry<AutomaticReset> AutomaticResetSetting { get; }
        public SettingEntry<int> MinutesUntilResetAfterModuleShutdownSetting { get; }
        public SettingEntry<KeyBinding> WindowVisibilityKeyBindingSetting { get; }
        public SettingEntry<bool> IsFakeDrfServerUsedSetting { get; }

        // hidden settings
        public SettingEntry<int> SettingsVersionSetting { get; private set; }
        public SettingEntry<DateTime> NextResetDateTimeUtcSetting { get; private set; }
        public SettingEntry<TimeSpan> FarmingDurationTimeSpanSetting { get; private set; }

        // sort and filter settings
        public SettingEntry<List<SortByWithDirection>> SortByWithDirectionListSetting { get; private set; }
        public SettingEntry<List<CountFilter>> CountFilterSetting { get; private set; }
        public SettingEntry<List<SellMethodFilter>> SellMethodFilterSetting { get; private set; }
        public SettingEntry<List<ItemRarity>> RarityStatsFilterSetting { get; private set; }
        public SettingEntry<List<ItemType>> TypeStatsFilterSetting { get; private set; }
        public SettingEntry<List<ItemFlag>> FlagStatsFilterSetting { get; private set; }
        public SettingEntry<List<CurrencyFilter>> CurrencyFilterSetting { get; private set; }
        public SettingEntry<List<KnownByApiFilter>> KnownByApiFilterSetting { get; private set; }

        // icon settings
        public SettingEntry<bool> RarityIconBorderIsVisibleSetting { get; private set; }
        public SettingEntry<int> NegativeCountIconOpacitySetting { get; private set; }
        public SettingEntry<StatIconSize> StatIconSizeSetting { get; private set; }

        // count settings
        public SettingEntry<int> CountBackgroundOpacitySetting { get; private set; }
        public SettingEntry<ColorType> CountBackgroundColorSetting { get; private set; }
        public SettingEntry<ColorType> PositiveCountTextColorSetting { get; private set; }
        public SettingEntry<ColorType> NegativeCountTextColorSetting { get; private set; }
        public SettingEntry<ContentService.FontSize> CountFontSizeSetting { get; private set; }
        public SettingEntry<HorizontalAlignment> CountHoritzontalAlignmentSetting { get; private set; }
        
        // profit window settings
        public SettingEntry<bool> IsProfitWindowVisibleSetting { get; private set; }
        public SettingEntry<bool> DragProfitWindowWithMouseIsEnabledSetting { get; private set; }
        public SettingEntry<WindowAnchor> WindowAnchorSetting { get; private set; }
        public SettingEntry<bool> ProfitWindowCanBeClickedThroughSetting { get; private set; }
        public SettingEntry<int> ProfitWindowBackgroundOpacitySetting { get; private set; }
        public SettingEntry<string> ProfitPerHourLabelTextSetting { get; private set; }
        public SettingEntry<string> ProfitLabelTextSetting { get; private set; }
        public SettingEntry<ProfitWindowDisplayMode> ProfitWindowDisplayModeSetting { get; private set; }
        public SettingEntry<FloatPoint> ProfitWindowRelativeWindowAnchorLocationSetting { get; private set; }

        private const string DRAG_WITH_MOUSE_LABEL_TEXT = "drag with mouse";
        private const string SETTINGS_VERSION_SETTING_KEY = "settings version";
    }
}
#nullable enable