using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Blish_HUD.Modules.Managers;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace FarmingTracker
{
    public class TrackerCornerIcon : CornerIcon
    {
        public TrackerCornerIcon(ContentsManager contentsManager, EventHandler<MouseEventArgs> cornerIconClickEventHandler)
        {
            _cornerIconClickEventHandler = cornerIconClickEventHandler;
            _cornerIconTexture = contentsManager.GetTexture(@"cornerIcon.png");
            _cornerIconHoverTexture = contentsManager.GetTexture(@"cornerIconHover.png");

            Icon = _cornerIconTexture;
            HoverIcon = _cornerIconHoverTexture;
            BasicTooltipText = "Open farming tracker window";
            Priority = 1788560160;
            Parent = GameService.Graphics.SpriteScreen;

            Click += cornerIconClickEventHandler;
        }

        protected override void DisposeControl()
        {
            _cornerIconTexture?.Dispose();
            _cornerIconHoverTexture?.Dispose();
            Click -= _cornerIconClickEventHandler;
            base.DisposeControl();
        }

        private readonly Texture2D _cornerIconTexture;
        private readonly Texture2D _cornerIconHoverTexture;
        private readonly FarmingTrackerWindowService _farmingTrackerWindowService;
        private readonly EventHandler<MouseEventArgs> _cornerIconClickEventHandler;
    }
}
