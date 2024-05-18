using Blish_HUD.Controls;
using System.Collections.Generic;
using System.Linq;

namespace FarmingTracker
{
    public class UiUpdater
    {
        public static void UpdateStatPanels(StatsPanels statsPanels, Services services)
        {
            var items = FilterService.FilterItems(services.Stats.ItemById.Values, services);
            var currencies = FilterService.FilterCurrencies(services.Stats.CurrencyById.Values, services);

            items = items
                .OrderBy(i => i.Count >= 0 ? -1 : 1)
                .ThenBy(i => i.ApiId);

            var sortedCurrencies = currencies
                .OrderBy(c => c.ApiId)
                .ToList();

            var currencyControls = CreateStatControls(sortedCurrencies, services);
            var itemControls = CreateStatControls(items.ToList(), services);

            Hacks.ClearAndAddChildrenWithoutUiFlickering(itemControls, statsPanels.FarmedItemsFlowPanel);
            Hacks.ClearAndAddChildrenWithoutUiFlickering(currencyControls, statsPanels.FarmedCurrenciesFlowPanel);

            var noItemChangesDetected = !services.Stats.ItemById.Any();
            var noCurrencyChangesDetected = !services.Stats.CurrencyById.Any();

            if (statsPanels.FarmedItemsFlowPanel.IsEmpty())
                new HintLabel(statsPanels.FarmedItemsFlowPanel, $"{PADDING}No item changes detected!");

            if (statsPanels.FarmedCurrenciesFlowPanel.IsEmpty())
                new HintLabel(statsPanels.FarmedCurrenciesFlowPanel, $"{PADDING}No currency changes detected!");
        }

        private static ControlCollection<Control> CreateStatControls(List<Stat> stats, Services services)
        {
            var controls = new ControlCollection<Control>();

            foreach (var stat in stats)
                controls.Add(new StatContainer(stat, services));

            return controls;
        }

        private const string PADDING = "  ";
    }
}
