using Blish_HUD.Controls;
using System.Collections.Generic;
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
                controls.Add(new StatContainer(item, services));

            return controls;
        }
    }
}
