using Blish_HUD.Controls;
using System.Collections.Generic;

namespace FarmingTracker
{
    public class UiUpdater
    {
        public static void UpdateStatPanels(StatsPanels statsPanels, StatsSnapshot snapshot, Model model, Services services)
        {
            var favoriteStats = model.FavoriteStats.ToListSafe(); // dont use this snapshot inside StatControls. statcontrols have to update the list.
            var customStatProfits = model.CustomStatProfits.ToListSafe(); // dont use this snapshot inside StatControls. statcontrols have to update the list.
            List<Stat> favorites;

            // todo x sobald itemById und currenciesById weg sind -> direkt auf stats arbeiten bevor man es in items, currcies splittet? müsste bei ignore und count = 0 entfernen usw. funktionieren
            var (items, currencies) = StatsService.ShallowCopyStatsToPreventModification(snapshot);
            (items, currencies) = StatsService.RemoveZeroCountStats(items, currencies); // dont call this AFTER the coin splitter. it would remove them.
            (items, currencies) = StatsService.RemoveStatsNotUpdatedYetDueToApiError(items, currencies);
            (items, currencies, favorites) = StatsService.SplitFavoritesFromCurrenciesAndItems(items, currencies, favoriteStats);
            (items, currencies) = StatsService.RemoveIgnoredStats(items, currencies, model);
            currencies = CoinSplitter.ReplaceCoinWithGoldSilverCopperStats(currencies);
            favorites = CoinSplitter.ReplaceCoinWithGoldSilverCopperStats(favorites);
            (items, currencies) = SearchService.FilterBySearchTerm(items, currencies, services.SearchTerm);
            (items, currencies) = FilterService.FilterStatsAndSetFunnelOpacity(items, currencies, customStatProfits, statsPanels, services.SettingService);
            (items, currencies) = SortService.SortStats(items, currencies, services.SettingService);

            var favoriteStatsControls = CreateStatControls(favorites, PanelType.SummaryFavorites, model, model.FavoriteStats, model.CustomStatProfits, services);
            var currencyControls = CreateStatControls(currencies, PanelType.SummaryCurrencies, model, model.FavoriteStats, model.CustomStatProfits, services);
            var itemControls = CreateStatControls(items, PanelType.SummaryItems, model, model.FavoriteStats, model.CustomStatProfits, services);

            if (currencyControls.IsEmpty())
                currencyControls.Add(new HintLabel($"{Constants.HINT_IN_PANEL_PADDING}No currency changes detected!"));

            if (itemControls.IsEmpty())
                itemControls.Add(new HintLabel($"{Constants.HINT_IN_PANEL_PADDING}No item changes detected!"));

            if (favoriteStatsControls.IsEmpty())
            {
                if(favoriteStats.IsEmpty())
                    favoriteStatsControls.Add(new HintLabel($"{Constants.HINT_IN_PANEL_PADDING}Right click item to add to favorites!"));
                else
                    favoriteStatsControls.Add(new HintLabel($"{Constants.HINT_IN_PANEL_PADDING}No favorite item changes detected!"));
            }

            Hacks.ClearAndAddChildrenWithoutUiFlickering(favoriteStatsControls, statsPanels.FavoriteStatsFlowPanel);
            Hacks.ClearAndAddChildrenWithoutUiFlickering(itemControls, statsPanels.ItemsFlowPanel);
            Hacks.ClearAndAddChildrenWithoutUiFlickering(currencyControls, statsPanels.CurrenciesFlowPanel);
        }

        private static ControlCollection<Control> CreateStatControls(
            List<Stat> stats, 
            PanelType panelType,
            Model model, 
            SafeList<FavoriteStat> favoriteStats,
            SafeList<CustomStatProfit> customStatProfits,
            Services services)
        {
            var controls = new ControlCollection<Control>();

            foreach (var stat in stats)
                controls.Add(new StatContainer(stat, panelType, model, favoriteStats, customStatProfits, services));

            return controls;
        }
    }
}
