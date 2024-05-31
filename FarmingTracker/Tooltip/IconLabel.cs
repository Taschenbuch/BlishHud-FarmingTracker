using Blish_HUD.Content;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using MonoGame.Extended.BitmapFonts;

namespace FarmingTracker
{
    public class IconLabel : Container
    {
        public IconLabel(string text, AsyncTexture2D texture, int height, BitmapFont font, Container parent)
        {
            HeightSizingMode = SizingMode.AutoSize;
            Width = 75;
            Parent = parent;

            new Image(texture)
            {
                Size = new Point(height),
                Parent = this,
            };

            new Label
            {
                Text = text,
                Font = font,
                AutoSizeWidth = true,
                Height = height,
                Left = height + 5,
                Parent = this,
            };
        }
    }
}
