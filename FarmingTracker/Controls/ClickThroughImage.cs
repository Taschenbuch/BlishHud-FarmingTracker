using Blish_HUD.Content;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;

namespace FarmingTracker
{
    public class ClickThroughImage : Image
    {
        public ClickThroughImage(AsyncTexture2D texture, Point location, Container parent) : base(texture)
        {
            Location = location;
            Parent = parent;
        }

        protected override CaptureType CapturesInput()
        {
            return CaptureType.None;
        }
    }
}
