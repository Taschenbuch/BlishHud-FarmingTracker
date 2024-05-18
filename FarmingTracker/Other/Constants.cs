using Gw2Sharp.WebApi.V2.Models;
using System;

namespace FarmingTracker
{
    public class Constants
    {
        public const string EMPTY_LABEL = " "; // blank required to have at least 1 char so that label height does not change when set to something else
        public const int GOLD_ICON_ASSET_ID = 156904;
        public const int SILVER_ICON_ASSET_ID = 156907;
        public const int COPPER_ICON_ASSET_ID = 156902;
        public const int BUG_TEXTURE_ASSET_ID = 157084;
        public const int PROFIT_PER_HOUR_UPDATE_INTERVAL_IN_SECONDS = 5;
        public static ItemRarity[] ALL_ITEM_RARITIES => (ItemRarity[])Enum.GetValues(typeof(ItemRarity));
        public static SellMethodFilter[] ALL_SELL_METHODS => (SellMethodFilter[])Enum.GetValues(typeof(SellMethodFilter));
        public static CountFilter[] ALL_COUNTS => (CountFilter[])Enum.GetValues(typeof(CountFilter));
        public static ItemType[] ALL_ITEM_TYPES => (ItemType[])Enum.GetValues(typeof(ItemType));
        public static ItemFlag[] ALL_ITEM_FLAGS => (ItemFlag[])Enum.GetValues(typeof(ItemFlag));
        public static CurrencyFilter[] ALL_CURRENCIES => (CurrencyFilter[])Enum.GetValues(typeof(CurrencyFilter));
    }
}
