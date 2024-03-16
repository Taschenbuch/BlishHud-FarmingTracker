using System;

namespace FarmingTracker
{
    // MS = milliseconds
    public class UpdateLoop
    {
        public double RunningTimeMs { get; set; }
        public double UpdateIntervalMs { get; set; } // default 0 to start immediately in first Module.Update() call

        public void AddToRunningTime(double milliseconds)
        {
            RunningTimeMs += milliseconds;
        }

        public void ResetRunningTime()
        {
            RunningTimeMs = 0;
        }

        public bool UpdateIntervalEnded()
        {
            return RunningTimeMs >= UpdateIntervalMs;
        }

        public void UseWaitForApiTokenUpdateInterval()
        {
            UpdateIntervalMs = WAIT_FOR_API_TOKEN_UPDATE_INTERVALL_MS;
        }

        public void UseFarmingUpdateInterval()
        {
            UpdateIntervalMs = FARMING_UPDATE_INTERVAL_MS;
        }

        public void TiggerUpdateInstantly()
        {
            UpdateIntervalMs = 0; // trigger items update immediately
        }

        public void UseRetryAfterApiFailureUpdateInterval()
        {
            UpdateIntervalMs = RETRY_AFTER_API_FAILURE_UPDATE_INTERVAL_MS;
        }

        public static readonly TimeSpan FARMING_UPDATE_INTERVAL_TIME = TimeSpan.FromMilliseconds(FARMING_UPDATE_INTERVAL_MS);
        public const int RETRY_AFTER_API_FAILURE_UPDATE_INTERVAL_MS = 30 * 1000;
        public const int WAIT_FOR_API_TOKEN_UPDATE_INTERVALL_MS = 2 * 1000;
        private const int FARMING_UPDATE_INTERVAL_MS = 3 * 60 * 1000; // todo sinnvolle zeit angeben
    }
}
