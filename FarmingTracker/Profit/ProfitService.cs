using System;
using System.Diagnostics;
using System.Linq;

namespace FarmingTracker
{
    public class ProfitService
    {
        public ProfitService(ProfitPanel totalProfitPanel, ProfitPanel profitPerHourPanel)
        {
            _totalProfitPanel = totalProfitPanel;
            _profitPerHourPanel = profitPerHourPanel;
            _stopwatch.Restart();
            SetTotalAndPerHourProfit(0, 0);
        }

        public void UpdateProfit(Model model, TimeSpan elapsedFarmingTime)
        {
            var totalProfitInCopper = CalculateTotalProfitInCopper(model);
            var profitPerHourInCopper = CalculateProfitPerHourInCopper(totalProfitInCopper, elapsedFarmingTime);
            SetTotalAndPerHourProfit(totalProfitInCopper, profitPerHourInCopper);
        }

        private void SetTotalAndPerHourProfit(long totalProfitInCopper, long profitPerHourInCopper)
        {
            _totalProfitPanel.SetProfit(totalProfitInCopper);
            _profitPerHourPanel.SetProfit(profitPerHourInCopper);
            _totalProfitInCopper = totalProfitInCopper;
        }

        private static long CalculateTotalProfitInCopper(Model model)
        {
            var coinsInCopper = model.CurrencyById.Values.SingleOrDefault(s => s.IsCoin)?.Count ?? 0;
            
            var itemsSellProfitInCopper = model.ItemById.Values
                .Where(s => !model.IgnoredItemApiIds.ContainsSafe(s.ApiId))
                .Sum(s => s.CountSign * s.Profits.All.MaxProfitInCopper);

            var totalProfit = coinsInCopper + itemsSellProfitInCopper;

            Module.Logger.Debug(
                $"totalProfit {totalProfit} = coinsInCopper {coinsInCopper} + itemsSellProfitInCopper {itemsSellProfitInCopper} | " +
                $"maxProfitsPerItem {string.Join(" ", model.ItemById.Values.Select(s => s.CountSign * s.Profits.All.MaxProfitInCopper))}");

            return totalProfit;
        }

        private static long CalculateProfitPerHourInCopper(long totalProfitInCopper, TimeSpan elapsedFarmingTime)
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


        public void UpdateProfitPerHourEveryFiveSeconds(TimeSpan elapsedFarmingTime)
        {
            var time = _stopwatch.Elapsed;
            var fiveSecondsHavePassed = time >= _oldTime + TimeSpan.FromSeconds(Constants.PROFIT_PER_HOUR_UPDATE_INTERVAL_IN_SECONDS);
            if (fiveSecondsHavePassed)
            {
                var profitPerHourInCopper = CalculateProfitPerHourInCopper(_totalProfitInCopper, elapsedFarmingTime);
                _profitPerHourPanel.SetProfit(profitPerHourInCopper);
                _oldTime = time;
            }
        }

        private readonly ProfitPanel _totalProfitPanel;
        private readonly ProfitPanel _profitPerHourPanel;
        private readonly Stopwatch _stopwatch = new Stopwatch();  // do not use elapsedFarmingTime, because it can be resetted and maybe other stuff in the future.
        private TimeSpan _oldTime = TimeSpan.Zero;
        private long _totalProfitInCopper;
    }
}
