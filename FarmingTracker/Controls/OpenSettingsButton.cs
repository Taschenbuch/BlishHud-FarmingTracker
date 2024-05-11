using Blish_HUD.Controls;

namespace FarmingTracker
{
    public class OpenSettingsButton : StandardButton
    {
        public OpenSettingsButton(FarmingTrackerWindowService farmingTrackerWindowService, Container parent)
        {
            _farmingTrackerWindowService = farmingTrackerWindowService;

            Text = "Open Settings";
            BasicTooltipText = "Open farming tracker settings";
            Width            = 150;
            Parent           = parent;

            Click += OnSettingsButtonClick;
        }

        protected override void DisposeControl()
        {
            Click -= OnSettingsButtonClick;
            base.DisposeControl();
        }

        private void OnSettingsButtonClick(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            _farmingTrackerWindowService.ShowWindowAndSelectSettingsTab();
        }

        private readonly FarmingTrackerWindowService _farmingTrackerWindowService;
    }
}