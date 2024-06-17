using Blish_HUD.Controls;
using System.Collections.Generic;

namespace FarmingTracker
{
    public class UiUpdater
    {
        public static void UpdateStatPanels(StatsPanels statsPanels, StatsSnapshot snapshot, Services services)
        {
            var (items, currencies) = StatsService.ShallowCopyStatsToPreventModification(snapshot);
            (items, currencies) = StatsService.RemoveZeroCountStats(items, currencies); // dont call this AFTER the coin splitter. it would remove them.
            items = StatsService.RemoveIgnoredItems(items, services.Model.IgnoredItemApiIds.ToListSafe());
            currencies = CoinSplitter.ReplaceCoinWithGoldSilverCopperStats(currencies);
            (items, currencies) = StatsService.RemoveStatsNotUpdatedYetDueToApiError(items, currencies);
            (items, currencies) = SearchService.FilterBySearchTerm(items, currencies, services.SearchTerm);
            (items, currencies) = FilterService.FilterStatsAndSetFunnelOpacity(items, currencies, statsPanels, services.SettingService);
            (items, currencies) = SortService.SortStats(items, currencies, services.SettingService);

            var currencyControls = CreateStatControls(currencies, services);
            var itemControls = CreateStatControls(items, services);

            if (currencyControls.IsEmpty())
                currencyControls.Add(new HintLabel($"{Constants.HINT_IN_PANEL_PADDING}No currency changes detected!"));

            if (itemControls.IsEmpty())
                itemControls.Add(new HintLabel($"{Constants.HINT_IN_PANEL_PADDING}No item changes detected!"));

            Hacks.ClearAndAddChildrenWithoutUiFlickering(itemControls, statsPanels.ItemsFlowPanel);
            Hacks.ClearAndAddChildrenWithoutUiFlickering(currencyControls, statsPanels.CurrenciesFlowPanel);
        }

        private static ControlCollection<Control> CreateStatControls(List<Stat> stats, Services services)
        {
            var controls = new ControlCollection<Control>();

            foreach (var stat in stats)
                controls.Add(new StatContainer(stat, PanelType.SessionSummary, services));

            return controls;
        }
    }
}
