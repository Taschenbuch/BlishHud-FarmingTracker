using Blish_HUD.Controls;

namespace FarmingTracker
{
    public class OpenSettingsButton : StandardButton
    {
        public OpenSettingsButton(string buttonText, FarmingTrackerWindow farmingTrackerWindow, Container parent)
        {
            _farmingTrackerWindow = farmingTrackerWindow;

            Text = buttonText;
            BasicTooltipText = "Open farming tracker settings";
            Width = 300;
            Parent = parent;

            Click += OnSettingsButtonClick;
        }

        protected override void DisposeControl()
        {
            Click -= OnSettingsButtonClick;
            base.DisposeControl();
        }

        private void OnSettingsButtonClick(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            _farmingTrackerWindow.ShowWindowAndSelectSettingsTab();
        }

        private readonly FarmingTrackerWindow _farmingTrackerWindow;
    }
}