using System;
using System.Diagnostics;

namespace FarmingTracker
{
    public class FarmingDuration
    {
        public FarmingDuration(SettingService settingService)
        {
            _settingService = settingService;
            Elapsed = settingService.FarmingDurationTimeSpanSetting.Value;
            _stopwatch.Restart();
        }

        public TimeSpan Elapsed 
        { 
            get => _value + _stopwatch.Elapsed; 
            set => _value = value; 
        }

        public void Restart()
        {
            _stopwatch.Restart();
            _value = TimeSpan.Zero;
            _settingService.FarmingDurationTimeSpanSetting.Value = TimeSpan.Zero;
        }

        public void SaveFarmingTime()
        {
            _settingService.FarmingDurationTimeSpanSetting.Value = Elapsed;
        }

        private readonly SettingService _settingService;
        private readonly Stopwatch _stopwatch = new Stopwatch();
        private TimeSpan _value;
    }
}
