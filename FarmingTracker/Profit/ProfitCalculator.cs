using System.Linq;
using System;
using System.Collections.Generic;

namespace FarmingTracker
{
    public class ProfitCalculator
    {
        public long Signed_ProfitInCopper { get; private set; }
        public long Signed_ProfitPerHourInCopper { get; private set; }

        public void CalculateProfitPerHour(TimeSpan elapsedFarmingTime)
        {
            Signed_ProfitPerHourInCopper = CalculateSignedProfitPerHourInCopper(Signed_ProfitInCopper, elapsedFarmingTime);
        }

        public void CalculateProfits(StatsSnapshot snapshot, SafeList<CustomStatProfit> customStatProfits, SafeList<int> ignoredItemApiIds, TimeSpan elapsedFarmingTime)
        {
            var signed_profitInCopper = CalculateSignedProfitInCopper(snapshot, customStatProfits, ignoredItemApiIds);
            Signed_ProfitPerHourInCopper = CalculateSignedProfitPerHourInCopper(signed_profitInCopper, elapsedFarmingTime);
            Signed_ProfitInCopper = signed_profitInCopper;
        }

        private static long CalculateSignedProfitInCopper(StatsSnapshot snapshot, SafeList<CustomStatProfit> customStatProfits, SafeList<int> ignoredItemApiIds)
        {
            var customStatProfitsCopy = customStatProfits.ToListSafe();
            var ignoredItemApiIdsCopy = ignoredItemApiIds.ToListSafe();

            var signed_itemsSellProfitInCopper = snapshot.ItemById.Values
                .Where(i => !ignoredItemApiIdsCopy.Contains(i.ApiId))
                .Sum(i => GetSignedStatProfit(customStatProfitsCopy, i));

            var signed_currenciesSellProfitInCopper = snapshot.CurrencyById.Values
                .Where(c => !c.IsCoinOrCustomCoin)
                .Sum(c => GetSignedStatProfit(customStatProfitsCopy, c));

            var signed_coinsInCopper = snapshot.CurrencyById.Values.SingleOrDefault(s => s.IsCoin)?.Signed_Count ?? 0;
            var signed_totalProfit = signed_coinsInCopper + signed_itemsSellProfitInCopper + signed_currenciesSellProfitInCopper;

            if (DebugMode.DebugLoggingRequired)
                Module.Logger.Debug(
                    $"totalProfit {signed_totalProfit} = " +
                    $"coinsInCopper {signed_coinsInCopper} " +
                    $"+ itemsSellProfitInCopper {signed_itemsSellProfitInCopper} " +
                    $"+ currenciesSellProfitInCopper {signed_currenciesSellProfitInCopper} " +
                    $"| maxAllProfits per Item (including ignored) {string.Join(" ", snapshot.ItemById.Values.Select(i => GetSignedStatProfit(customStatProfitsCopy, i)))}" +
                    $"| maxAllProfits per Currency {string.Join(" ", snapshot.CurrencyById.Values.Select(c => GetSignedStatProfit(customStatProfitsCopy, c)))}");

            return signed_totalProfit;
        }

        private static long CalculateSignedProfitPerHourInCopper(long signed_totalProfitInCopper, TimeSpan elapsedFarmingTime)
        {
            if (signed_totalProfitInCopper == 0)
                return 0;

            var sessionJustStarted = elapsedFarmingTime.TotalSeconds < 1;
            if (sessionJustStarted) // otherwise value per hour would be inflated
                return 0;

            var signed_profitPerHourInCopper = signed_totalProfitInCopper / elapsedFarmingTime.TotalHours;

            if (signed_profitPerHourInCopper > long.MaxValue)
                return long.MaxValue;

            if (signed_profitPerHourInCopper <= long.MinValue)
                return long.MinValue + 1; // hack: +1 to prevent that Math.Abs() crashes, because (-1 * long.MinValue) is bigger than long.MaxValue.

            return (long)signed_profitPerHourInCopper;
        }

        private static long GetSignedStatProfit(List<CustomStatProfit> customStatProfits, Stat s)
        {
            var customStatProfit = customStatProfits.SingleOrDefault(c => c.BelongsToStat(s));
            return customStatProfit == null
                ? s.CountSign * s.Profits.All.Unsigned_MaxProfitInCopper
                : s.Signed_Count * customStatProfit.Unsigned_CustomProfitInCopper;
        }
    }
}
