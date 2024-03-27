using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD;
using System.Collections.Generic;
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
            FlowPanel farmedItemsFlowPanel,
            Services services)
        {
            var noItemsFarmed = !itemById.Any();
            var noCurrenciesFarmed = !currencyById.Any();

            if (noItemsFarmed)
                ControlFactory.CreateHintLabel(farmedItemsFlowPanel, "No item changes detected!");

            if (noCurrenciesFarmed)
                ControlFactory.CreateHintLabel(farmedCurrenciesFlowPanel, "No currency changes detected!");

            if (noItemsFarmed && noCurrenciesFarmed)
                return;

            var items = CreateItems(itemById, services);
            var currencies = CreateItems(currencyById, services);

            Hacks.ClearAndAddChildrenWithoutUiFlickering(items, farmedItemsFlowPanel);
            Hacks.ClearAndAddChildrenWithoutUiFlickering(currencies, farmedCurrenciesFlowPanel);
        }

        private static ControlCollection<Control> CreateItems(Dictionary<int, ItemX> itemById, Services services)
        {
            var items = new ControlCollection<Control>();

            foreach (var farmedItem in itemById.Values)
                items.Add(CreateItem(farmedItem, services));

            return items;
        }

        private static LocationContainer CreateItem(ItemX item, Services services)
        {
            var itemContainer = new LocationContainer()
            {
                Size = new Point(60), // without fixed size the UI will start flickering again
            };

            var tooltipText = string.IsNullOrWhiteSpace(item.Description)
                ? $"{item.Count} {item.Name}"
                : $"{item.Count} {item.Name}\n{item.Description}";

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
                Font = services.FontService.Fonts[ContentService.FontSize.Size16],
                AutoSizeHeight = true,
                AutoSizeWidth = true,
                StrokeText = true,
                Parent = itemContainer
            };

            return itemContainer;
        }
    }
}
