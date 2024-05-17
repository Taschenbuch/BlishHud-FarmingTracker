using Blish_HUD.Controls;
using System.Collections.Generic;
using System.Linq;

namespace FarmingTracker
{
    public class UiUpdater
    {
        public static void UpdateStatsInUi(StatsPanels statsPanels, Services services)
        {
            var items = FilterItems(services.Stats.ItemById.Values, services);

            items = items
                .OrderBy(i => i.Count >= 0 ? -1 : 1)
                .ThenBy(i => i.ApiId);

            var sortedCurrencies = services.Stats.CurrencyById.Values
                .Where(c => !c.IsCoin)
                .OrderBy(c => c.ApiId)
                .ToList();

            var currencyControls = CreateStatControls(sortedCurrencies, services);
            var itemControls = CreateStatControls(items.ToList(), services);

            Hacks.ClearAndAddChildrenWithoutUiFlickering(itemControls, statsPanels.FarmedItemsFlowPanel);
            Hacks.ClearAndAddChildrenWithoutUiFlickering(currencyControls, statsPanels.FarmedCurrenciesFlowPanel);

            var noItemChangesDetected = !services.Stats.ItemById.Any();
            var noCurrencyChangesDetected = !services.Stats.CurrencyById.Any();

            if (statsPanels.FarmedItemsFlowPanel.IsEmpty())
                ControlFactory.CreateHintLabel(statsPanels.FarmedItemsFlowPanel, $"{PADDING}No item changes detected!");

            if (statsPanels.FarmedCurrenciesFlowPanel.IsEmpty())
                ControlFactory.CreateHintLabel(statsPanels.FarmedCurrenciesFlowPanel, $"{PADDING}No currency changes detected!");
        }

        private static IEnumerable<Stat> FilterItems(IEnumerable<Stat> items, Services services)
        {
            var rarityFilter = services.SettingService.RarityStatsFilterSetting.Value;
            if (rarityFilter.Any()) // prevents that all items are hidden, when no filter is set
                items = items.Where(i => rarityFilter.Contains(i.Details.Rarity));

            var typeFilter = services.SettingService.TypeStatsFilterSetting.Value;
            if (typeFilter.Any()) // prevents that all items are hidden, when no filter is set
                items = items.Where(i => typeFilter.Contains(i.Details.Type));

            var flagFilter = services.SettingService.FlagStatsFilterSetting.Value;
            if (flagFilter.Any()) // prevents that all items are hidden, when no filter is set
                items = items.Where(i => i.Details.Flag.List.Any(f => flagFilter.Contains(f)));

            return items;
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
