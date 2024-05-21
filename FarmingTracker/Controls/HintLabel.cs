using Blish_HUD.Controls;
using MonoGame.Extended.BitmapFonts;

namespace FarmingTracker
{
    public class HintLabel : Label
    {
        public HintLabel(Container parent, string text, BitmapFont font = null) // parent first because text might be long and hides parent then.
        {
            Text = text;
            AutoSizeHeight = true;
            AutoSizeWidth = true;
            Parent = parent;

            if(font != null)
                Font = font;
        }
    }
}