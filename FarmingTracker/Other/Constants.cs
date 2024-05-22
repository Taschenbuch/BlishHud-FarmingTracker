using Microsoft.Xna.Framework;
using System;
using Gw2SharpType = Gw2Sharp.WebApi.V2.Models;

namespace FarmingTracker
{
    public class Constants
    {
        public const string EMPTY_LABEL = " "; // blank required to have at least 1 char so that label height does not change when set to something else
        public const int PROFIT_PER_HOUR_UPDATE_INTERVAL_IN_SECONDS = 5;
        // enum lists
        public static Gw2SharpType.ItemRarity[] ALL_ITEM_RARITIES => (Gw2SharpType.ItemRarity[])Enum.GetValues(typeof(Gw2SharpType.ItemRarity));
        public static SellMethodFilter[] ALL_SELL_METHODS => (SellMethodFilter[])Enum.GetValues(typeof(SellMethodFilter));
        public static CountFilter[] ALL_COUNTS => (CountFilter[])Enum.GetValues(typeof(CountFilter));
        public static Gw2SharpType.ItemType[] ALL_ITEM_TYPES => (Gw2SharpType.ItemType[])Enum.GetValues(typeof(Gw2SharpType.ItemType));
        public static Gw2SharpType.ItemFlag[] ALL_ITEM_FLAGS => (Gw2SharpType.ItemFlag[])Enum.GetValues(typeof(Gw2SharpType.ItemFlag));
        public static CurrencyFilter[] ALL_CURRENCIES => (CurrencyFilter[])Enum.GetValues(typeof(CurrencyFilter));
        // colors
        public static Color JunkColor = new Color(113, 97, 95);
        public static Color BasicColor = new Color(113, 97, 95);
        public static Color FineColor = new Color(85, 153, 255);
        public static Color MasterworkColor = new Color(51, 204, 17);
        public static Color RareColor = new Color(255, 221, 34);
        public static Color ExoticColor = new Color(255, 170, 0);
        public static Color AscendedColor = new Color(255, 68, 136);
        public static Color LegendaryColor = new Color(153, 51, 255);
    }
}
