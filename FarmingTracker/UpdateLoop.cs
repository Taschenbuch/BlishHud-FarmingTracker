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

        public void TriggerUpdateStats()
        {
            _statsHaveToBeUpdated = true;
            _runningTimeMs = _updateIntervalMs; // trigger instant update
        }

        public bool HasToUpdateStats()
        {
            if (!_statsHaveToBeUpdated)
                return false;

            _statsHaveToBeUpdated = false;
            return true;
        }

        public void TriggerUpdateUi()
        {
            _uiHasToBeUpdated = true;
        }

        public bool HasToUpdateUi()
        {
            if (!_uiHasToBeUpdated)
                return false;

            _uiHasToBeUpdated = false;
            return true;
        }

        public void TriggerSaveModel()
        {
            _modelHasToBeSaved = true;
        }

        public bool HasToSaveModel()
        {
            var modelHasToBeSaved = _modelHasToBeSaved;
            if(modelHasToBeSaved)
                _modelHasToBeSaved = false;
            
            return modelHasToBeSaved;
        }

        public const int RETRY_AFTER_API_FAILURE_UPDATE_INTERVAL_MS = 5_000;
        public const int WAIT_FOR_API_TOKEN_UPDATE_INTERVALL_MS = 2_000;
        private const int FARMING_UPDATE_INTERVAL_MS = 1_000; // todo rename
        private double _runningTimeMs;
        private double _updateIntervalMs; // default 0 to start immediately in first Module.Update() call
        private bool _uiHasToBeUpdated;
        private bool _statsHaveToBeUpdated;
        private bool _modelHasToBeSaved;
    }
}
