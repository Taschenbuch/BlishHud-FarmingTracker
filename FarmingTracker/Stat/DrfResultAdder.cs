using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace FarmingTracker
{
    public class DrfResultAdder
    {
        public static void UpdateCountsOrAddNewStats(
            List<DrfMessage> drfMessages, 
            ConcurrentDictionary<int, Stat> itemById, 
            ConcurrentDictionary<int, Stat> currencyById)
        {
            var itemIdAndCounts = drfMessages.SelectMany(d => d.Payload.Drop.Items);
            InternalUpdateCountsOrAddNewStats(itemIdAndCounts, itemById, StatType.Item);

            var currencyIdAndCounts = drfMessages.SelectMany(d => d.Payload.Drop.Currencies);
            InternalUpdateCountsOrAddNewStats(currencyIdAndCounts, currencyById, StatType.Currency);
        }

        private static void InternalUpdateCountsOrAddNewStats(
            IEnumerable<KeyValuePair<int, long>> statIdAndCounts, 
            ConcurrentDictionary<int, Stat> statById, 
            StatType statType)
        {
            foreach (var statIdAndCount in statIdAndCounts)
            {
                if (statById.TryGetValue(statIdAndCount.Key, out var stat))
                    stat.Count += statIdAndCount.Value;
                else
                    statById[statIdAndCount.Key] = new Stat
                    {
                        StatType = statType,
                        ApiId = statIdAndCount.Key,
                        Count = statIdAndCount.Value,
                    };
            }
        }
    }
}
