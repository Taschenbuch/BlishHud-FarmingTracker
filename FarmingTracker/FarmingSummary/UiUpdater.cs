using Blish_HUD.Controls;
using System.Collections.Generic;
using System.Linq;

namespace FarmingTracker
{
    public class UiUpdater
    {
        public static void UpdateStatPanels(StatsPanels statsPanels, Services services)
        {
            var currencies = services.Stats.CurrencyById.Values.Where(c => !c.IsCoin).ToList(); // removing coin does not count to filtering by user settings.
            var items = services.Stats.ItemById.Values.ToList();
            
            var currenciesCountBeforeFiltering = currencies.Count();
            var itemsCountBeforeFiltering = items.Count();

            currencies = FilterService.FilterCurrencies(currencies, services);
            items = FilterService.FilterItems(items, services);

            currencies = currencies
                .OrderBy(c => c.ApiId)
                .ToList();

            items = items
                .OrderBy(i => i.Count >= 0 ? -1 : 1)
                .ThenBy(i => i.ApiId)
                .ToList();
            
            var noCurrenciesHiddenByFilter = currencies.Count() == currenciesCountBeforeFiltering;
            var noItemsHiddenByFilter = items.Count() == itemsCountBeforeFiltering;

            statsPanels.CurrencyFilterIcon.SetOpacity(noCurrenciesHiddenByFilter);
            statsPanels.ItemsFilterIcon.SetOpacity(noItemsHiddenByFilter);

            var currencyControls = CreateStatControls(currencies.ToList(), services);
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
