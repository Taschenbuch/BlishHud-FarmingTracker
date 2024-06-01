using System.Collections.Generic;
using System.Linq;

namespace FarmingTracker
{
    public class DrfResultAdder
    {
        public static void UpdateItemById(List<DrfMessage> drfMessages, Dictionary<int, Stat> itemById)
        {
            var itemIdAndCounts = drfMessages.SelectMany(d => d.Payload.Drop.Items);
            UpdateStatById(itemIdAndCounts, itemById);
        }

        public static void UpdateCurrencyById(List<DrfMessage> drfMessages, Dictionary<int, Stat> currencyById)
        {
            var currencyIdAndCounts = drfMessages.SelectMany(d => d.Payload.Drop.Currencies);
            UpdateStatById(currencyIdAndCounts, currencyById);
        }

        private static void UpdateStatById(IEnumerable<KeyValuePair<int, long>> statidAndCounts, Dictionary<int, Stat> statById)
        {
            foreach (var statIdAndCount in statidAndCounts)
            {
                if (statById.TryGetValue(statIdAndCount.Key, out var stat))
                    stat.Count += statIdAndCount.Value;
                else
                    statById[statIdAndCount.Key] = new Stat
                    {
                        ApiId = statIdAndCount.Key,
                        Count = statIdAndCount.Value,
                    };
            }
        }
    }
}
