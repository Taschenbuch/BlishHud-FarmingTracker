using Blish_HUD.Controls;
using System.Collections.Generic;
using System.Linq;

namespace FarmingTracker
{
    public class UiUpdater
    {
        public static void UpdateUi(
            Dictionary<int, Stat> currencyById,
            Dictionary<int, Stat> itemById,
            FlowPanel farmedCurrenciesFlowPanel,
            FlowPanel farmedItemsFlowPanel,
            Services services)
        {
            var noItemChanges = !itemById.Any();
            var noCurrencyChanges = !currencyById.Any();

            if (noItemChanges)
            {
                farmedItemsFlowPanel.ClearChildren();
                ControlFactory.CreateHintLabel(farmedItemsFlowPanel, $"{PADDING}No item changes detected!");
            }

            if (noCurrencyChanges)
            {
                farmedCurrenciesFlowPanel.ClearChildren();
                ControlFactory.CreateHintLabel(farmedCurrenciesFlowPanel, $"{PADDING}No currency changes detected!");
            }

            if (noItemChanges && noCurrencyChanges)
                return;

            var sortedCurrencies = currencyById.Values
                .Where(c => !c.IsCoin)
                .OrderBy(c => c.ApiId)
                .ToList();

            var sortedItems = itemById.Values
                .OrderBy(i => i.Count >= 0 ? -1 : 1)
                .ThenBy(i => i.ApiId)
                .ToList();

            var currencyControls = CreateItems(sortedCurrencies, services);
            var itemControls = CreateItems(sortedItems, services);

            Hacks.ClearAndAddChildrenWithoutUiFlickering(itemControls, farmedItemsFlowPanel);
            Hacks.ClearAndAddChildrenWithoutUiFlickering(currencyControls, farmedCurrenciesFlowPanel);
        }

        private static ControlCollection<Control> CreateItems(List<Stat> items, Services services)
        {
            var controls = new ControlCollection<Control>();

            foreach (var item in items)
                controls.Add(new StatContainer(item, services));

            return controls;
        }

        private const string PADDING = "  ";
    }
}
