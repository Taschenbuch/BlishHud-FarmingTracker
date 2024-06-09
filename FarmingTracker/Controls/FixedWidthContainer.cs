using Blish_HUD.Controls;

namespace FarmingTracker
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
