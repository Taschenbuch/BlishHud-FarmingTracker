using System.Collections.Generic;
using System.Linq;

namespace FarmingTracker
{
    public class DrfResultAdder
    {
        public static void UpdateItemById(List<DrfMessage> drfMessages, Dictionary<int, Stat> oldItemById)
        {
            var itemIdAndCounts = drfMessages.SelectMany(d => d.Payload.Drop.Items);
            UpdateStatById(itemIdAndCounts, oldItemById);
        }

        public static void UpdateCurrencyById(List<DrfMessage> drfMessages, Dictionary<int, Stat> oldCurrencyById)
        {
            var currencyIdAndCounts = drfMessages.SelectMany(d => d.Payload.Drop.Currencies);
            UpdateStatById(currencyIdAndCounts, oldCurrencyById);
        }

        private static void UpdateStatById(IEnumerable<KeyValuePair<int, int>> itemIdAndCounts, Dictionary<int, Stat> oldItemById)
        {
            foreach (var itemIdAndCount in itemIdAndCounts)
            {
                if (oldItemById.TryGetValue(itemIdAndCount.Key, out var oldItem))
                    oldItem.Count += itemIdAndCount.Value;
                else
                    oldItemById[itemIdAndCount.Key] = new Stat
                    {
                        ApiId = itemIdAndCount.Key,
                        Count = itemIdAndCount.Value,
                    };
            }

            foreach (var itemWithZeroCount in oldItemById.Values.Where(o => o.Count == 0).ToList())
                oldItemById.Remove(itemWithZeroCount.ApiId);
        }
    }
}
