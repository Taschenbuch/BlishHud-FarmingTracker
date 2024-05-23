using System.Collections.Generic;
using System.Linq;

namespace FarmingTracker
{
    public class RemoveStatsService
    {
        public static (List<Stat> items, List<Stat> currencies) RemoveStatsNotUpdatedYetDueToApiError(List<Stat> items, List<Stat> currencies)
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
    }
}
