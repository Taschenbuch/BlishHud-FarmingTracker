namespace FarmingTracker
{
    // MS = milliseconds
    public class UpdateLoop
    {
        public void AddToRunningTime(double milliseconds)
        {
            _runningTimeMs += milliseconds;
        }

        public void ResetRunningTime()
        {
            _runningTimeMs = 0;
        }

        public bool UpdateIntervalEnded()
        {
            return _runningTimeMs >= _updateIntervalMs;
        }

        public void UseFarmingUpdateInterval()
        {
            _updateIntervalMs = FARMING_UPDATE_INTERVAL_MS;
        }

        public void UseRetryAfterApiFailureUpdateInterval()
        {
            _updateIntervalMs = RETRY_AFTER_API_FAILURE_UPDATE_INTERVAL_MS;
        }

        public void TriggerUpdateStatPane()
        {
            lock(_updateLock)
                _statPanelsHaveToBeUpdated = true;
        }

        public bool GetAndResetStatPanelsHaveToBeUpdated()
        {
            lock (_updateLock)
            {
                var statPanelsHaveToBeUpdated = _statPanelsHaveToBeUpdated;
                _statPanelsHaveToBeUpdated = false;
                return statPanelsHaveToBeUpdated;
            }
        }

        public const int RETRY_AFTER_API_FAILURE_UPDATE_INTERVAL_MS = 5_000;
        public const int WAIT_FOR_API_TOKEN_UPDATE_INTERVALL_MS = 2_000;
        private const int FARMING_UPDATE_INTERVAL_MS = 1_000; // todo rename
        private double _runningTimeMs;
        private double _updateIntervalMs; // default 0 to start immediately in first Module.Update() call
        private bool _statPanelsHaveToBeUpdated;
        private static readonly object _updateLock = new object();
    }
}
