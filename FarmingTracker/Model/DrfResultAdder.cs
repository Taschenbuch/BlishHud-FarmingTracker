using System.Collections.Generic;
using System.Linq;

namespace FarmingTracker
{
    public class DrfResultAdder
    {
        // WARNING:
        // do not remove stats when their count becomes 0. this may trigger bugs with gw2sharp when it tries to partially read from cache and partially from api
        // but wont find the ids in the api.
        public static void UpdateCountsOrAddNewStats(List<DrfMessage> drfMessages, Dictionary<int, Stat> statsById)
        {
            var itemIdAndCounts = drfMessages.SelectMany(d => d.Payload.Drop.Items);
            InternalUpdateCountsOrAddNewStats(itemIdAndCounts, StatType.Item, statsById);

            var currencyIdAndCounts = drfMessages.SelectMany(d => d.Payload.Drop.Currencies);
            InternalUpdateCountsOrAddNewStats(currencyIdAndCounts, StatType.Currency, statsById);
        }

        private static void InternalUpdateCountsOrAddNewStats(IEnumerable<KeyValuePair<int, long>> statIdAndCounts, StatType statType, Dictionary<int, Stat> statById)
        {
            var statTypeFactor = statType == StatType.Currency ? -1 : 1;

            foreach (var statIdAndCount in statIdAndCounts)
            {
                var key = statTypeFactor * statIdAndCount.Key;

                if (statById.TryGetValue(key, out var stat))
                    stat.Signed_Count += statIdAndCount.Value;
                else
                    statById[key] = new Stat
                    {
                        ApiId = statIdAndCount.Key,
                        StatType = statType,
                        Signed_Count = statIdAndCount.Value,
                    };
            }
        }
    }
}
