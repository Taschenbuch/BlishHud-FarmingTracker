using Blish_HUD.Controls;
using Blish_HUD.Settings.UI.Views;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework;

namespace FarmingTracker
{
    public class SettingControls
    {
        public static ViewContainer CreateSetting(Container parent, SettingEntry settingEntry)
        {
            var viewContainer = new ViewContainer { Parent = parent };
            viewContainer.Show(SettingView.FromType(settingEntry, parent.Width));
            return viewContainer;
        }

        public static FlowPanel CreateSettingsFlowPanel(Container parent, string title)
        {
            return new FlowPanel
            {
                Title = title,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                BackgroundColor = Color.Black * 0.5f,
                OuterControlPadding = new Vector2(5, 5),
                ControlPadding = new Vector2(0, 10),
                ShowBorder = true,
                Width = Constants.PANEL_WIDTH,
                HeightSizingMode = SizingMode.AutoSize,
                Parent = parent,
            };
        }
    }
}
