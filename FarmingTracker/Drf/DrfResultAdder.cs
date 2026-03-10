using System.Collections.Generic;
using System.Linq;

namespace FarmingTracker
{
    public class DrfResultAdder
    {
        // WARNING:
        // do not remove stats when their count becomes 0. this may trigger bugs with gw2sharp when it tries to partially read from cache and partially from api
        // but wont find the ids in the api.
        public static void UpdateCountsOrAddNewStats(List<DrfMessage> drfMessages, Stats stats)
        {
            var itemIdAndCounts = drfMessages.SelectMany(d => d.Payload.Drop.Items);
            InternalUpdateCountsOrAddNewStats(itemIdAndCounts, StatType.Item, stats);

            var currencyIdAndCounts = drfMessages.SelectMany(d => d.Payload.Drop.Currencies);
            InternalUpdateCountsOrAddNewStats(currencyIdAndCounts, StatType.Currency, stats);
        }

        private static void InternalUpdateCountsOrAddNewStats(IEnumerable<KeyValuePair<int, long>> statIdAndCounts, StatType statType, Stats stats)
        {
            foreach (var statIdAndCount in statIdAndCounts)
            {
                var stat = new Stat
                {
                    ApiId = statIdAndCount.Key,
                    StatType = statType,
                    Signed_Count =
                    {
                        Value = statIdAndCount.Value,
                    }
                };

                stats.UpdateCountOrAddNewStat(stat);
            }
        }
    }
}
