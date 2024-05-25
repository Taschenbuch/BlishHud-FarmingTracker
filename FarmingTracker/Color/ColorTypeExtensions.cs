using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace FarmingTracker
{
    public static class ColorTypeExtensions
    {
        public static Color GetColor(this ColorType colorType)
        {
            return ColorByColorType[colorType];
        }

        public static Dictionary<ColorType, Color> ColorByColorType = new Dictionary<ColorType, Color>
        {
            [ColorType.White] = Color.White,
            [ColorType.Black] = Color.Black,
            [ColorType.LightGray] = Color.LightGray,
            [ColorType.Gray] = Color.Gray, // Gray is darker than DarkGray... very funny
            [ColorType.DimGray] = Color.DimGray,
            [ColorType.Lime] = Color.Lime,
            [ColorType.LightGreen] = Color.LightGreen,
            [ColorType.Green] = Color.Green,
            [ColorType.DarkGreen] = Color.DarkGreen,
            [ColorType.Red] = Color.Red,
            [ColorType.DarkRed] = Color.DarkRed,
            [ColorType.Cyan] = Color.Cyan,
            [ColorType.LightBlue] = Color.LightBlue,
            [ColorType.Blue] = Color.Blue,
            [ColorType.DarkBlue] = Color.DarkBlue,
            [ColorType.Beige] = Color.Beige,
            [ColorType.Orange] = Color.Orange,
            [ColorType.Gold] = Color.Gold,
            [ColorType.Brown] = Color.Brown,
            [ColorType.Yellow] = Color.Yellow,
            [ColorType.Magenta] = Color.Magenta,
            [ColorType.Violet] = Color.Violet,
            [ColorType.Purple] = Color.Purple,
        };
    }
}