﻿using Blish_HUD.Controls;
using static Blish_HUD.ContentService;
using System.Collections.Generic;
using System.Linq;
using MonoGame.Extended.BitmapFonts;
using FarmingTracker.Controls;

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

            if(!stat.IsSingleItem) // hack: because single item has empty string table headers and not headers like "each" and "all". the empty header still use space.
                AddEmptyLine(parent);

            var profitColumnsFlowPanel = new FlowPanel
            {
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
                HeightSizingMode = SizingMode.AutoSize,
                WidthSizingMode = SizingMode.AutoSize,
                Parent = parent,
            };

            var font = services.FontService.Fonts[FontSize.Size16];

            AddTitleColumn(stat, font, services, profitColumnsFlowPanel);

            if(stat.IsSingleItem)
            {
                AddProfitColumn("", stat.Profits.Each, stat, font, services, profitColumnsFlowPanel);
            }
            else
            {
                AddProfitColumn("all", stat.Profits.All, stat, font, services, profitColumnsFlowPanel);
                AddProfitColumn("each", stat.Profits.Each, stat, font, services, profitColumnsFlowPanel);
            }

            if (stat.Profits.CanBeSoldOnTp)
                AddText("\n(15% trading post fee is already deducted from TP sell/buy)", parent);
        }

        private static void AddTitleColumn(Stat stat, BitmapFont font, Services services, Container parent)
        {
            var titleColumnFlowPanel = new FlowPanel
            {
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                HeightSizingMode = SizingMode.AutoSize,
                WidthSizingMode = SizingMode.AutoSize,
                Parent = parent,
            };

            // empty top left cell
            new Label
            {
                Text = Constants.EMPTY_LABEL,
                Font = font,
                AutoSizeWidth = true,
                Height = ROW_HEIGHT,
                Parent = titleColumnFlowPanel,
            };

            if (stat.Profits.CanBeSoldOnTp)
            {
                // TP Sell title
                new IconLabel("TP Sell", services.TextureService.TradingPostTexture, ROW_HEIGHT, font, titleColumnFlowPanel);
                // TP Buy title
                new IconLabel("TP Buy", services.TextureService.TradingPostTexture, ROW_HEIGHT, font, titleColumnFlowPanel);
            }

            if (stat.Profits.CanBeSoldToVendor)
            {
                // Vendor title
                new IconLabel("Vendor", services.TextureService.MerchantTexture, ROW_HEIGHT, font, titleColumnFlowPanel);
            }

            //var maxChildWidth = titleColumnFlowPanel.Children.Max(l => l.Width);
            //foreach (var childControl in titleColumnFlowPanel.Children)
            //{
            //    //((Label)childControl).AutoSizeWidth = false;
            //    childControl.Width = maxChildWidth;
            //}
        }

        private static void AddProfitColumn(
            string columnHeaderText,
            Profit profit,
            Stat stat,
            BitmapFont font,
            Services services,
            Container parent)
        {
            var columnFlowPanel = new FlowPanel
            {
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                HeightSizingMode = SizingMode.AutoSize,
                WidthSizingMode = SizingMode.AutoSize,
                Parent = parent,
            };

            // Profit column header
            var headerLabel = new Label
            {
                Text = columnHeaderText,
                Font = font,
                Height = ROW_HEIGHT,
                HorizontalAlignment = HorizontalAlignment.Center,
                Parent = columnFlowPanel,
            };

            var profitPanels = new List<ProfitPanel>();
            var containers = new List<FixedWidthContainer>();

            if (stat.Profits.CanBeSoldOnTp)
            {
                // TP Sell profit
                var tpSellProfitContainer = new FixedWidthContainer(columnFlowPanel);
                var tpSellProfitPanel = new ProfitPanel("", "", font, services, tpSellProfitContainer, ROW_HEIGHT);
                tpSellProfitPanel.SetProfit(stat.CountSign * profit.TpSellProfitInCopper);
                profitPanels.Add(tpSellProfitPanel);
                containers.Add(tpSellProfitContainer);

                // TP Buy profit
                var tpBuyProfitContainer = new FixedWidthContainer(columnFlowPanel);
                var tpBuyProfitPanel = new ProfitPanel("", "", font, services, tpBuyProfitContainer, ROW_HEIGHT);
                tpBuyProfitPanel.SetProfit(stat.CountSign * profit.TpBuyProfitInCopper);
                profitPanels.Add(tpBuyProfitPanel);
                containers.Add(tpBuyProfitContainer);
            }

            if (stat.Profits.CanBeSoldToVendor)
            {
                // Vendor profit
                var vendorProfitContainer = new FixedWidthContainer(columnFlowPanel);
                var vendorProfitPanel = new ProfitPanel("", "", font, services, vendorProfitContainer, ROW_HEIGHT);
                vendorProfitPanel.SetProfit(stat.CountSign * profit.VendorProfitInCopper);
                profitPanels.Add(vendorProfitPanel);
                containers.Add(vendorProfitContainer);
            }

            foreach (var profitPanel in profitPanels)
            {
                profitPanel.ContentResized += (s, e) =>
                {
                    var maxProfitPanelWidth = profitPanels.Max(p => p.Width);
                    var columnWidth = maxProfitPanelWidth + 30;

                    headerLabel.Width = columnWidth;

                    for (int i = 0; i < containers.Count; i++)
                    {
                        containers[i].Width = columnWidth;
                        profitPanels[i].Right = columnWidth;
                    }
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

        private const int ROW_HEIGHT = 22;
    }
}