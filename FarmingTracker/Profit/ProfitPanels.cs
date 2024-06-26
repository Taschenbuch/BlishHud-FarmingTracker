﻿using Blish_HUD.Controls;
using System;
using System.Diagnostics;
using System.Linq;
using static Blish_HUD.ContentService;

namespace FarmingTracker
{
    public class ProfitPanels
    {
        public ProfitPanels(TextureService textureService, FontService fontService, Container parent)
        {
            var profitTooltip = "Rough profit when selling everything to vendor and on trading post. Click help button for more info.";
            var font = fontService.Fonts[FontSize.Size16];
            _totalProfitPanel = new ProfitPanel("Profit", profitTooltip, font, textureService, parent);
            _profitPerHourPanel = new ProfitPanel("Profit per hour", profitTooltip, font, textureService, parent);
            _stopwatch.Restart();
            SetTotalAndPerHourProfit(0, 0);
        }

        public void UpdateProfitLabels(StatsSnapshot snapshot, SafeList<int> ignoredItemApiIds, TimeSpan elapsedFarmingTime)
        {
            var totalProfitInCopper = CalculateTotalProfitInCopper(snapshot, ignoredItemApiIds);
            var profitPerHourInCopper = CalculateProfitPerHourInCopper(totalProfitInCopper, elapsedFarmingTime);
            SetTotalAndPerHourProfit(totalProfitInCopper, profitPerHourInCopper);
        }

        private void SetTotalAndPerHourProfit(long totalProfitInCopper, long profitPerHourInCopper)
        {
            _totalProfitPanel.SetProfit(totalProfitInCopper);
            _profitPerHourPanel.SetProfit(profitPerHourInCopper);
            _totalProfitInCopper = totalProfitInCopper;
        }

        private static long CalculateTotalProfitInCopper(StatsSnapshot snapshot, SafeList<int> ignoredItemApiIds)
        {
            var coinsInCopper = snapshot.CurrencyById.Values.SingleOrDefault(s => s.IsCoin)?.Count ?? 0;
            
            var ignoredItemApiIdsCopy = ignoredItemApiIds.ToListSafe();
            var itemsSellProfitInCopper = snapshot.ItemById.Values
                .Where(s => !ignoredItemApiIdsCopy.Contains(s.ApiId))
                .Sum(s => s.CountSign * s.Profits.All.MaxProfitInCopper);

            var totalProfit = coinsInCopper + itemsSellProfitInCopper;

            if(Module.DebugEnabled)
                Module.Logger.Debug(
                    $"totalProfit {totalProfit} = coinsInCopper {coinsInCopper} + itemsSellProfitInCopper {itemsSellProfitInCopper} | " +
                    $"maxProfitsPerItem {string.Join(" ", snapshot.ItemById.Values.Select(s => s.CountSign * s.Profits.All.MaxProfitInCopper))}");

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
