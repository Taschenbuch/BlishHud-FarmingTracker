using Blish_HUD.Controls;
using MonoGame.Extended.BitmapFonts;

namespace FarmingTracker
{
    public class ControlFactory
    {
        public static Label CreateHintLabel(Container parent, string text, BitmapFont font = null) // parent first because text might be long and hides parent then.
        {
            var label = new Label
            {
                Text           = text,
                AutoSizeHeight = true,
                AutoSizeWidth  = true,
                Parent         = parent
            };

            if(font != null)
                label.Font = font;

            return label;
        }
    }
}