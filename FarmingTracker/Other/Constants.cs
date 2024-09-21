using System;
using Gw2SharpType = Gw2Sharp.WebApi.V2.Models;

namespace FarmingTracker
{
    public class Constants
    {
        public const int PANEL_WIDTH = 500; // prevents overlap with window scrollbar
        public const int LABEL_WIDTH = PANEL_WIDTH - 20;
        public const int SCROLLBAR_WIDTH_OFFSET = 30;
        public const string DRF_CONNECTION_LABEL_TEXT = "DRF Server Connection";
        public const string UPDATING_HINT_TEXT = "Updating...";
        public const string RESETTING_HINT_TEXT = "Resetting...";
        public const string GW2_API_ERROR_HINT = "GW2 API error";
        public const string FAVORITE_ITEMS_PANEL_TITLE = "Favorite Items";
        public const string ITEMS_PANEL_TITLE = "Items";
        public const string CURRENCIES_PANEL_TITLE = "Currencies";
        public const string FULL_HEIGHT_EMPTY_LABEL = " "; // blank required to have at least 1 char so that label height does not change when set to something else
        public const string ZERO_HEIGHT_EMPTY_LABEL = ""; // no text -> label autoheight will be 0.
        public const string HINT_IN_PANEL_PADDING = "  ";
        public const int PROFIT_PER_HOUR_UPDATE_INTERVAL_IN_SECONDS = 5;
        // enum lists
        public static Gw2SharpType.ItemRarity[] ALL_ITEM_RARITIES => (Gw2SharpType.ItemRarity[])Enum.GetValues(typeof(Gw2SharpType.ItemRarity));
        public static SellMethodFilter[] ALL_SELL_METHODS => (SellMethodFilter[])Enum.GetValues(typeof(SellMethodFilter));
        public static CountFilter[] ALL_COUNTS => (CountFilter[])Enum.GetValues(typeof(CountFilter));
        public static KnownByApiFilter[] ALL_KNOWN_BY_API => (KnownByApiFilter[])Enum.GetValues(typeof(KnownByApiFilter));
        public static Gw2SharpType.ItemType[] ALL_ITEM_TYPES => (Gw2SharpType.ItemType[])Enum.GetValues(typeof(Gw2SharpType.ItemType));
        public static Gw2SharpType.ItemFlag[] ALL_ITEM_FLAGS => (Gw2SharpType.ItemFlag[])Enum.GetValues(typeof(Gw2SharpType.ItemFlag));
        public static CurrencyFilter[] ALL_CURRENCIES => (CurrencyFilter[])Enum.GetValues(typeof(CurrencyFilter));
    }
}
