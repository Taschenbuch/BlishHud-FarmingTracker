using System;

namespace FarmingTracker
{
    public class NextAutomaticResetCalculator
    {
        public static DateTime GetNextResetDateTimeUtc(DateTime dateTimeUtc, AutomaticReset automaticReset, int minutesUntilResetAfterModuleShutdown)
        {
            switch (automaticReset)
            {
                case AutomaticReset.OnModuleStart:
                case AutomaticReset.Never:
                    return NEVER_OR_ON_MODULE_START_RESET_DATE_TIME;
                case AutomaticReset.MinutesAfterModuleShutdown:
                    return dateTimeUtc + TimeSpan.FromMinutes(minutesUntilResetAfterModuleShutdown);
                case AutomaticReset.OnDailyReset:
                    return GetNextDailyResetDateTimeUtc(dateTimeUtc);
                case AutomaticReset.OnWeeklyReset:
                    return GetNextWeeklyResetDateTimeUtc(dateTimeUtc, DayOfWeek.Monday, 7, 30);
                case AutomaticReset.OnWeeklyNaWvwReset:
                    return GetNextWeeklyResetDateTimeUtc(dateTimeUtc, DayOfWeek.Saturday, 2);
                case AutomaticReset.OnWeeklyEuWvwReset:
                    return GetNextWeeklyResetDateTimeUtc(dateTimeUtc, DayOfWeek.Friday, 18);
                case AutomaticReset.OnWeeklyMapBonusRewardsReset:
                    return GetNextWeeklyResetDateTimeUtc(dateTimeUtc, DayOfWeek.Thursday, 20);
                default:
                    Module.Logger.Error(Helper.CreateSwitchCaseNotFoundMessage(automaticReset, nameof(AutomaticReset), "never reset"));
                    return NEVER_OR_ON_MODULE_START_RESET_DATE_TIME;
            }
        }

        private static DateTime GetNextWeeklyResetDateTimeUtc(DateTime dateTimeUtc, DayOfWeek resetDayOfWeekUtc, double resetHoursUtc, double resetMinutesUtc = 0)
        {
            var daysUntilWeeklyReset = (resetDayOfWeekUtc - dateTimeUtc.DayOfWeek + 7) % 7;
            var weeklyResetDateTimeUtc = dateTimeUtc.Date.AddDays(daysUntilWeeklyReset).AddHours(resetHoursUtc).AddMinutes(resetMinutesUtc);
            return dateTimeUtc < weeklyResetDateTimeUtc
                ? weeklyResetDateTimeUtc
                : weeklyResetDateTimeUtc.AddDays(7);
        }

        private static DateTime GetNextDailyResetDateTimeUtc(DateTime dateTimeUtc)
        {
            return dateTimeUtc.Date.AddDays(1); // daily reset is at 00:00 UTC
        }

        public static readonly DateTime UNDEFINED_RESET_DATE_TIME = new DateTime(0L, DateTimeKind.Utc); // UTC DateTime.Min
        private static readonly DateTime NEVER_OR_ON_MODULE_START_RESET_DATE_TIME = new DateTime(3155378975999999999L, DateTimeKind.Utc); // UTC DateTime.Max
    }
}
