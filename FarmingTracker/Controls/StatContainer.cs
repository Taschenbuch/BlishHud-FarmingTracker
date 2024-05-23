using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using FarmingTracker.Controls;
using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;
using Gw2SharpType = Gw2Sharp.WebApi.V2.Models;

namespace FarmingTracker
{
    public class StatContainer : Container
    {
        public StatContainer(Stat stat, Services services)
        {
            _stat = stat;
            Size = new Point(BACKGROUND_SIZE + 2 * BACKGROUND_IMAGE_MARGIN);

            var tooltip = StatTooltipSetter.CreateTooltip(stat);

            // inventory slot background
            new Image(services.TextureService.InventorySlotBackgroundTexture)
            {
                BasicTooltipText = tooltip,
                Size = new Point(BACKGROUND_SIZE),
                Location = new Point(BACKGROUND_IMAGE_MARGIN),
                Parent = this,
            };

            // stat icon
            new Image(GetStatIconTexture(stat, services))
            {
                BasicTooltipText = tooltip,
                Opacity = stat.Count > 0 ? 1f : 0.3f,
                Size = new Point(ICON_SIZE),
                Location = new Point(BACKGROUND_IMAGE_MARGIN + ICON_MARGIN),
                Parent = this
            };

            // stat count
            new Label
            {
                Text = stat.Count.ToString(),
                BasicTooltipText = tooltip,
                Font = services.FontService.Fonts[ContentService.FontSize.Size20],
                HorizontalAlignment = HorizontalAlignment.Right,
                StrokeText = true,
                AutoSizeHeight = true,
                Width = ICON_SIZE - 5,
                Location = new Point(BACKGROUND_IMAGE_MARGIN + ICON_MARGIN, BACKGROUND_IMAGE_MARGIN + ICON_MARGIN + 1),
                Parent = this
            };

            // stat count
            var isNotCurrency = stat.Details.Rarity != Gw2SharpType.ItemRarity.Unknown;
            if (services.SettingService.RarityIconBorderIsVisibleSetting.Value && isNotCurrency)
                AddRarityBorder(stat, tooltip);
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

        private void AddRarityBorder(Stat stat, string tooltip)
        {
            var borderColor = DetermineBorderColor(stat.Details.Rarity);
            new BorderContainer(new Point(BORDER_LEFT_OR_TOP_LOCATION), new Point(BORDER_THICKNESS, BORDER_LENGTH), borderColor, tooltip, this);
            new BorderContainer(new Point(BORDER_RIGHT_OR_BOTTOM_LOCATION, BORDER_LEFT_OR_TOP_LOCATION), new Point(BORDER_THICKNESS, BORDER_LENGTH), borderColor, tooltip, this);
            new BorderContainer(new Point(BORDER_LEFT_OR_TOP_LOCATION), new Point(BORDER_LENGTH, BORDER_THICKNESS), borderColor, tooltip, this);
            new BorderContainer(new Point(BORDER_LEFT_OR_TOP_LOCATION, BORDER_RIGHT_OR_BOTTOM_LOCATION), new Point(BORDER_LENGTH, BORDER_THICKNESS), borderColor, tooltip, this);
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
                OpenWikiIdQueryInDefaultBrowser(_stat.ApiId);

            if(_stat.Details.HasWikiSearchTerm)
                OpenWikiSearchInDefaultBrowser(_stat.Details.WikiSearchTerm);

            base.OnRightMouseButtonPressed(e);
        }

        private static void OpenWikiSearchInDefaultBrowser(string wikiSearchTerm)
        {
            var url = $"https://wiki.guildwars2.com/index.php?&search={Uri.EscapeDataString(wikiSearchTerm)}";
            OpenUrlInDefaultBrowser(url);
        }

        private static void OpenWikiIdQueryInDefaultBrowser(int apiId)
        {
            var url = $"https://wiki.guildwars2.com/wiki/Special:RunQuery/Search_by_id?title=Special%3ARunQuery%2FSearch_by_id&pfRunQueryFormName=Search+by+id&Search+by+id%5Bid%5D={apiId}&Search+by+id%5Bcontext%5D=&wpRunQuery=&pf_free_text=";
            OpenUrlInDefaultBrowser(url);
        }

        private static void OpenUrlInDefaultBrowser(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception e)
            {
                Module.Logger.Error(e, "Failed to open url in default browser.");
            }
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
        private readonly Stat _stat;
    }
}
