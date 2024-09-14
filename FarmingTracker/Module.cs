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
            _dateTimeService = new DateTimeService(settings); // defines settings. Must be called here.
        }

        public override IView GetSettingsView()
        {
            return _moduleLoadError.HasModuleLoadFailed
                ? _moduleLoadError.CreateErrorSettingsView()
                : new ModuleSettingsView(_farmingTrackerWindow);
        }

        protected override async Task LoadAsync()
        {
            var services = new Services(ContentsManager, DirectoriesManager, Gw2ApiManager, _settingService, _dateTimeService);
            var model = await services.FileLoader.LoadModelFromFile();
            _model = model;
            _services = services;

            if(_services.Drf.WindowsVersionIsTooLowToSupportWebSockets)
            {
                _moduleLoadError.InitializeErrorSettingsViewAndShowErrorWindow(
                    $"{Name}: Module does not work :-(",
                    "Your Windows version is too old. This module requires at least Windows 8\nbecause DRF relies on the WebSocket technology.");

                return;
            }

            _farmingTrackerWindow = new FarmingTrackerWindow(570, 650, model, services);
            _trackerCornerIcon = new TrackerCornerIcon(services, CornerIconClickEventHandler);

            _services.SettingService.WindowVisibilityKeyBindingSetting.Value.Activated += OnWindowVisibilityKeyBindingActivated;
            _services.SettingService.WindowVisibilityKeyBindingSetting.Value.Enabled = true;
        }

        protected override void Update(GameTime gameTime)
        {
            if(_moduleLoadError.HasModuleLoadFailed)
                return;

            _farmingTrackerWindow.Update2(gameTime);
        }

        protected override void Unload()
        {
            _moduleLoadError?.Dispose();
            _trackerCornerIcon?.Dispose();
            _farmingTrackerWindow?.Dispose();
            _services?.Dispose();
            _services.SettingService.WindowVisibilityKeyBindingSetting.Value.Enabled = false;
            _services.SettingService.WindowVisibilityKeyBindingSetting.Value.Activated -= OnWindowVisibilityKeyBindingActivated;
            if(_model != null)
            {
                _services.FarmingDuration.SaveFarmingTime();
                _services.FileSaver.SaveModelToFileSync(_model);
            }
        }

        private void OnWindowVisibilityKeyBindingActivated(object sender, System.EventArgs e) => _farmingTrackerWindow.ToggleWindowAndSelectSummaryTab();
        private void CornerIconClickEventHandler(object s, MouseEventArgs e) => _farmingTrackerWindow.ToggleWindowAndSelectSummaryTab();

        private readonly ModuleLoadError _moduleLoadError = new ModuleLoadError();
        private TrackerCornerIcon _trackerCornerIcon;
        private FarmingTrackerWindow _farmingTrackerWindow;
        private SettingService _settingService;
        private DateTimeService _dateTimeService;
        private Services _services;
        private Model _model;
    }
}
