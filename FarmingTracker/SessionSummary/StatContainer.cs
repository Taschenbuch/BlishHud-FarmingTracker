using Blish_HUD.Content;
using Blish_HUD.Controls;
using FarmingTracker.Controls;
using Microsoft.Xna.Framework;
using System;
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

            var statIconTexture = GetStatIconTexture(stat, services);
            var statTooltip = new StatTooltip(stat, statIconTexture, services);
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
                Opacity = stat.Count > 0 ? 1f : 0.3f,
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
                TextColor = services.SettingService.CountTextColorSetting.Value.GetColor(),
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

            _contextMenuStrip = new ContextMenuStrip();
            Menu = _contextMenuStrip;

            var wikiMenuItem = _contextMenuStrip.AddMenuItem("Open Wiki");
            wikiMenuItem.Click += (s, e) => OpenWiki();
            wikiMenuItem.BasicTooltipText = "Open its wiki page in your default browser.";
        }
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

        private void OpenWiki()
        {
            if (_stat.Details.State == ApiStatDetailsState.MissingBecauseUnknownByApi)
                WikiService.OpenWikiIdQueryInDefaultBrowser(_stat.ApiId);

            if (_stat.Details.HasWikiSearchTerm)
                WikiService.OpenWikiSearchInDefaultBrowser(_stat.Details.WikiSearchTerm);
        }

        protected override void DisposeControl()
        {
            _contextMenuStrip.Dispose();
            _statTooltip?.Dispose();
            base.DisposeControl();
        }

        private readonly Stat _stat;
        private readonly ContextMenuStrip _contextMenuStrip;
        private StatTooltip _statTooltip;
    }
}
