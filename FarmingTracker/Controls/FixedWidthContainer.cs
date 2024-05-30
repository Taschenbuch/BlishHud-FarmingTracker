using Blish_HUD.Controls;
using Microsoft.Xna.Framework;

namespace FarmingTracker.Controls
{
    public class FixedWidthContainer : Container
    {
        public FixedWidthContainer(Container parent)
        {
            HeightSizingMode = SizingMode.AutoSize;
            Parent = parent;
        }
    }
}
