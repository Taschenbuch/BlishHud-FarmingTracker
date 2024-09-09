using System.Linq;
using System;

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

        public void CalculateProfits(StatsSnapshot snapshot, SafeList<int> ignoredItemApiIds, TimeSpan elapsedFarmingTime)
        {
            var profitInCopper = CalculateTotalProfitInCopper(snapshot, ignoredItemApiIds);
            ProfitPerHourInCopper = CalculateProfitPerHourInCopper(profitInCopper, elapsedFarmingTime);
            ProfitInCopper = profitInCopper;
        }

        public static long CalculateTotalProfitInCopper(StatsSnapshot snapshot, SafeList<int> ignoredItemApiIds)
        {
            var coinsInCopper = snapshot.CurrencyById.Values.SingleOrDefault(s => s.IsCoin)?.Count ?? 0;

            var ignoredItemApiIdsCopy = ignoredItemApiIds.ToListSafe();
            var itemsSellProfitInCopper = snapshot.ItemById.Values
                .Where(s => !ignoredItemApiIdsCopy.Contains(s.ApiId))
                .Sum(s => s.CountSign * s.Profits.All.MaxProfitInCopper);

            var totalProfit = coinsInCopper + itemsSellProfitInCopper;

            if (Module.DebugEnabled)
                Module.Logger.Debug(
                    $"totalProfit {totalProfit} = coinsInCopper {coinsInCopper} + itemsSellProfitInCopper {itemsSellProfitInCopper} | " +
                    $"maxProfitsPerItem {string.Join(" ", snapshot.ItemById.Values.Select(s => s.CountSign * s.Profits.All.MaxProfitInCopper))}");

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
    }
}
