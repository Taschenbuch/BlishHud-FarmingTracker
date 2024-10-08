﻿using System.Collections.Generic;
using System.Linq;

namespace FarmingTracker
{
    public class DrfResultAdder
    {
        public static void UpdateCountsOrAddNewStats(
            List<DrfMessage> drfMessages, 
            Dictionary<int, Stat> itemById, 
            Dictionary<int, Stat> currencyById)
        {
            var itemIdAndCounts = drfMessages.SelectMany(d => d.Payload.Drop.Items);
            InternalUpdateCountsOrAddNewStats(itemIdAndCounts, StatType.Item, itemById);

            var currencyIdAndCounts = drfMessages.SelectMany(d => d.Payload.Drop.Currencies);
            InternalUpdateCountsOrAddNewStats(currencyIdAndCounts, StatType.Currency, currencyById);
        }

        private static void InternalUpdateCountsOrAddNewStats(IEnumerable<KeyValuePair<int, long>> statIdAndCounts, StatType statType, Dictionary<int, Stat> statById)
        {
            foreach (var statIdAndCount in statIdAndCounts)
            {
                if (statById.TryGetValue(statIdAndCount.Key, out var stat))
                    stat.Count += statIdAndCount.Value;
                else
                    statById[statIdAndCount.Key] = new Stat
                    {
                        ApiId = statIdAndCount.Key,
                        StatType = statType,
                        Count = statIdAndCount.Value,
                    };
            }
        }
    }
}
