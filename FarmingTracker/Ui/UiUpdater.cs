using Blish_HUD.Controls;
using System.Collections.Generic;
using System.Linq;

namespace FarmingTracker
{
    public class UiUpdater
    {
        public static void UpdateStatsInUi(
            Stats stats,
            StatsPanels statsPanels,
            Services services)
        {
            var sortedCurrencies = stats.CurrencyById.Values
                .Where(c => !c.IsCoin)
                .OrderBy(c => c.ApiId)
                .ToList();

            var sortedItems = stats.ItemById.Values
                .OrderBy(i => i.Count >= 0 ? -1 : 1)
                .ThenBy(i => i.ApiId)
                .ToList();

            var currencyControls = CreateStatControls(sortedCurrencies, services);
            var itemControls = CreateStatControls(sortedItems, services);

            Hacks.ClearAndAddChildrenWithoutUiFlickering(itemControls, statsPanels.FarmedItemsFlowPanel);
            Hacks.ClearAndAddChildrenWithoutUiFlickering(currencyControls, statsPanels.FarmedCurrenciesFlowPanel);

            var noItemChangesDetected = !stats.ItemById.Any();
            var noCurrencyChangesDetected = !stats.CurrencyById.Any();

            if(statsPanels.FarmedItemsFlowPanel.IsEmpty())
                ControlFactory.CreateHintLabel(statsPanels.FarmedItemsFlowPanel, $"{PADDING}No item changes detected!");
            
            if(statsPanels.FarmedCurrenciesFlowPanel.IsEmpty())
                ControlFactory.CreateHintLabel(statsPanels.FarmedCurrenciesFlowPanel, $"{PADDING}No currency changes detected!");
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
