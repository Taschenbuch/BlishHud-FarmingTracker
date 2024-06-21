using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Gw2SharpType = Gw2Sharp.WebApi.V2.Models;

namespace FarmingTracker
{
    public class StatContainer : Container
    {
        public StatContainer(Stat stat, PanelType panelType, SafeList<int> ignoredItemApiIds, SafeList<int> favoriteItemApiIds, Services services)
        {
            // icon
            var iconSize = (int)services.SettingService.StatIconSizeSetting.Value;
            var iconMargin = 1;
            // background
            var backgroundSize = iconSize + 2 * iconMargin;
            var backgroundMargin = 1;
            // rarity border
            var rarityBorderThickness = 2;
            var rarityBorderLength = backgroundSize;
            var rarityBorderLeftOrTopLocation = backgroundMargin;
            var rarityBorderRightOrBottomLocation = rarityBorderLeftOrTopLocation + rarityBorderLength - rarityBorderThickness;

            Size = new Point(backgroundSize + 2 * backgroundMargin);

            var statIconTexture = GetStatIconTexture(stat, services);
            var statTooltip = new StatTooltip(stat, statIconTexture, panelType, services);
            _statTooltip = statTooltip;

            // inventory slot background
            new Image(services.TextureService.InventorySlotBackgroundTexture)
            {
                Tooltip = statTooltip,
                Size = new Point(backgroundSize),
                Location = new Point(backgroundMargin),
                Parent = this,
            };

            // stat icon
            new Image(statIconTexture)
            {
                Tooltip = statTooltip,
                Opacity = stat.Count > 0 ? 1f : (services.SettingService.NegativeCountIconOpacitySetting.Value / 255f),
                Size = new Point(iconSize),
                Location = new Point(backgroundMargin + iconMargin),
                Parent = this
            };

            // stat count
            new Label
            {
                Text = stat.Count.ToString(),
                Tooltip = statTooltip,
                Font = services.FontService.Fonts[services.SettingService.CountFontSizeSetting.Value],
                TextColor = stat.Count >= 0
                    ? services.SettingService.PositiveCountTextColorSetting.Value.GetColor()
                    : services.SettingService.NegativeCountTextColorSetting.Value.GetColor(),
                HorizontalAlignment = services.SettingService.CountHoritzontalAlignmentSetting.Value,
                BackgroundColor = services.SettingService.CountBackgroundColorSetting.Value.GetColor() * (services.SettingService.CountBackgroundOpacitySetting.Value / 255f),
                StrokeText = true,
                AutoSizeHeight = true,
                Width = iconSize - 5,
                Location = new Point(backgroundMargin + iconMargin, backgroundMargin + iconMargin + 1),
                Parent = this
            };

            if (services.SettingService.RarityIconBorderIsVisibleSetting.Value)
                AddRarityBorder(stat.Details.Rarity, rarityBorderLeftOrTopLocation, rarityBorderRightOrBottomLocation, rarityBorderThickness, rarityBorderLength, statTooltip);

            if (panelType != PanelType.IgnoredItems)
                RightMouseButtonPressed += (s, e) =>
                {
                    var contextMenuStrip = new StatContextMenuStrip(stat, panelType, ignoredItemApiIds, favoriteItemApiIds, services);
                    contextMenuStrip.Hidden += (s, e) => contextMenuStrip.Dispose();
                    contextMenuStrip.Show(GameService.Input.Mouse.Position);
                };
        }

        private static AsyncTexture2D GetStatIconTexture(Stat stat, Services services)
        {
            return stat.Details.State switch
            {
                ApiStatDetailsState.GoldCoinCustomStat => services.TextureService.GoldCoinTexture,
                ApiStatDetailsState.SilveCoinCustomStat => services.TextureService.SilverCoinTexture,
                ApiStatDetailsState.CopperCoinCustomStat => services.TextureService.CopperCoinTexture,
                _ => services.TextureService.GetTextureFromAssetCacheOrFallback(stat.Details.IconAssetId),
            };
        }

        private void AddRarityBorder(Gw2SharpType.ItemRarity rarity, int borderLeftOrTopLocation, int borderRightOrBottomLocation, int borderThickness, int borderLength, StatTooltip tooltip)
        {
            var borderColor = ColorService.GetRarityBorderColor(rarity);
            new BorderContainer(new Point(borderLeftOrTopLocation), new Point(borderThickness, borderLength), borderColor, tooltip, this);
            new BorderContainer(new Point(borderRightOrBottomLocation, borderLeftOrTopLocation), new Point(borderThickness, borderLength), borderColor, tooltip, this);
            new BorderContainer(new Point(borderLeftOrTopLocation), new Point(borderLength, borderThickness), borderColor, tooltip, this);
            new BorderContainer(new Point(borderLeftOrTopLocation, borderRightOrBottomLocation), new Point(borderLength, borderThickness), borderColor, tooltip, this);
        }

        protected override void DisposeControl()
        {
            _statTooltip?.Dispose();
            base.DisposeControl();
        }

        private readonly StatTooltip _statTooltip;
    }
}
