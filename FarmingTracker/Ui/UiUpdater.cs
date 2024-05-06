using Blish_HUD.Controls;
using System.Collections.Generic;
using System.Linq;

namespace FarmingTracker
{
    public class UiUpdater
    {
        public static void UpdateStatsInUi(
            Dictionary<int, Stat> currencyById,
            Dictionary<int, Stat> itemById,
            StatsPanels statsPanels,
            Services services)
        {
            var noItemChanges = !itemById.Any();
            var noCurrencyChanges = !currencyById.Any();

            if (noItemChanges)
            {
                statsPanels.FarmedItemsFlowPanel.ClearChildren();
                ControlFactory.CreateHintLabel(statsPanels.FarmedItemsFlowPanel, $"{PADDING}No item changes detected!");
            }

            if (noCurrencyChanges)
            {
                statsPanels.FarmedCurrenciesFlowPanel.ClearChildren();
                ControlFactory.CreateHintLabel(statsPanels.FarmedCurrenciesFlowPanel, $"{PADDING}No currency changes detected!");
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

            Hacks.ClearAndAddChildrenWithoutUiFlickering(itemControls, statsPanels.FarmedItemsFlowPanel);
            Hacks.ClearAndAddChildrenWithoutUiFlickering(currencyControls, statsPanels.FarmedCurrenciesFlowPanel);
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
