using Blish_HUD.Controls;

namespace FarmingTracker
{
    public class FixedWidthHintLabel : Label
    {
        public FixedWidthHintLabel(Container parent, int width, string text) // parent first because text might be long and hides parent then.
        {
            Text = text;
            WrapText = true;
            Width = width;
            AutoSizeHeight = true;
            Parent = parent;
        }
    }
}