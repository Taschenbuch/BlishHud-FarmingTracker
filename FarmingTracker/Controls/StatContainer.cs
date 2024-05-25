using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using FarmingTracker.Controls;
using Microsoft.Xna.Framework;
using Gw2SharpType = Gw2Sharp.WebApi.V2.Models;

namespace FarmingTracker
{
    public class StatContainer : Container
    {
        public StatContainer(Stat stat, Services services)
        {
            _stat = stat;

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

            var tooltip = StatTooltipSetter.CreateTooltip(stat);

            // inventory slot background
            new Image(services.TextureService.InventorySlotBackgroundTexture)
            {
                BasicTooltipText = tooltip,
                Size = new Point(backgroundSize),
                Location = new Point(backgroundMargin),
                Parent = this,
            };

            // stat icon
            new Image(GetStatIconTexture(stat, services))
            {
                BasicTooltipText = tooltip,
                Opacity = stat.Count > 0 ? 1f : 0.3f,
                Size = new Point(iconSize),
                Location = new Point(backgroundMargin + iconMargin),
                Parent = this
            };

            // stat count
            new Label
            {
                Text = stat.Count.ToString(),
                BasicTooltipText = tooltip,
                Font = services.FontService.Fonts[services.SettingService.CountFontSizeSetting.Value],
                TextColor = services.SettingService.CountTextColorSetting.Value.GetColor(),
                HorizontalAlignment = services.SettingService.CountHoritzontalAlignmentSetting.Value,
                BackgroundColor = services.SettingService.CountBackgroundColorSetting.Value.GetColor() * (services.SettingService.CountBackgroundOpacitySetting.Value / 255f),
                StrokeText = true,
                AutoSizeHeight = true,
                Width = iconSize - 5,
                Location = new Point(backgroundMargin + iconMargin, backgroundMargin + iconMargin + 1),
                Parent = this
            };

            // stat count
            var isNotCurrency = stat.Details.Rarity != Gw2SharpType.ItemRarity.Unknown;
            if (services.SettingService.RarityIconBorderIsVisibleSetting.Value && isNotCurrency)
                AddRarityBorder(stat, rarityBorderLeftOrTopLocation, rarityBorderRightOrBottomLocation, rarityBorderThickness, rarityBorderLength, tooltip);
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

        private void AddRarityBorder(Stat stat, int borderLeftOrTopLocation, int borderRightOrBottomLocation, int borderThickness, int borderLength, string tooltip)
        {
            var borderColor = DetermineBorderColor(stat.Details.Rarity);
            new BorderContainer(new Point(borderLeftOrTopLocation), new Point(borderThickness, borderLength), borderColor, tooltip, this);
            new BorderContainer(new Point(borderRightOrBottomLocation, borderLeftOrTopLocation), new Point(borderThickness, borderLength), borderColor, tooltip, this);
            new BorderContainer(new Point(borderLeftOrTopLocation), new Point(borderLength, borderThickness), borderColor, tooltip, this);
            new BorderContainer(new Point(borderLeftOrTopLocation, borderRightOrBottomLocation), new Point(borderLength, borderThickness), borderColor, tooltip, this);
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

        protected override void OnRightMouseButtonPressed(MouseEventArgs e)
        {
            if(_stat.Details.State == ApiStatDetailsState.MissingBecauseUnknownByApi)
                WikiService.OpenWikiIdQueryInDefaultBrowser(_stat.ApiId);

            if(_stat.Details.HasWikiSearchTerm)
                WikiService.OpenWikiSearchInDefaultBrowser(_stat.Details.WikiSearchTerm);

            base.OnRightMouseButtonPressed(e);
        }

        private readonly Stat _stat;
    }
}
