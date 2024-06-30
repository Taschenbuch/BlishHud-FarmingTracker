using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Settings;
using System;
using System.Diagnostics;

namespace FarmingTracker
{
    public class DateTimeService : IDisposable
    {
        // STATIC PART

        public static DateTime UtcNow
        {
            get
            {
                return _debugEnabled
                    ? _utcNowDebug + _stopWatch.Elapsed
                    : DateTime.UtcNow;
            }
            set
            {
                _utcNowDebug = value;
                _stopWatch.Restart();
            }
        }

        private static DateTime _utcNowDebug = DateTime.UtcNow;
        private static bool _debugEnabled;
        private static readonly Stopwatch _stopWatch = new Stopwatch(); // no disposing necessary

        // NONE STATIC PART (but updating static values)
        
        // ctor must be called in Module.DefineSettings()
        public DateTimeService(SettingCollection settings)
        {
            _stopWatch.Restart();
            DefineSettings(settings);
            _utcNowDebug = _debugDateTimeValueSetting.Value; // because it is static. It would userwise survive module unload/load with old value.
        }

        public void CreateDateTimeDebugPanel(Container parent)
        {
            DateTimeDebugPanelService.CreateDateTimeDebugPanel(parent, _debugDateTimeEnabledSetting, _debugDateTimeValueSetting);
        }

        private void DefineSettings(SettingCollection settings)
        {
            _debugDateTimeEnabledSetting = settings.DefineSetting(
                "debug dateTime enabled",
                false,
                () => "use debug dateTime",
                () => "Use debug dateTime instead of system time.");

            _debugDateTimeValueSetting = settings.DefineSetting(
                "debug dateTime value",
                DateTime.UtcNow,
                () => "use debug dateTime",
                () => "Use debug dateTime instead of system time.");

            _debugDateTimeEnabledSetting.SettingChanged += OnDebugDateTimeEnabledSettingChanged;
            OnDebugDateTimeEnabledSettingChanged();
        }

        public void Dispose()
        {
            _debugDateTimeEnabledSetting.SettingChanged -= OnDebugDateTimeEnabledSettingChanged;
        }

        private void OnDebugDateTimeEnabledSettingChanged(object sender = null, ValueChangedEventArgs<bool> e = null)
        {
            _debugEnabled = _debugDateTimeEnabledSetting.Value;
        }

        private SettingEntry<bool> _debugDateTimeEnabledSetting;
        private SettingEntry<DateTime> _debugDateTimeValueSetting;
    }
}
