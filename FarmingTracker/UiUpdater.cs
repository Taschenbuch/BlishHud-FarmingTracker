using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD;
using System.Collections.Generic;
using static Blish_HUD.ContentService;
using Microsoft.Xna.Framework;
using System.Linq;

namespace FarmingTracker
{
    public class UiUpdater
    {
        public static void UpdateUi(
            List<ItemX> farmedCurrencies,
            List<ItemX> farmedItems,
            FlowPanel farmedCurrenciesFlowPanel,
            FlowPanel farmedItemsFlowPanel)
        {
            farmedItemsFlowPanel.ClearChildren();
            farmedCurrenciesFlowPanel.ClearChildren();

            if(!farmedItems.Any())
            {
                new Label 
                { 
                    Text = "No item changes detected yet!\nUpdates from the API can take several minutes!",
                    AutoSizeWidth = true,
                    AutoSizeHeight = true,
                    Parent = farmedItemsFlowPanel
                };
            }

            if (!farmedCurrencies.Any())
            {
                new Label
                {
                    Text = "No currency changes detected yet!\nUpdates from the API can take several minutes!",
                    AutoSizeWidth = true,
                    AutoSizeHeight = true,
                    Parent = farmedCurrenciesFlowPanel
                };
            }

            foreach (var farmedItem in farmedItems)
                AddItemToUi(farmedItem, farmedItemsFlowPanel);

            foreach (var farmedCurrency in farmedCurrencies)
                AddItemToUi(farmedCurrency, farmedCurrenciesFlowPanel);
        }

        private static void AddItemToUi(ItemX item, Container parent)
        {
            var itemContainer = new LocationContainer()
            {
                WidthSizingMode = SizingMode.AutoSize,
                HeightSizingMode = SizingMode.AutoSize,
                Parent = parent
            };

            var tooltipText = string.IsNullOrWhiteSpace(item.Description)
                ? $"{item.Name}\n{item.Count}"
                : $"{item.Name}\n{item.Description}\n{item.Count}";

            new Image(AsyncTexture2D.FromAssetId(item.IconAssetId))
            {
                BasicTooltipText = tooltipText,
                Size = new Point(40),
                Parent = itemContainer
            };

            new Label
            {
                Text = item.Count.ToString(),
                Font = GameService.Content.GetFont(FontFace.Menomonia, FontSize.Size14, FontStyle.Regular), // todo ständige GetFont() calls
                StrokeText = true,
                Parent = itemContainer
            };
        }
    }
}
