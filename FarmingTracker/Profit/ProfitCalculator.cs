using System.Linq;
using System;
using System.Collections.Generic;

namespace FarmingTracker
{
    public class ProfitCalculator
    {
        public long ProfitInCopper { get; private set; }
        public long ProfitPerHourInCopper { get; private set; }

        public void CalculateProfitPerHour(TimeSpan elapsedFarmingTime)
        {
            ProfitPerHourInCopper = CalculateProfitPerHourInCopper(ProfitInCopper, elapsedFarmingTime);
        }

        public void CalculateProfits(StatsSnapshot snapshot, SafeList<CustomStatProfit> customStatProfits, SafeList<int> ignoredItemApiIds, TimeSpan elapsedFarmingTime)
        {
            var profitInCopper = CalculateProfitInCopper(snapshot, customStatProfits, ignoredItemApiIds);
            ProfitPerHourInCopper = CalculateProfitPerHourInCopper(profitInCopper, elapsedFarmingTime);
            ProfitInCopper = profitInCopper;
        }

        public static long CalculateProfitInCopper(StatsSnapshot snapshot, SafeList<CustomStatProfit> customStatProfits, SafeList<int> ignoredItemApiIds)
        {
            var customStatProfitsCopy = customStatProfits.ToListSafe();
            var ignoredItemApiIdsCopy = ignoredItemApiIds.ToListSafe();

            var itemsSellProfitInCopper = snapshot.ItemById.Values
                .Where(i => !ignoredItemApiIdsCopy.Contains(i.ApiId))
                .Sum(i => GetStatProfit(customStatProfitsCopy, i));

            var currenciesSellProfitInCopper = snapshot.CurrencyById.Values
                .Where(c => !c.IsCoinOrCustomCoin)
                .Sum(c => GetStatProfit(customStatProfitsCopy, c));

            var coinsInCopper = snapshot.CurrencyById.Values.SingleOrDefault(s => s.IsCoin)?.Count ?? 0;
            var totalProfit = coinsInCopper + itemsSellProfitInCopper + currenciesSellProfitInCopper;

            if (DebugMode.DebugLoggingRequired)
                Module.Logger.Debug(
                    $"totalProfit {totalProfit} = " +
                    $"coinsInCopper {coinsInCopper} " +
                    $"+ itemsSellProfitInCopper {itemsSellProfitInCopper} " +
                    $"+ currenciesSellProfitInCopper {currenciesSellProfitInCopper} " +
                    $"| maxAllProfits per Item (including ignored) {string.Join(" ", snapshot.ItemById.Values.Select(i => GetStatProfit(customStatProfitsCopy, i)))}" +
                    $"| maxAllProfits per Currency {string.Join(" ", snapshot.CurrencyById.Values.Select(c => GetStatProfit(customStatProfitsCopy, c)))}");

            return totalProfit;
        }

        public static long CalculateProfitPerHourInCopper(long totalProfitInCopper, TimeSpan elapsedFarmingTime)
        {
            if (totalProfitInCopper == 0)
                return 0;

            var sessionJustStarted = elapsedFarmingTime.TotalSeconds < 1;
            if (sessionJustStarted) // otherwise value per hour would be inflated
                return 0;

            var profitPerHourInCopper = totalProfitInCopper / elapsedFarmingTime.TotalHours;

            if (profitPerHourInCopper > long.MaxValue)
                return long.MaxValue;

            if (profitPerHourInCopper <= long.MinValue)
                return long.MinValue + 1; // hack: +1 to prevent that Math.Abs() crashes, because (-1 * long.MinValue) is bigger than long.MaxValue.

            return (long)profitPerHourInCopper;
        }

        private static long GetStatProfit(List<CustomStatProfit> customStatProfits, Stat s)
        {
            var customStatProfit = customStatProfits.SingleOrDefault(c => c.BelongsToStat(s));
            return customStatProfit == null
                ? s.CountSign * s.Profits.All.MaxProfitInCopper
                : s.CountSign * s.Count * customStatProfit.CustomProfitInCopper;
        }
    }
}
