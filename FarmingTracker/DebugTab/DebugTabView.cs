using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;

namespace FarmingTracker
{
    public class DebugTabView : View
    {
        public DebugTabView(Services services)
        {
            _services = services;
        }

        protected override void Build(Container buildPanel)
        {
            SettingControls.CreateSetting(buildPanel, _services.SettingService.IsFakeDrfServerUsedSetting);
        }

        private readonly Services _services;
    }
}
