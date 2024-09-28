using Blish_HUD.Controls;

namespace FarmingTracker
{
    public class OpenSettingsButton : StandardButton
    {
        public OpenSettingsButton(string buttonText, WindowTabSelector windowTabSelector, Container parent)
        {
            _windowTabSelector = windowTabSelector;

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
            _windowTabSelector.SelectWindowTab(WindowTab.Settings, WindowVisibility.Show);
        }

        private readonly WindowTabSelector _windowTabSelector;
    }
}