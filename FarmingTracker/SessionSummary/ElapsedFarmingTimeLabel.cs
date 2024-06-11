using Blish_HUD.Controls;
using System;
using static Blish_HUD.ContentService;

namespace FarmingTracker
{
    public class ElapsedFarmingTimeLabel : Label
    {
        public ElapsedFarmingTimeLabel(Services services, Container parent)
        {
            _services = services;

            Text = CreateFarmingTimeText("-:--:--");
            Font = services.FontService.Fonts[FontSize.Size14];
            AutoSizeHeight = true;
            AutoSizeWidth = true;
            Parent = parent;
        }

        public void UpdateTimeEverySecond()
        {
            var farmingTime = _services.Model.FarmingDuration.Elapsed;
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
            _services.Model.FarmingDuration.Restart();
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
            return $"Farming for {timeString}";
        }

        private TimeSpan _oldFarmingTime = TimeSpan.Zero;
        private readonly Services _services;
    }
}
