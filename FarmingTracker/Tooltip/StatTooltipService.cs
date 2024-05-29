using Blish_HUD.Controls;
using static Blish_HUD.ContentService;
using System.Collections.Generic;
using System.Linq;

namespace FarmingTracker
{
    public class StatTooltipService
    {
        public static void AddText(string text, Container parent)
        {
            new Label
            {
                Text = text,
                AutoSizeWidth = true,
                AutoSizeHeight = true,
                Parent = parent,
            };
        }

        public static void AddProfitTable(Stat stat, Services services, Container parent)
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

            var titleColumnLabels = new List<Label>();

            var topLeftLabel = new Label
            {
                Text = Constants.EMPTY_LABEL,
                Font = font,
                Height = rowHeight,
                Parent = titleColumnFlowPanel,
            };

            titleColumnLabels.Add(topLeftLabel);

            if (stat.Profits.CanBeSoldToVendor)
            {
                var vendorRowTitleLabel = new Label
                {
                    Text = "Vendor",
                    Font = font,
                    Height = rowHeight,
                    Parent = titleColumnFlowPanel,
                };

                titleColumnLabels.Add(vendorRowTitleLabel);
            }

            if (stat.Profits.CanBeSoldOnTp)
            {
                var tpSellTitleLabel = new Label
                {
                    Text = "TP Sell",
                    Font = font,
                    Height = rowHeight,
                    Parent = titleColumnFlowPanel,
                };

                var tpBuyTitleLabel = new Label
                {
                    Text = "TP Buy",
                    Font = font,
                    Height = rowHeight,
                    Parent = titleColumnFlowPanel,
                };

                titleColumnLabels.Add(tpSellTitleLabel);
                titleColumnLabels.Add(tpBuyTitleLabel);
            }

            var titleColumnWidth = titleColumnLabels.Max(l => l.Width) + 20;
            foreach (var titleColumn in titleColumnLabels)
                titleColumn.Width = titleColumnWidth;

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

            if (stat.Profits.CanBeSoldOnTp)
            {
                var allTpSellProfitPanel = new ProfitPanel("", "", font, services, allColumnFlowPanel, rowHeight);
                allTpSellProfitPanel.SetProfit(stat.CountSign * stat.Profits.All.TpSellProfitInCopper);

                var allTpBuyProfitPanel = new ProfitPanel("", "", font, services, allColumnFlowPanel, rowHeight);
                allTpBuyProfitPanel.SetProfit(stat.CountSign * stat.Profits.All.TpBuyProfitInCopper);

                var eachTpSellProfitPanel = new ProfitPanel("", "", font, services, eachColumnFlowPanel, rowHeight);
                eachTpSellProfitPanel.SetProfit(stat.CountSign * stat.Profits.Each.TpSellProfitInCopper);

                var eachTpBuyProfitPanel = new ProfitPanel("", "", font, services, eachColumnFlowPanel, rowHeight);
                eachTpBuyProfitPanel.SetProfit(stat.CountSign * stat.Profits.Each.TpBuyProfitInCopper);
            }

            if (stat.Profits.CanBeSoldToVendor)
            {
                var allVendorProfitPanel = new ProfitPanel("", "", font, services, allColumnFlowPanel, rowHeight);
                allVendorProfitPanel.SetProfit(stat.CountSign * stat.Profits.All.VendorProfitInCopper);

                var eachVendorProfitPanel = new ProfitPanel("", "", font, services, eachColumnFlowPanel, rowHeight);
                eachVendorProfitPanel.SetProfit(stat.CountSign * stat.Profits.Each.VendorProfitInCopper);
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

        private static void AddEmptyLine(Container parent)
        {
            new Label
            {
                Text = Constants.EMPTY_LABEL,
                Parent = parent,
            };
        }
    }
}
