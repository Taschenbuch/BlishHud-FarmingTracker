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
            Text = CreateFarmingTimeText("-:--:--");
            Font = services.FontService.Fonts[FontSize.Size14];
            AutoSizeHeight = true;
            AutoSizeWidth = true;
            Parent = parent;

            _farmingTimeStopwatch.Restart();
        }

        public TimeSpan ElapsedTime => _farmingTimeStopwatch.Elapsed;

        public void UpdateTimeEverySecond()
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

        public void RestartTime()
        {
            _farmingTimeStopwatch.Restart();
            _oldFarmingTime = TimeSpan.Zero;
            UpdateLabelText(TimeSpan.Zero);
        }

        private void UpdateLabelText(TimeSpan farmingTime)
        {
            var formattedFarmingTime = CreateFormattedTimeText(farmingTime);
            Text = CreateFarmingTimeText(formattedFarmingTime);
        }

        private static string CreateFormattedTimeText(TimeSpan timeSpan)
        {
            if (timeSpan >= TimeSpan.FromHours(1))
                return $"{timeSpan:h' hr  'm' min  'ss' sec'}";

            if (timeSpan >= TimeSpan.FromMinutes(1))
                return $"{timeSpan:m' min  'ss' sec'}";

            return $"{timeSpan:ss' sec'}";
        }

        private static string CreateFarmingTimeText(string timeString)
        {
            return $"farming for {timeString}";
        }

        private TimeSpan _oldFarmingTime = TimeSpan.Zero;
        private readonly Stopwatch _farmingTimeStopwatch = new Stopwatch();
    }
}
