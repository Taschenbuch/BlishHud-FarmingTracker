using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace FarmingTracker
{
    public class StatsService
    {
        public static (List<Stat> items, List<Stat> currencies) ShallowCopyStatsToPreventModification(Model model)
        {
            var items = model.ItemById.Values.ToList();
            var currencies = model.CurrencyById.Values.ToList();

            return (items, currencies);
        }

        // normally after an api error, the UI is not updated. So stats that did not get api details yet, do not show up in the UI until the next success api call.
        // But when the UI is updated due to a user action (changed sort, changed filter, ...), those missing-details-stats would be displayed without name, icon, tooltip.
        // this method prevents that they are displayed.
        public static (List<Stat> items, List<Stat> currencies) RemoveStatsNotUpdatedYetDueToApiError(List<Stat> items, List<Stat> currencies)
        {
            items = items.Where(s => s.Details.State != ApiStatDetailsState.MissingBecauseApiNotCalledYet).ToList();
            currencies = currencies.Where(s => s.Details.State != ApiStatDetailsState.MissingBecauseApiNotCalledYet).ToList();

            return (items, currencies);
        }

        public static (List<Stat> items, List<Stat> currencies) RemoveZeroCountStats(List<Stat> items, List<Stat> currencies)
        {
            items = items.Where(s => s.Count != 0).ToList();
            currencies = currencies.Where(s => s.Count != 0).ToList();
         
            return (items, currencies);
        }

        public static void ResetCounts(ConcurrentDictionary<int, Stat> statById)
        {
            foreach (var stat in statById.Values)
                stat.Count = 0;
        }

        public static List<Stat> RemoveIgnoredItems(List<Stat> items, List<int> ignoredItemApiIds)
        {
            return items.Where(s => !ignoredItemApiIds.Contains(s.ApiId)).ToList();
        }
    }
}
