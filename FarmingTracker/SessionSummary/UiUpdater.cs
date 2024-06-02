using Blish_HUD.Controls;
using System.Collections.Generic;
using System.Linq;

namespace FarmingTracker
{
    public class UiUpdater
    {
        public static void UpdateStatPanels(StatsPanels statsPanels, Services services)
        {
            var (items, currencies) = StatsService.ShallowCopyStatsToPreventModification(services.Model);
            (items, currencies) = StatsService.RemoveZeroCountStats(items, currencies); // dont call this AFTER the coin splitter. it would remove them.
            currencies = CoinSplitter.ReplaceCoinWithGoldSilverCopperStats(currencies);
            (items, currencies) = StatsService.RemoveStatsNotUpdatedYetDueToApiError(items, currencies);
            (items, currencies) = SearchService.FilterBySearchTerm(items, currencies, services.SearchTerm);
            (items, currencies) = FilterService.FilterStatsAndSetFunnelOpacity(items, currencies, statsPanels, services);
            (items, currencies) = SortService.SortStats(items, currencies, services);

            var currencyControls = CreateStatControls(currencies.ToList(), services);
            var itemControls = CreateStatControls(items.ToList(), services);

            Hacks.ClearAndAddChildrenWithoutUiFlickering(itemControls, statsPanels.FarmedItemsFlowPanel);
            Hacks.ClearAndAddChildrenWithoutUiFlickering(currencyControls, statsPanels.FarmedCurrenciesFlowPanel);

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
