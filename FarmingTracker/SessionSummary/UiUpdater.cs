using Blish_HUD.Controls;
using System.Collections.Generic;
using System.Linq;

namespace FarmingTracker
{
    public class UiUpdater
    {
        public static void UpdateStatPanels(StatsPanels statsPanels, Services services)
        {
            var currencies = services.Stats.CurrencyById.Values.Where(c => !c.IsCoin).ToList(); // remove coin before the counting-to-check-if-stats-were-filtered
            var items = services.Stats.ItemById.Values.ToList();

            (items, currencies) = RemoveStatsNotUpdatedYetDueToApiError(items, currencies, services);
            (items, currencies) = SearchService.FilterBySearchTerm(items, currencies, services.SearchTerm);
            (items, currencies) = FilterService.FilterStatsAndSetFunnelOpacity(items, currencies, statsPanels, services);
            (items, currencies) = SortService.SortStats(items, currencies, services);

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

        private static (List<Stat> items, List<Stat> currencies) RemoveStatsNotUpdatedYetDueToApiError(List<Stat> items, List<Stat> currencies, Services services)
        {
            items = RemoveStatsNotUpdatedYetDueToApiError(items);
            currencies = RemoveStatsNotUpdatedYetDueToApiError(currencies);

            return (items, currencies);
        }

        // normally after an api error, the UI is not updated. So stats that did not get api details yet, do not show up in the UI until the next success api call.
        // But when the UI is updated due to a user action (changed sort, changed filter, ...), those missing-details-stats would be displayed without name, icon, tooltip.
        // this method prevents that they are displayed.
        private static List<Stat> RemoveStatsNotUpdatedYetDueToApiError(List<Stat> stats)
        {
            return stats
                .Where(c => c.Details.State != ApiStatDetailsState.MissingBecauseApiNotCalledYet)
                .ToList();
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
