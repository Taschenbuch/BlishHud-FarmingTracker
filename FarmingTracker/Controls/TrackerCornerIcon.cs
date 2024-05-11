using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Modules.Managers;
using Microsoft.Xna.Framework.Graphics;

namespace FarmingTracker
{
    public class TrackerCornerIcon : CornerIcon
    {
        public TrackerCornerIcon(ContentsManager contentsManager, FarmingTrackerWindowService farmingTrackerWindowService)
        {
            _farmingTrackerWindowService = farmingTrackerWindowService;
            _cornerIconTexture = contentsManager.GetTexture(@"cornerIcon.png");
            _cornerIconHoverTexture = contentsManager.GetTexture(@"cornerIconHover.png");

            Icon = _cornerIconTexture;
            HoverIcon = _cornerIconHoverTexture;
            BasicTooltipText = "Open farming tracker window";
            Priority = 1788560160;
            Parent = GameService.Graphics.SpriteScreen;

            Click += OnCornerIconClick;
        }

        protected override void DisposeControl()
        {
            _cornerIconTexture?.Dispose();
            _cornerIconHoverTexture?.Dispose();
            Click -= OnCornerIconClick;
            base.DisposeControl();
        }

        private void OnCornerIconClick(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            _farmingTrackerWindowService.ToggleWindow();
        }

        private readonly Texture2D _cornerIconTexture;
        private readonly Texture2D _cornerIconHoverTexture;
        private readonly FarmingTrackerWindowService _farmingTrackerWindowService;
    }
}
