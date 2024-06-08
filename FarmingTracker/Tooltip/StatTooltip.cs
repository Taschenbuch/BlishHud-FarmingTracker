using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using MonoGame.Extended.BitmapFonts;
using static Blish_HUD.ContentService;

namespace FarmingTracker
{
    public class StatTooltip : DisposableTooltip
    {
        public StatTooltip(Stat stat, AsyncTexture2D statIconTexture, PanelType panelType, Services services)
        {
            var rootFlowPanel = new FlowPanel()
            {
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                HeightSizingMode = SizingMode.AutoSize,
                WidthSizingMode = SizingMode.AutoSize,
                Parent = this
            };

            var font = services.FontService.Fonts[FontSize.Size16];

            switch (stat.Details.State)
            {
                case ApiStatDetailsState.SetByApi:
                    AddTitle(stat, statIconTexture, rootFlowPanel);
                    AddDescription(stat, font, rootFlowPanel);
                    if(panelType == PanelType.SessionSummary)
                    {
                        StatTooltipService.AddProfitTable(stat, font, services, rootFlowPanel);
                        StatTooltipService.AddText("\nRight click for more options.", font, rootFlowPanel);
                    }
                    else
                    {
                        StatTooltipService.AddText("\nLeft click to unignore this item.", font, rootFlowPanel);
                    }
                    break;
                case ApiStatDetailsState.GoldCoinCustomStat:
                case ApiStatDetailsState.SilveCoinCustomStat:
                case ApiStatDetailsState.CopperCoinCustomStat:
                    StatTooltipService.AddText($"{stat.Count}\nChanges in 'raw gold'.\nIn other words coins spent or gained.", font, rootFlowPanel);
                    break;
                case ApiStatDetailsState.MissingBecauseUnknownByApi:
                {
                    var errorMessage =
                        $"Unknown item/currency (ID: {stat.ApiId})\n" +
                        $"GW2 API has no information about it.\n" +
                        $"This issue typically occurs for items related to renown hearts.";

                    StatTooltipService.AddText(errorMessage, font, rootFlowPanel);

                    if (panelType == PanelType.SessionSummary)
                        StatTooltipService.AddText("\nRight click to search its ID in the wiki in your default browser.", font, rootFlowPanel);
                    else
                        StatTooltipService.AddText("\nLeft click to unignore this item.", font, rootFlowPanel);
                    
                    break;
                }
                case ApiStatDetailsState.MissingBecauseApiNotCalledYet:
                {
                    var errorMessage = $"Module error: API was not called for stat. Stats like that should not be displayed (id: {stat.ApiId}).";
                    Module.Logger.Error(errorMessage);
                    StatTooltipService.AddText(errorMessage, font, rootFlowPanel);
                    break;
                }
                default:
                {
                    Module.Logger.Error(Helper.CreateSwitchCaseNotFoundMessage(stat.Details.State, nameof(ApiStatDetailsState), "use error tooltip"));
                    StatTooltipService.AddText("Module error: unexpected state", font, rootFlowPanel);
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

            // stat icon
            var iconImage = new Image(statIconTexture)
            {
                Size = new Point(50),
                Parent = iconAndNamePanel
            };

            var rarityColor = ColorService.GetRarityTextColor(stat.Details.Rarity);

            // stat name
#pragma warning disable IDE0017 // Simplify object initialization
            var nameLabel = new Label
            {
                Text = $"{stat.Count} {stat.Details.Name}",
                Font = GameService.Content.GetFont(FontFace.Menomonia, FontSize.Size18, FontStyle.Bold), // bold because otherwise text colors become too dark
                TextColor = rarityColor,
                AutoSizeHeight = true,
                AutoSizeWidth = true,
                Parent = iconAndNamePanel,
            };
#pragma warning restore IDE0017 // Simplify object initialization

            nameLabel.Left = iconImage.Right + 5;
            nameLabel.Top = (iconImage.Height - nameLabel.Height) / 2;  
        }

        private static void AddDescription(Stat stat, BitmapFont font, Container parent)
        {
            var hasNoDescription = string.IsNullOrWhiteSpace(stat.Details.Description);
            if (hasNoDescription)
                return;

            // stat description
            var description = new Label
            {
                Text = stat.Details.Description,
                Font = font,
                AutoSizeHeight = true,
                Parent = parent,
            };

            if (stat.Details.Description.Length > 36)
            {
                // limit width because some tooltips ended up using half screen width. This solution is not perfect, but works for now.
                description.Width = 450;
                description.WrapText = true;
            }
            else
                description.AutoSizeWidth = true;
        }
    }
}
