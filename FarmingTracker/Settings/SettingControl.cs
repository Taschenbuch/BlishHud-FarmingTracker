using Blish_HUD.Controls;
using Blish_HUD.Settings.UI.Views;
using Blish_HUD.Settings;

namespace FarmingTracker
{
    public class SettingControl: ViewContainer
    {
        public SettingControl(Container parent, SettingEntry settingEntry)
        {
            Parent = parent;
            Show(SettingView.FromType(settingEntry, parent.Width));
        }
    }
}
