using Blish_HUD.Controls;
using Blish_HUD.Settings.UI.Views;
using Blish_HUD.Settings;

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
    }
}
