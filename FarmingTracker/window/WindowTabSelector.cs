namespace FarmingTracker
{
    // a wrapper to be able to log when farmingTrackerWindow is accidently null. Missing this check caused a hidden bug in the past.
    public class WindowTabSelector
    {
        public void Init(FarmingTrackerWindow farmingTrackerWindow)
        {
            _farmingTrackerWindow = farmingTrackerWindow;
        }

        public void SelectWindowTab(WindowTab windowTab, WindowVisibility windowVisibility)
        {
            if (_farmingTrackerWindow == null)
            {
                Module.Logger.Error("Cannot select tab because window field is not set yet.");
                return;
            }

            _farmingTrackerWindow.SelectWindowTab(windowTab, windowVisibility);
        }

        private FarmingTrackerWindow? _farmingTrackerWindow;
    }
}
