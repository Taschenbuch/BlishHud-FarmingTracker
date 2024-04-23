using Blish_HUD.Controls;
using System;
using System.Diagnostics;
using static Blish_HUD.ContentService;

namespace FarmingTracker
{
    public class ElapsedFarmingTimeLabel : Label
    {
        public ElapsedFarmingTimeLabel(Services services, Container parent)
        {
            Text = CreateTimeText("-:--:--");
            Font = services.FontService.Fonts[FontSize.Size18];
            AutoSizeHeight = true;
            AutoSizeWidth = true;
            Parent = parent;

            _farmingTimeStopwatch.Restart();
        }

        public void UpdateTimeOncePerSecond()
        {
            var farmingTime = _farmingTimeStopwatch.Elapsed;
            var oneSecondHasPassed = farmingTime >= _oldFarmingTime + TimeSpan.FromSeconds(1);
            if (oneSecondHasPassed)
            {
                // todo next update time vs elappsed farming time has 1 second difference
                _oldFarmingTime = farmingTime;
                UpdateLabelText(farmingTime);
            }
        }

        public void ResetTime()
        {
            _farmingTimeStopwatch.Restart();
            _oldFarmingTime = TimeSpan.Zero;
            UpdateLabelText(TimeSpan.Zero);
        }

        private void UpdateLabelText(TimeSpan farmingTime)
        {
            Text = CreateTimeText($"{farmingTime:h':'mm':'ss}");
        }

        private static string CreateTimeText(string timeString)
        {
            return $"farming for {timeString}";
        }

        private TimeSpan _oldFarmingTime;
        private readonly Stopwatch _farmingTimeStopwatch = new Stopwatch();
    }
}
