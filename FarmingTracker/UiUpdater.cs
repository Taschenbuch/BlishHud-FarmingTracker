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
            var noItemsFarmed = !itemById.Any();
            var noCurrenciesFarmed = !currencyById.Any();

            if (noItemsFarmed)
                ControlFactory.CreateHintLabel(farmedItemsFlowPanel, "No item changes detected!");

            if (noCurrenciesFarmed)
                ControlFactory.CreateHintLabel(farmedCurrenciesFlowPanel, "No currency changes detected!");

            if (noItemsFarmed && noCurrenciesFarmed)
                return;

            var items = CreateItems(itemById);
            var currencies = CreateItems(currencyById);

            Hacks.ClearAndAddChildrenWithoutUiFlickering(items, farmedItemsFlowPanel);
            Hacks.ClearAndAddChildrenWithoutUiFlickering(currencies, farmedCurrenciesFlowPanel);
        }

        private static ControlCollection<Control> CreateItems(Dictionary<int, ItemX> itemById)
        {
            var items = new ControlCollection<Control>();

            foreach (var farmedItem in itemById.Values)
                items.Add(CreateItem(farmedItem));

            return items;
        }

        private static LocationContainer CreateItem(ItemX item)
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
