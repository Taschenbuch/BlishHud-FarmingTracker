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
            ResetProfit();
        }

        public void ResetProfit()
        {
            SetTotalAndPerHourProfit(0, 0);
        }

        public void UpdateProfit(Stats stats, TimeSpan elapsedFarmingTime)
        {
            var totalProfitInCopper = CalculateTotalProfitInCopper(stats);
            var profitPerHourInCopper = CalculateProfitPerHourInCopper(totalProfitInCopper, elapsedFarmingTime);
            SetTotalAndPerHourProfit(totalProfitInCopper, profitPerHourInCopper);
        }

        private void SetTotalAndPerHourProfit(int totalProfitInCopper, int profitPerHourInCopper)
        {
            _totalProfitPanel.SetProfit(totalProfitInCopper);
            _profitPerHourPanel.SetProfit(profitPerHourInCopper);
            _totalProfitInCopper = totalProfitInCopper;
        }

        private static int CalculateTotalProfitInCopper(Stats stats)
        {
            var coinsInCopper = stats.CurrencyById.Values.SingleOrDefault(s => s.IsCoin)?.Count ?? 0;
            var itemSellProfitsInCopper = stats.ItemById.Values.Sum(s => s.Profit.MaxProfitInCopper * s.Count);
            var totalProfit = coinsInCopper + itemSellProfitsInCopper;

            Module.Logger.Debug(
                $"totalProfit {totalProfit} | " +
                $"coinsInCopper {coinsInCopper} | " +
                $"itemSellProfitsInCopper {itemSellProfitsInCopper} | " +
                $"maxProfits {string.Join(" ", stats.ItemById.Values.Select(s => s.Profit.MaxProfitInCopper * s.Count))}");

            return totalProfit;
        }

        private static int CalculateProfitPerHourInCopper(int totalProfitInCopper, TimeSpan elapsedFarmingTime)
        {
            if (totalProfitInCopper == 0)
                return 0;

            var sessionJustStarted = elapsedFarmingTime.TotalSeconds < 1;
            if (sessionJustStarted) // otherwise value per hour would be inflated
                return 0;

            var profitPerHourInCopper = totalProfitInCopper / elapsedFarmingTime.TotalHours;

            if (profitPerHourInCopper > int.MaxValue)
                return int.MaxValue;

            if (profitPerHourInCopper <= int.MinValue)
                return int.MinValue + 1; // hack: +1 to prevent that Math.Abs() crashes, because (-1 * int.MinValue) is bigger than int.MaxValue.

            return (int)profitPerHourInCopper;
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
        private int _totalProfitInCopper;
    }
}
