using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Blish_HUD;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Input;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework;

namespace FarmingTracker
{
    [Export(typeof(Blish_HUD.Modules.Module))]
    public class Module : Blish_HUD.Modules.Module
    {
        public static readonly Logger Logger = Logger.GetLogger<Module>();

        internal SettingsManager SettingsManager => ModuleParameters.SettingsManager;
        internal ContentsManager ContentsManager => ModuleParameters.ContentsManager;
        internal DirectoriesManager DirectoriesManager => ModuleParameters.DirectoriesManager;
        internal Gw2ApiManager Gw2ApiManager => ModuleParameters.Gw2ApiManager;

        [ImportingConstructor]
        public Module([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters)
        {
        }

        protected override void DefineSettings(SettingCollection settings)
        {
            _settingService = new SettingService(settings);
        }

        public override IView GetSettingsView()
        {
            return new ModuleSettingsView(_farmingTrackerWindowService);
        }

        protected override async Task LoadAsync()
        {
            _services = await Services.CreateServices(ContentsManager, DirectoriesManager, Gw2ApiManager, _settingService);
            _farmingTrackerWindowService = new FarmingTrackerWindowService(_services);
            _trackerCornerIcon = new TrackerCornerIcon(_services, CornerIconClickEventHandler);

            _services.SettingService.WindowVisibilityKeyBindingSetting.Value.Activated += OnWindowVisibilityKeyBindingActivated;
            _services.SettingService.WindowVisibilityKeyBindingSetting.Value.Enabled = true;
        }

        protected override void Update(GameTime gameTime)
        {
            _farmingTrackerWindowService.Update(gameTime);
        }

        protected override void Unload()
        {
            _trackerCornerIcon?.Dispose();
            _farmingTrackerWindowService?.Dispose();
            _services?.Dispose();
            _services.SettingService.WindowVisibilityKeyBindingSetting.Value.Enabled = false;
            _services.SettingService.WindowVisibilityKeyBindingSetting.Value.Activated -= OnWindowVisibilityKeyBindingActivated;
            _services.FileSaveService.SaveModelToFileSync(_services.Model);
        }

        private void OnWindowVisibilityKeyBindingActivated(object sender, System.EventArgs e) => _farmingTrackerWindowService.ToggleWindowAndSelectSummaryTab();
        private void CornerIconClickEventHandler(object s, MouseEventArgs e) => _farmingTrackerWindowService.ToggleWindowAndSelectSummaryTab();

        private TrackerCornerIcon _trackerCornerIcon;
        private FarmingTrackerWindowService _farmingTrackerWindowService;
        private SettingService _settingService;
        private Services _services;
    }
}
