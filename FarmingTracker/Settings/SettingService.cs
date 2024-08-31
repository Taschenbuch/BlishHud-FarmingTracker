using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Blish_HUD.Settings;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework.Input;
using System;
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
            NextResetDateTimeUtcSetting = internalSettings.DefineSetting("next reset dateTimeUtc", NextAutomaticResetCalculator.UNDEFINED_RESET_DATE_TIME);
            FarmingDurationTimeSpanSetting = internalSettings.DefineSetting("farming duration time span", TimeSpan.Zero);

            DefineProfitWindowSettings(settings, internalSettings);

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
                false,
                () => $"mouse clickthrough (disabled when '{DRAG_WITH_MOUSE_LABEL_TEXT}' is checked)",
                () => "This allows clicking with the mouse through the window to interact with Guild Wars 2 behind the window.");

            ProfitWindowBackgroundOpacitySetting = settings.DefineSetting(
                "profit window background opacity",
                125,
                () => "background opacity",
                () => "Change background opacity / transparency.");
            ProfitWindowBackgroundOpacitySetting.SetRange(0, 255);

            ProfitPerHourLabelTextSetting = settings.DefineSetting(
               "profit per hour label text",
               "Profit per hour",
               () => "profit per hour label",
               () => "Change the label for profit per hour. This will affect profit display in profit window and summary tab. " +
                     "You have to click somewhere outside of this text input to see your change. " +
                     "This will not affect the value itself.");

            TotalProfitLabelTextSetting = settings.DefineSetting(
              "total profit label text",
              "Profit",
              () => "total profit label",
              () => "Change the label for total profit. This will affect profit display in profit window and summary tab. " +
                    "You have to click somewhere outside of this text input to see your change. " +
                    "This will not affect the value itself.");

            ProfitWindowRelativeWindowAnchorLocationSetting = internalSettings.DefineSetting("profit window relative location", new FloatPoint(0.2f, 0.2f));
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
        public SettingEntry<int> MinutesUntilResetAfterModuleShutdownSetting { get; }
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
        public SettingEntry<DateTime> NextResetDateTimeUtcSetting { get; }
        public SettingEntry<TimeSpan> FarmingDurationTimeSpanSetting { get; }
        public SettingEntry<List<SortByWithDirection>> SortByWithDirectionListSetting { get; }
        public SettingEntry<List<CountFilter>> CountFilterSetting { get; }
        public SettingEntry<List<SellMethodFilter>> SellMethodFilterSetting { get; }
        public SettingEntry<List<ItemRarity>> RarityStatsFilterSetting { get; }
        public SettingEntry<List<ItemType>> TypeStatsFilterSetting { get; }
        public SettingEntry<List<ItemFlag>> FlagStatsFilterSetting { get; }
        public SettingEntry<List<CurrencyFilter>> CurrencyFilterSetting { get; }
        public SettingEntry<List<KnownByApiFilter>> KnownByApiFilterSetting { get; }
        // profit window settings
        public SettingEntry<bool> IsProfitWindowVisibleSetting { get; private set; }
        public SettingEntry<bool> DragProfitWindowWithMouseIsEnabledSetting { get; private set; }
        public SettingEntry<WindowAnchor> WindowAnchorSetting { get; private set; }
        public SettingEntry<bool> ProfitWindowCanBeClickedThroughSetting { get; private set; }
        public SettingEntry<int> ProfitWindowBackgroundOpacitySetting { get; private set; }
        public SettingEntry<string> ProfitPerHourLabelTextSetting { get; private set; }
        public SettingEntry<string> TotalProfitLabelTextSetting { get; private set; }
        public SettingEntry<FloatPoint> ProfitWindowRelativeWindowAnchorLocationSetting { get; private set; }

        private const string DRAG_WITH_MOUSE_LABEL_TEXT = "drag with mouse";
    }
}