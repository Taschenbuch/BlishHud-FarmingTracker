using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using MonoGame.Extended.BitmapFonts;

namespace FarmingTracker
{
    public class HeaderLabel : Label
    {
        public HeaderLabel(Container parent, string text, BitmapFont font) // parent first because text might be long and hides parent then.
        {
            Text = text;
            TextColor = Color.DeepSkyBlue;
            Font = font;
            AutoSizeHeight = true;
            AutoSizeWidth = true;
            Parent = parent;
        }
    }
}