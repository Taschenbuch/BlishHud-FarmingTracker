using System.Collections.Generic;

namespace FarmingTracker
{
    public class StatsService
    {
        public static void ResetCounts(Dictionary<int, Stat> statById)
        {
            foreach (var stat in statById.Values)
                stat.Signed_Count = 0;
        }
    }
}
