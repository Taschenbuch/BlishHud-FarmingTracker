using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using static Blish_HUD.ContentService;

namespace FarmingTracker
{
    public class StatTooltip : Tooltip
    {
        public StatTooltip(Stat stat, AsyncTexture2D statIconTexture, Services services)
        {
            var rootFlowPanel = new FlowPanel()
            {
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                HeightSizingMode = SizingMode.AutoSize,
                WidthSizingMode = SizingMode.AutoSize,
                Parent = this
            };

            switch (stat.Details.State)
            {
                case ApiStatDetailsState.SetByApi:
                    AddTitle(stat, statIconTexture, rootFlowPanel);
                    AddDescription(stat, rootFlowPanel);
                    StatTooltipService.AddProfitTable(stat, services, rootFlowPanel);
                    StatTooltipService.AddText("\nRight click to open its wiki page in your default browser.", rootFlowPanel);
                    break;
                case ApiStatDetailsState.GoldCoinCustomStat:
                case ApiStatDetailsState.SilveCoinCustomStat:
                case ApiStatDetailsState.CopperCoinCustomStat:
                    StatTooltipService.AddText($"{stat.Count}\nChanges in 'raw gold'.\nIn other words coins spent or gained.", rootFlowPanel);
                    break;
                case ApiStatDetailsState.MissingBecauseUnknownByApi:
                {
                    var errorMessage =
                        $"Unknown item/currency (ID: {stat.ApiId})\n" +
                        $"GW2 API has no information about it.\n" +
                        $"This issue typically occurs for items related to renown hearts.\n" +
                        $"Right click to search its ID in the wiki in your default browser.";
                    StatTooltipService.AddText(errorMessage, rootFlowPanel);
                    break;
                }
                case ApiStatDetailsState.MissingBecauseApiNotCalledYet:
                {
                    var errorMessage = $"Module error: API was not called for stat. Stats like that should not be displayed (id: {stat.ApiId}).";
                    Module.Logger.Error(errorMessage);
                    StatTooltipService.AddText(errorMessage, rootFlowPanel);
                    break;
                }
                default:
                {
                    Module.Logger.Error(Helper.CreateSwitchCaseNotFoundMessage(stat.Details.State, nameof(ApiStatDetailsState), "use error tooltip"));
                    StatTooltipService.AddText("Module error: unexpected state", rootFlowPanel);
                    break;
                }
            }
        }

        private static void AddTitle(Stat stat, AsyncTexture2D statIconTexture, Container parent)
        {
            var iconAndNamePanel = new Panel()
            {
                HeightSizingMode = SizingMode.AutoSize,
                WidthSizingMode = SizingMode.AutoSize,
                Parent = parent
            };

            // icon
            var iconImage = new Image(statIconTexture)
            {
                Size = new Point(50),
                Parent = iconAndNamePanel
            };

            var rarityColor = ColorService.GetRarityTextColor(stat.Details.Rarity);

            // name
            var nameLabel = new Label
            {
                Text = $"{stat.Count} {stat.Details.Name}",
                Font = GameService.Content.GetFont(FontFace.Menomonia, FontSize.Size16, FontStyle.Bold), // bold because otherwise text colors become too dark
                TextColor = rarityColor,
                AutoSizeHeight = true,
                AutoSizeWidth = true,
                Parent = iconAndNamePanel,
            };

            nameLabel.Left = iconImage.Right + 5;
            nameLabel.Top = (iconImage.Height - nameLabel.Height) / 2;  
        }

        private static void AddDescription(Stat stat, Container parent)
        {
            var hasNoDescription = string.IsNullOrWhiteSpace(stat.Details.Description);
            if (hasNoDescription)
                return;

            if (stat.Details.Description.Length > 36)
            {
                // limit width because some tooltips ended up using half screen width. This solution is not perfect, but works for now.
                new FormattedLabelBuilder()
                    .SetWidth(450)
                    .Wrap()
                    .AutoSizeHeight()
                    .CreatePart(stat.Details.Description, builder => builder
                    .SetFontSize(FontSize.Size14))
                    .Build()
                    .Parent = parent;
            }
            else
            {
                new FormattedLabelBuilder()
                    .AutoSizeWidth()
                    .AutoSizeHeight()
                    .CreatePart(stat.Details.Description, builder => builder
                    .SetFontSize(FontSize.Size14))
                    .Build()
                    .Parent = parent;
            }
        }
    }
}
