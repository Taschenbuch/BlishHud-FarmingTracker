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
            var noItemChanges = !stats.ItemById.Any();
            var noCurrencyChanges = !stats.CurrencyById.Any();

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
