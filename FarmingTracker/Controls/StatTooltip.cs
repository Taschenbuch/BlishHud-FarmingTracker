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
                    AddProfitTable(stat, services, rootFlowPanel);
                    AddText("\nRight click to open its wiki page in your default browser.", rootFlowPanel);
                    break;
                case ApiStatDetailsState.GoldCoinCustomStat:
                case ApiStatDetailsState.SilveCoinCustomStat:
                case ApiStatDetailsState.CopperCoinCustomStat:
                    AddText("Changes in 'raw gold'.\nIn other words coins spent or gained.", rootFlowPanel);
                    break;
                case ApiStatDetailsState.MissingBecauseUnknownByApi:
                {
                    var errorMessage =
                        $"Unknown item/currency (ID: {stat.ApiId})\n" +
                        $"GW2 API has no information about it.\n" +
                        $"This issue typically occurs for items related to renown hearts.\n" +
                        $"Right click to search its ID in the wiki in your default browser.";
                    AddText(errorMessage, rootFlowPanel);
                    break;
                }
                case ApiStatDetailsState.MissingBecauseApiNotCalledYet:
                {
                    var errorMessage = $"Error: Api was not called for stat (id: {stat.ApiId}).";
                    Module.Logger.Error(errorMessage);
                    AddText(errorMessage, rootFlowPanel);
                    break;
                }
                default:
                {
                    Module.Logger.Error(Helper.CreateSwitchCaseNotFoundMessage(stat.Details.State, nameof(ApiStatDetailsState), "use error tooltip"));
                    AddText("Module Error: unexpected state", rootFlowPanel);
                    break;
                }
            }
        }

        private void AddText(string text, Container parent)
        {
            new Label
            {
                Text = text,
                AutoSizeWidth = true,
                AutoSizeHeight = true,
                Parent = parent,
            };
        }

        private static void AddEmptyLine(Container parent)
        {
            new Label
            {
                Text = Constants.EMPTY_LABEL,
                Parent = parent,
            };
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

        private static void AddProfitTable(Stat stat, Services services, Container parent)
        {
            if (stat.Profits.CanNotBeSold)
                return;

            AddEmptyLine(parent);

            var profitRootFlowPanel = new FlowPanel
            {
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
                HeightSizingMode = SizingMode.AutoSize,
                WidthSizingMode = SizingMode.AutoSize,
                Parent = parent,
            };

            var font = services.FontService.Fonts[FontSize.Size16];

            var titleColumnFlowPanel = new FlowPanel
            {
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                HeightSizingMode = SizingMode.AutoSize,
                WidthSizingMode = SizingMode.AutoSize,
                Parent = profitRootFlowPanel,
            };

            var allColumnFlowPanel = new FlowPanel
            {
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                HeightSizingMode = SizingMode.AutoSize,
                WidthSizingMode = SizingMode.AutoSize,
                Parent = stat.IsSingleItem ? null : profitRootFlowPanel,
            };

            var eachColumnFlowPanel = new FlowPanel
            {
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                HeightSizingMode = SizingMode.AutoSize,
                WidthSizingMode = SizingMode.AutoSize,
                Parent = profitRootFlowPanel,
            };

            var rowHeight = 22;

            new Label
            {
                Text = Constants.EMPTY_LABEL,
                Font = font,
                Height = rowHeight,
                AutoSizeWidth = true,
                Parent = titleColumnFlowPanel,
            };

            var padding = " ";

            new Label
            {
                Text = $"{padding}all",
                Font = font,
                Height = rowHeight,
                AutoSizeWidth = true,
                Parent = allColumnFlowPanel,
            };

            new Label
            {
                Text = stat.IsSingleItem ? Constants.EMPTY_LABEL : $"{padding}each",
                Font = font,
                Height = rowHeight,
                AutoSizeWidth = true,
                Parent = eachColumnFlowPanel,
            };

            if(stat.Profits.CanBeSoldOnTp)
            {
                var allTpSellProfitPanel = new ProfitPanel("", "", font, services, allColumnFlowPanel, rowHeight);
                allTpSellProfitPanel.SetProfit(stat.CountSign * stat.Profits.All.TpSellProfitInCopper);

                var allTpBuyProfitPanel = new ProfitPanel("", "", font, services, allColumnFlowPanel, rowHeight);
                allTpBuyProfitPanel.SetProfit(stat.CountSign * stat.Profits.All.TpBuyProfitInCopper);

                var eachTpSellProfitPanel = new ProfitPanel("", "", font, services, eachColumnFlowPanel, rowHeight);
                eachTpSellProfitPanel.SetProfit(stat.CountSign * stat.Profits.Each.TpSellProfitInCopper);

                var eachTpBuyProfitPanel = new ProfitPanel("", "", font, services, eachColumnFlowPanel, rowHeight);
                eachTpBuyProfitPanel.SetProfit(stat.CountSign * stat.Profits.Each.TpBuyProfitInCopper);

                new Label
                {
                    Text = "TP Sell",
                    Font = font,
                    Height = rowHeight,
                    AutoSizeWidth = true,
                    Parent = titleColumnFlowPanel,
                };

                new Label
                {
                    Text = "TP Buy",
                    Font = font,
                    Height = rowHeight,
                    AutoSizeWidth = true,
                    Parent = titleColumnFlowPanel,
                };
            }

            if (stat.Profits.CanBeSoldToVendor)
            {
                var allVendorProfitPanel = new ProfitPanel("", "", font, services, allColumnFlowPanel, rowHeight);
                allVendorProfitPanel.SetProfit(stat.CountSign * stat.Profits.All.VendorProfitInCopper);

                var eachVendorProfitPanel = new ProfitPanel("", "", font, services, eachColumnFlowPanel, rowHeight);
                eachVendorProfitPanel.SetProfit(stat.CountSign * stat.Profits.Each.VendorProfitInCopper);

                new Label
                {
                    Text = "Vendor",
                    Font = font,
                    Height = rowHeight,
                    AutoSizeWidth = true,
                    Parent = titleColumnFlowPanel,
                };
            }

            if (stat.Profits.CanBeSoldOnTp)
            {
                new Label
                {
                    Text = "\n(15% trading post fee is already deducted from TP sell/buy.)",
                    Font = font,
                    AutoSizeHeight = true,
                    AutoSizeWidth = true,
                    Parent = parent,
                };
            }
        }
    }
}
