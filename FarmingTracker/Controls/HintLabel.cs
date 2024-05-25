using Blish_HUD.Controls;

namespace FarmingTracker
{
    public class HintLabel : Label
    {
        public HintLabel(Container parent, string text) // parent first because text might be long and hides parent then.
        {
            Text = text;
            AutoSizeHeight = true;
            AutoSizeWidth = true;
            Parent = parent;
        }
    }
}