using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;

namespace FarmingTracker
{
    public class StatContainer : Container
    {
        public StatContainer(ItemX item, Services services)
        {
            _backgroundImage = new Image(AsyncTexture2D.FromAssetId(1318622))
            {
                BasicTooltipText = item.Tooltip,
                Location = new Point(BACKGROUND_IMAGE_MARGIN),
                Parent = this,
            };

            _icon = new Image(AsyncTexture2D.FromAssetId(item.IconAssetId))
            {
                BasicTooltipText = item.Tooltip,
                Opacity = item.Count > 0 ? 1f : 0.3f,
                Size = new Point(60),
                Location = new Point(BACKGROUND_IMAGE_MARGIN + ICON_MARGIN),
                Parent = this
            };

            new Label
            {
                Text = item.Count.ToString(),
                Font = services.FontService.Fonts[ContentService.FontSize.Size20],
                AutoSizeHeight = true,
                AutoSizeWidth = true,
                StrokeText = true,
                Location = new Point(BACKGROUND_IMAGE_MARGIN + ICON_MARGIN + 3, BACKGROUND_IMAGE_MARGIN + ICON_MARGIN + 1),
                Parent = this
            };

            var statIconSize = 60;
            Size = new Point(statIconSize + 2 * ICON_MARGIN + 2 * BACKGROUND_IMAGE_MARGIN);
            _backgroundImage.Size = new Point(statIconSize + 2 * ICON_MARGIN);
            _icon.Size = new Point(statIconSize);
        }

        private readonly Image _backgroundImage;
        private readonly Image _icon;
        private const int ICON_MARGIN = 1;
        private const int BACKGROUND_IMAGE_MARGIN = 1;
    }
}
