using Blish_HUD.Controls;
using System.Collections.Generic;

namespace FarmingTracker
{
    public class UiUpdater
    {
        public static void UpdateStatPanels(StatsPanels statsPanels, StatsSnapshot snapshot, Model model, Services services)
        {
            var favoriteItemApiIds = model.FavoriteItemApiIds.ToListSafe(); // dont use this snapshot inside StatControls. statcontrols have to update the list.

            var (items, currencies) = StatsService.ShallowCopyStatsToPreventModification(snapshot);
            (items, currencies) = StatsService.RemoveZeroCountStats(items, currencies); // dont call this AFTER the coin splitter. it would remove them.
            List<Stat> favoriteItems;
            (favoriteItems, items) = StatsService.SplitIntoFavoriteAndRegularItems(items, favoriteItemApiIds);
            items = StatsService.RemoveIgnoredItems(items, model.IgnoredItemApiIds.ToListSafe());
            currencies = CoinSplitter.ReplaceCoinWithGoldSilverCopperStats(currencies);
            (items, currencies) = StatsService.RemoveStatsNotUpdatedYetDueToApiError(items, currencies);
            (items, currencies) = SearchService.FilterBySearchTerm(items, currencies, services.SearchTerm);
            (items, currencies) = FilterService.FilterStatsAndSetFunnelOpacity(items, currencies, statsPanels, services.SettingService);
            (items, currencies) = SortService.SortStats(items, currencies, services.SettingService);

            var currencyControls = CreateStatControls(currencies, PanelType.SummaryCurrencies, model.IgnoredItemApiIds, model.FavoriteItemApiIds, model.CustomStatProfits, services);
            var favoriteItemsControls = CreateStatControls(favoriteItems, PanelType.SummaryFavoriteItems, model.IgnoredItemApiIds, model.FavoriteItemApiIds, model.CustomStatProfits, services);
            var itemControls = CreateStatControls(items, PanelType.SummaryRegularItems, model.IgnoredItemApiIds, model.FavoriteItemApiIds, model.CustomStatProfits, services);

            if (currencyControls.IsEmpty())
                currencyControls.Add(new HintLabel($"{Constants.HINT_IN_PANEL_PADDING}No currency changes detected!"));

            if (itemControls.IsEmpty())
                itemControls.Add(new HintLabel($"{Constants.HINT_IN_PANEL_PADDING}No item changes detected!"));

            if (favoriteItemsControls.IsEmpty())
            {
                if(favoriteItemApiIds.IsEmpty())
                    favoriteItemsControls.Add(new HintLabel($"{Constants.HINT_IN_PANEL_PADDING}Right click item to add to favorites!"));
                else
                    favoriteItemsControls.Add(new HintLabel($"{Constants.HINT_IN_PANEL_PADDING}No favorite item changes detected!"));
            }

            Hacks.ClearAndAddChildrenWithoutUiFlickering(itemControls, statsPanels.ItemsFlowPanel);
            Hacks.ClearAndAddChildrenWithoutUiFlickering(favoriteItemsControls, statsPanels.FavoriteItemsFlowPanel);
            Hacks.ClearAndAddChildrenWithoutUiFlickering(currencyControls, statsPanels.CurrenciesFlowPanel);
        }

        private static ControlCollection<Control> CreateStatControls(
            List<Stat> stats, 
            PanelType panelType,
            SafeList<int> ignoredItemApiIds, 
            SafeList<int> favoriteItemApiIds,
            SafeList<CustomStatProfit> customStatProfits,
            Services services)
        {
            var controls = new ControlCollection<Control>();

            foreach (var stat in stats)
                controls.Add(new StatContainer(stat, panelType, ignoredItemApiIds, favoriteItemApiIds, customStatProfits, services));

            return controls;
        }
    }
}
