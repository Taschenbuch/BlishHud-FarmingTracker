using System.Collections.Generic;
using System.Linq;

namespace FarmingTracker
{
    public class StatsService
    {
        public static (List<Stat> items, List<Stat> currencies) ShallowCopyStatsToPreventModification(StatsSnapshot snapshot) // todo x weg?
        {
            var items = snapshot.ItemById.Values.ToList();
            var currencies = snapshot.CurrencyById.Values.ToList();
            return (items, currencies);
        }

        public static void ResetCounts(Dictionary<int, Stat> statById)
        {
            foreach (var stat in statById.Values)
                stat.Signed_Count = 0;
        }
    }
}
