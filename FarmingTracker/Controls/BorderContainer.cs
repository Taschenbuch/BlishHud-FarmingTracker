using Blish_HUD.Controls;
using Microsoft.Xna.Framework;

namespace FarmingTracker.Controls
{
    public class BorderContainer : Container
    {
        public BorderContainer(Point location, Point size, Color borderColor, string tooltip, Container parent)
        {
            Location = location;
            Size = size;
            BackgroundColor = borderColor;
            BasicTooltipText = tooltip;
            Parent = parent;
        }
    }
}
