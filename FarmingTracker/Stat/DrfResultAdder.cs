using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace FarmingTracker
{
    public class DrfResultAdder
    {
        public static void UpdateItemById(List<DrfMessage> drfMessages, ConcurrentDictionary<int, Stat> itemById)
        {
            var itemIdAndCounts = drfMessages.SelectMany(d => d.Payload.Drop.Items);
            UpdateStatById(itemIdAndCounts, itemById, StatType.Item);
        }

        public static void UpdateCurrencyById(List<DrfMessage> drfMessages, ConcurrentDictionary<int, Stat> currencyById)
        {
            var currencyIdAndCounts = drfMessages.SelectMany(d => d.Payload.Drop.Currencies);
            UpdateStatById(currencyIdAndCounts, currencyById, StatType.Currency);
        }

        private static void UpdateStatById(IEnumerable<KeyValuePair<int, long>> statidAndCounts, ConcurrentDictionary<int, Stat> statById, StatType statType)
        {
            foreach (var statIdAndCount in statidAndCounts)
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
