using Blish_HUD.Controls;

namespace FarmingTracker
{
    public class ControlFactory
    {
        public static Label CreateHintLabel(Container parent, string text) // parent first because text might be long and hides parent then.
        {
            return new Label
            {
                Text           = text,
                AutoSizeHeight = true,
                AutoSizeWidth  = true,
                Parent         = parent
            };
        }
    }
}