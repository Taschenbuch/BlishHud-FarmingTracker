using Blish_HUD.Controls;
using System.Collections.Generic;
using System.Linq;

namespace FarmingTracker
{
    public class UiUpdater
    {
        public static void UpdateStatPanels(StatsPanels statsPanels, Model model, Services services)
        {
            // normally after an api error, the UI is not updated. So stats that did not get api details yet, do not show up in the UI until the next success api call.
            // But when the UI is updated due to a user action (changed sort, changed filter, ...), those missing-details-stats would be displayed without name, icon, tooltip.
            // this method prevents that they are displayed.
            var stats = model.Stats.StatById.Values // todo x lock?
               .Where(s => s.Signed_Count.Value != 0) // dont call this AFTER the coin splitter. it would remove them.
               .Where(s => s.Details.State != ApiStatDetailsState.MissingBecauseApiNotCalledYet)
               .Where(s => SearchService.IncludesSearchTerm(s, services.SearchTerm))
               .ToList();
            
            var items = stats.Where(s => s.IsItem).Where(s => s.StatVisibility == StatVisibility.Regular).ToList();
            var currencies = stats.Where(s => s.IsCurrency).Where(s => s.StatVisibility == StatVisibility.Regular).ToList();
            var favoriteStats = stats.Where(s => s.StatVisibility == StatVisibility.Favorite).OrderBy(f => f.StatType).ToList(); // OrderBy to show always show currencies first.

            currencies = CoinSplitter.ReplaceCoinWithGoldSilverCopperStats(currencies);
            favoriteStats = CoinSplitter.ReplaceCoinWithGoldSilverCopperStats(favoriteStats);
            (items, currencies) = FilterService.FilterStatsAndSetFunnelOpacity(items, currencies, statsPanels, services.SettingService);
            (items, currencies) = SortService.SortStats(items, currencies, services.SettingService);

            var favoriteStatsControls = CreateStatControls(favoriteStats, PanelType.SummaryFavorites, model, services);
            var currencyControls = CreateStatControls(currencies, PanelType.SummaryCurrencies, model, services);
            var itemControls = CreateStatControls(items, PanelType.SummaryItems, model, services);

            if (currencyControls.IsEmpty())
                currencyControls.Add(new HintLabel($"{Constants.HINT_IN_PANEL_PADDING}No currency changes detected!"));

            if (itemControls.IsEmpty())
                itemControls.Add(new HintLabel($"{Constants.HINT_IN_PANEL_PADDING}No item changes detected!"));

            if (favoriteStatsControls.IsEmpty())
            {
                if(favoriteStats.IsEmpty())
                    favoriteStatsControls.Add(new HintLabel($"{Constants.HINT_IN_PANEL_PADDING}Right click item or currency to add to favorites!"));
                else
                    favoriteStatsControls.Add(new HintLabel($"{Constants.HINT_IN_PANEL_PADDING}No favorite item changes detected!"));
            }

            Hacks.ClearAndAddChildrenWithoutUiFlickering(favoriteStatsControls, statsPanels.FavoriteStatsFlowPanel);
            Hacks.ClearAndAddChildrenWithoutUiFlickering(itemControls, statsPanels.ItemsFlowPanel);
            Hacks.ClearAndAddChildrenWithoutUiFlickering(currencyControls, statsPanels.CurrenciesFlowPanel);
        }

        private static ControlCollection<Control> CreateStatControls(List<Stat> stats, PanelType panelType, Model model, Services services)
        {
            var controls = new ControlCollection<Control>();

            foreach (var stat in stats)
                controls.Add(new StatContainer(stat, panelType, model, services));

            return controls;
        }
    }
}
