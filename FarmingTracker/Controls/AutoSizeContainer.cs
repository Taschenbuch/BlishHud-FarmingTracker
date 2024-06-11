using Blish_HUD.Controls;

namespace FarmingTracker
{
    public class AutoSizeContainer : Container
    {
        public AutoSizeContainer(Container parent)
        {
            HeightSizingMode = SizingMode.AutoSize;
            WidthSizingMode = SizingMode.AutoSize;
            Parent = parent;
        }
    }
}
