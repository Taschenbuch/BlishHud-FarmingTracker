using Microsoft.Xna.Framework;
using Gw2SharpType = Gw2Sharp.WebApi.V2.Models;

namespace FarmingTracker
{
    public static class ColorService
    {
        public static Color GetRarityBorderColor(Gw2SharpType.ItemRarity rarity)
        {
            return rarity switch
            {
                Gw2SharpType.ItemRarity.Junk => Color.Transparent,
                Gw2SharpType.ItemRarity.Basic => Color.Transparent,
                Gw2SharpType.ItemRarity.Fine => FineColor,
                Gw2SharpType.ItemRarity.Masterwork => MasterworkColor,
                Gw2SharpType.ItemRarity.Rare => RareColor,
                Gw2SharpType.ItemRarity.Exotic => ExoticColor,
                Gw2SharpType.ItemRarity.Ascended => AscendedColor,
                Gw2SharpType.ItemRarity.Legendary => LegendaryColor,
                _ => Color.Transparent, // includes .Unknown which match currencies and items not known by api
            };
        }

        public static Color GetRarityTextColor(Gw2SharpType.ItemRarity rarity)
        {
            return rarity switch
            {
                Gw2SharpType.ItemRarity.Junk => JunkColor,
                Gw2SharpType.ItemRarity.Basic => Color.White,
                Gw2SharpType.ItemRarity.Fine => FineColor,
                Gw2SharpType.ItemRarity.Masterwork => MasterworkColor,
                Gw2SharpType.ItemRarity.Rare => RareColor,
                Gw2SharpType.ItemRarity.Exotic => ExoticColor,
                Gw2SharpType.ItemRarity.Ascended => AscendedColor,
                Gw2SharpType.ItemRarity.Legendary => LegendaryColor,
                _ => Color.White, // includes .Unknown which match currencies and items not known by api
            };
        }

        // colors
        public static Color JunkColor = new Color(170, 170, 170);
        public static Color FineColor = new Color(85, 153, 255);
        public static Color MasterworkColor = new Color(51, 204, 17);
        public static Color RareColor = new Color(255, 221, 34);
        public static Color ExoticColor = new Color(255, 170, 0);
        public static Color AscendedColor = new Color(255, 68, 136);
        public static Color LegendaryColor = new Color(153, 51, 255);
    }
}
