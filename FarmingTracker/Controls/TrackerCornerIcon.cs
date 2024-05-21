using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using System;

namespace FarmingTracker
{
    public class TrackerCornerIcon : CornerIcon
    {
        public TrackerCornerIcon(Services services, EventHandler<MouseEventArgs> cornerIconClickEventHandler)
        {
            _cornerIconClickEventHandler = cornerIconClickEventHandler;

            Icon = services.TextureService.CornerIconTexture;
            HoverIcon = services.TextureService.CornerIconHoverTexture;
            BasicTooltipText = "Open farming tracker window";
            Priority = 1788560160;
            Parent = GameService.Graphics.SpriteScreen;

            Click += cornerIconClickEventHandler;
        }

        protected override void DisposeControl()
        {
            Click -= _cornerIconClickEventHandler;
            base.DisposeControl();
        }

        private readonly EventHandler<MouseEventArgs> _cornerIconClickEventHandler;
    }
}
