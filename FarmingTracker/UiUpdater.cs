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
            Dictionary<int, ItemX> currencyById, 
            Dictionary<int, ItemX> itemById, 
            FlowPanel farmedCurrenciesFlowPanel, 
            FlowPanel farmedItemsFlowPanel)
        {
            if (!itemById.Any())
            {
                new Label
                {
                    Text = "No item changes detected yet!",
                    AutoSizeWidth = true,
                    AutoSizeHeight = true,
                    Parent = farmedItemsFlowPanel
                };
            }

            if (!currencyById.Any())
            {
                new Label
                {
                    Text = "No currency changes detected yet!",
                    AutoSizeWidth = true,
                    AutoSizeHeight = true,
                    Parent = farmedCurrenciesFlowPanel
                };
            }

            var items = new ControlCollection<Control>();
            var currencies = new ControlCollection<Control>();

            foreach (var farmedItem in itemById.Values)
                items.Add(CreateItem(farmedItem));

            foreach (var farmedCurrency in currencyById.Values)
                currencies.Add(CreateItem(farmedCurrency));

            Hacks.AddChildrenWithoutUiFlickering(items, farmedItemsFlowPanel);
            Hacks.AddChildrenWithoutUiFlickering(currencies, farmedCurrenciesFlowPanel);
        }
        private static LocationContainer CreateItem(ItemX item)
        {
            var itemContainer = new LocationContainer()
            {
                Size = new Point(60), // without fixed size the UI will start flickering again
            };

            var tooltipText = string.IsNullOrWhiteSpace(item.Description)
                ? $"{item.Name}\n{item.Count}" // todo hin
                : $"{item.Name}\n{item.Description}\n{item.Count}";

            var iconAssetId = item.IconAssetId == 0
                ? 157084
                : item.IconAssetId;

            new Image(AsyncTexture2D.FromAssetId(iconAssetId))
            {
                BasicTooltipText = tooltipText,
                Opacity = item.Count > 0 ? 1f : 0.3f,
                Size = new Point(60),
                Parent = itemContainer
            };

            new Label
            {
                Text = item.Count.ToString(),
                Font = GameService.Content.GetFont(FontFace.Menomonia, FontSize.Size16, FontStyle.Regular), // todo ständige GetFont() calls
                AutoSizeHeight = true,
                AutoSizeWidth = true,
                StrokeText = true,
                Parent = itemContainer
            };

            return itemContainer;
        }
    }
}
