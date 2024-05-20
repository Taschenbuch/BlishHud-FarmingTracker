using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using FarmingTracker.Controls;
using Microsoft.Xna.Framework;
using Gw2SharpType = Gw2Sharp.WebApi.V2.Models;

namespace FarmingTracker
{
    public class StatContainer : Container
    {
        public StatContainer(Stat stat, Services services)
        {
            Size = new Point(BACKGROUND_SIZE + 2 * BACKGROUND_IMAGE_MARGIN);

            // inventory slot background
            new Image(AsyncTexture2D.FromAssetId(1318622))
            {
                BasicTooltipText = stat.Tooltip,
                Size = new Point(BACKGROUND_SIZE),
                Location = new Point(BACKGROUND_IMAGE_MARGIN),
                Parent = this,
            };

            // stat icon
            new Image(AsyncTexture2D.FromAssetId(stat.Details.IconAssetId))
            {
                BasicTooltipText = stat.Tooltip,
                Opacity = stat.Count > 0 ? 1f : 0.3f,
                Size = new Point(ICON_SIZE),
                Location = new Point(BACKGROUND_IMAGE_MARGIN + ICON_MARGIN),
                Parent = this
            };

            new Label
            {
                Text = stat.Count.ToString(),
                BasicTooltipText = stat.Tooltip,
                Font = services.FontService.Fonts[ContentService.FontSize.Size20],
                AutoSizeHeight = true,
                AutoSizeWidth = true,
                StrokeText = true,
                Location = new Point(BACKGROUND_IMAGE_MARGIN + ICON_MARGIN + 3, BACKGROUND_IMAGE_MARGIN + ICON_MARGIN + 1),
                Parent = this
            };

            var isNotCurrency = stat.Details.Rarity != Gw2SharpType.ItemRarity.Unknown;
            if (services.SettingService.RarityIconBorderIsVisibleSetting.Value && isNotCurrency)
                AddRarityBorder(stat);
        }

        private void AddRarityBorder(Stat stat)
        {
            var borderColor = DetermineBorderColor(stat.Details.Rarity);
            new BorderContainer(new Point(BORDER_LEFT_OR_TOP_LOCATION), new Point(BORDER_THICKNESS, BORDER_LENGTH), borderColor, stat.Tooltip, this);
            new BorderContainer(new Point(BORDER_RIGHT_OR_BOTTOM_LOCATION, BORDER_LEFT_OR_TOP_LOCATION), new Point(BORDER_THICKNESS, BORDER_LENGTH), borderColor, stat.Tooltip, this);
            new BorderContainer(new Point(BORDER_LEFT_OR_TOP_LOCATION), new Point(BORDER_LENGTH, BORDER_THICKNESS), borderColor, stat.Tooltip, this);
            new BorderContainer(new Point(BORDER_LEFT_OR_TOP_LOCATION, BORDER_RIGHT_OR_BOTTOM_LOCATION), new Point(BORDER_LENGTH, BORDER_THICKNESS), borderColor, stat.Tooltip, this);
        }

        private static Color DetermineBorderColor(Gw2SharpType.ItemRarity rarity)
        {
            return rarity switch
            {
                Gw2SharpType.ItemRarity.Junk => Constants.JunkColor,
                Gw2SharpType.ItemRarity.Basic => Constants.BasicColor,
                Gw2SharpType.ItemRarity.Fine => Constants.FineColor,
                Gw2SharpType.ItemRarity.Masterwork => Constants.MasterworkColor,
                Gw2SharpType.ItemRarity.Rare => Constants.RareColor,
                Gw2SharpType.ItemRarity.Exotic => Constants.ExoticColor,
                Gw2SharpType.ItemRarity.Ascended => Constants.AscendedColor,
                Gw2SharpType.ItemRarity.Legendary => Constants.LegendaryColor,
                _ => Color.Transparent, // includes .Unknown which match currencies
            };
        }

        // icon
        private const int ICON_SIZE = 60;
        private const int ICON_MARGIN = 1;
        // background
        private const int BACKGROUND_SIZE = ICON_SIZE + 2 * ICON_MARGIN;
        private const int BACKGROUND_IMAGE_MARGIN = 1;
        // rarity border
        private const int BORDER_THICKNESS = 2;
        private const int BORDER_LENGTH = BACKGROUND_SIZE;
        private const int BORDER_LEFT_OR_TOP_LOCATION = BACKGROUND_IMAGE_MARGIN;
        private const int BORDER_RIGHT_OR_BOTTOM_LOCATION = BORDER_LEFT_OR_TOP_LOCATION + BORDER_LENGTH - BORDER_THICKNESS;
    }
}
