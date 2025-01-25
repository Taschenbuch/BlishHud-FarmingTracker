using System.Linq;
using System;

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

        public void CalculateProfits(Model model, TimeSpan elapsedFarmingTime)
        {
            var signed_profitInCopper = CalculateSignedProfitInCopper(model);
            Signed_ProfitPerHourInCopper = CalculateSignedProfitPerHourInCopper(signed_profitInCopper, elapsedFarmingTime);
            Signed_ProfitInCopper = signed_profitInCopper;
        }

        private static long CalculateSignedProfitInCopper(Model model)
        {
            var stats = model.Stats.StatById.Values; // todo x lock?

            var multiple_signed_statsSellProfitsInCopper = stats // todo x lock?
                .Where(s => !s.IsCoinOrCustomCoin)
                .Where(s => s.StatVisibility != StatVisibility.Ignored)
                .Select(s => s.Signed_Count * s.Profit.Unsigned_MaxProfitInCopper);

            var total_signed_statsSellProfitInCopper = multiple_signed_statsSellProfitsInCopper.Sum(); // todo x lock?
            var signed_coinsInCopper = stats.SingleOrDefault(s => s.IsCoin)?.Signed_Count ?? 0;
            var signed_totalProfit = signed_coinsInCopper + total_signed_statsSellProfitInCopper;

            if (DebugMode.DebugLoggingRequired)
                Module.Logger.Debug(
                    $"totalProfit {signed_totalProfit} = " +
                    $"coinsInCopper {signed_coinsInCopper} " +
                    $"+ statsSellProfitInCopper {total_signed_statsSellProfitInCopper} " +
                    $"| maxAllProfits per Stat {string.Join(" ", multiple_signed_statsSellProfitsInCopper)}");

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
    }
}
