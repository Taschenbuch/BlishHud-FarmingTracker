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
            {
                farmedItemsFlowPanel.ClearChildren();
                ControlFactory.CreateHintLabel(farmedItemsFlowPanel, "No item changes detected!");
            }

            if (noCurrenciesFarmed)
            {
                farmedCurrenciesFlowPanel.ClearChildren();
                ControlFactory.CreateHintLabel(farmedCurrenciesFlowPanel, "No currency changes detected!");
            }

            if (noItemsFarmed && noCurrenciesFarmed)
                return;

            var sortedCurrencies = currencyById.Values.Where(c => !c.IsCoin).OrderBy(c => c.ApiId).ToList();
            var sortedItems = itemById.Values.OrderBy(i => i.ApiId).ToList();
            var currencyControls = CreateItems(sortedCurrencies, services);
            var itemControls = CreateItems(sortedItems, services);

            Hacks.ClearAndAddChildrenWithoutUiFlickering(itemControls, farmedItemsFlowPanel);
            Hacks.ClearAndAddChildrenWithoutUiFlickering(currencyControls, farmedCurrenciesFlowPanel);
        }

        private static ControlCollection<Control> CreateItems(List<ItemX> items, Services services)
        {
            var controls = new ControlCollection<Control>();

            foreach (var item in items)
                controls.Add(CreateItem(item, services));

            return controls;
        }

        private static LocationContainer CreateItem(ItemX item, Services services)
        {
            var itemContainer = new LocationContainer()
            {
                Size = new Point(60), // fixed size prevents that the UI is flickering even thought nekres workaround is used.
            };

            new Image(AsyncTexture2D.FromAssetId(item.IconAssetId))
            {
                BasicTooltipText = item.Tooltip,
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
