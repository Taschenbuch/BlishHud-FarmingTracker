using Blish_HUD;
using System;

namespace FarmingTracker
{
    public class AutomaticResetService : IDisposable
    {
        public AutomaticResetService(Services services)
        {
            _settingsService = services.SettingService;
            services.SettingService.AutomaticResetSetting.SettingChanged += OnAutomaticResetSettingChanged;
            InitializeNextResetDateTimeIfNecessary();
        }

        public void Dispose()
        {
            _settingsService.AutomaticResetSetting.SettingChanged -= OnAutomaticResetSettingChanged;
        }

        public bool HasToResetAutomatically()
        {
            var isModuleStart = _isModuleStart;
            _isModuleStart = false;

            var isPastResetDate = _settingsService.NextResetDateTimeUtcSetting.Value < DateTime.UtcNow;
            var hasToReset = _settingsService.AutomaticResetSetting.Value switch
            {
                AutomaticReset.Never => false,
                AutomaticReset.OnModuleStart => isModuleStart,
                AutomaticReset.MinutesAfterModuleShutdown => isModuleStart && isPastResetDate,
                _ => isPastResetDate,
            };

            if (hasToReset)
                Module.Logger.Info($"Automatic reset required because reset date has been exceeded. " +
                    $"isModuleStart: {isModuleStart} | " +
                    $"AutomaticResetSetting: {_settingsService.AutomaticResetSetting.Value} | " +
                    $"ResetDateTimeUtc {_settingsService.NextResetDateTimeUtcSetting.Value}");

            return hasToReset;
        }

        public void UpdateNextResetDateTimeForMinutesUntilResetAfterModuleShutdown()
        {
            if (_settingsService.AutomaticResetSetting.Value == AutomaticReset.MinutesAfterModuleShutdown)
                UpdateNextResetDateTime();
        }

        public void UpdateNextResetDateTime()
        {
            _settingsService.NextResetDateTimeUtcSetting.Value = NextAutomaticResetCalculator.GetNextResetDateTimeUtc(
                DateTime.UtcNow,
                _settingsService.AutomaticResetSetting.Value,
                _settingsService.MinutesUntilResetAfterModuleShutdownSetting.Value);
        }        

        private void OnAutomaticResetSettingChanged(object sender, ValueChangedEventArgs<AutomaticReset> e)
        {
            UpdateNextResetDateTime();
        }

        // init requried when lastResetTime hasnt existed in file before (module update) or file iss freshly created (new module installation or file was deleted)
        private void InitializeNextResetDateTimeIfNecessary()
        {
            var initializationRequired = _settingsService.NextResetDateTimeUtcSetting.Value == NextAutomaticResetCalculator.UNDEFINED_RESET_DATE_TIME;
            if (initializationRequired)
                UpdateNextResetDateTime();
        }

        private readonly SettingService _settingsService;
        private bool _isModuleStart = true;
    }
}
