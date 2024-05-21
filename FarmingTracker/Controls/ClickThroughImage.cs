using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarmingTracker
{
    public class ClickThroughImage : Image
    {
        public ClickThroughImage(Texture2D texture, Point location, Container parent) : base(texture)
        {
            Location = location;
            Parent = parent;
            SetOpacity(true);
        }

        protected override CaptureType CapturesInput()
        {
            return CaptureType.None;
        }

        public void SetOpacity(bool isTransparent)
        {
            Opacity = isTransparent ? 0.1f : 0.8f;
        }
    }
}
