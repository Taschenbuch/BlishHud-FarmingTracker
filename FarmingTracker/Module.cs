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
        public static bool DebugEnabled => _isDebugConfiguration || GameService.Debug.EnableDebugLogging.Value;
#if DEBUG
        private static readonly bool _isDebugConfiguration = true;
#else 
        private static bool _isDebugConfiguration = false;
#endif

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
            return _moduleLoadError.HasModuleLoadFailed
                ? _moduleLoadError.CreateErrorSettingsView()
                : new ModuleSettingsView(_farmingTrackerWindowService);
        }

        protected override async Task LoadAsync()
        {
            var services = new Services(ContentsManager, DirectoriesManager, Gw2ApiManager, _settingService);
            var model = await services.FileLoadService.LoadModelFromFile();
            _model = model;
            _services = services;

            if(_services.Drf.WindowsVersionIsTooLowToSupportWebSockets)
            {
                _moduleLoadError.InitializeErrorSettingsViewAndShowErrorWindow(
                    $"{Name}: Module does not work :-(",
                    "Your Windows version is too old. This module requires at least Windows 8\nbecause DRF relies on the WebSocket technology.");

                return;
            }

            _farmingTrackerWindowService = new FarmingTrackerWindowService(model, services);
            _trackerCornerIcon = new TrackerCornerIcon(services, CornerIconClickEventHandler);

            _services.SettingService.WindowVisibilityKeyBindingSetting.Value.Activated += OnWindowVisibilityKeyBindingActivated;
            _services.SettingService.WindowVisibilityKeyBindingSetting.Value.Enabled = true;
        }

        protected override void Update(GameTime gameTime)
        {
            if(_moduleLoadError.HasModuleLoadFailed)
                return;

            _farmingTrackerWindowService.Update(gameTime);
        }

        protected override void Unload()
        {
            _moduleLoadError?.Dispose();
            _trackerCornerIcon?.Dispose();
            _farmingTrackerWindowService?.Dispose();
            _services?.Dispose();
            _services.SettingService.WindowVisibilityKeyBindingSetting.Value.Enabled = false;
            _services.SettingService.WindowVisibilityKeyBindingSetting.Value.Activated -= OnWindowVisibilityKeyBindingActivated;
            
            if(_model != null)
                _services.FileSaveService.SaveModelToFileSync(_model);
        }

        private void OnWindowVisibilityKeyBindingActivated(object sender, System.EventArgs e) => _farmingTrackerWindowService.ToggleWindowAndSelectSummaryTab();
        private void CornerIconClickEventHandler(object s, MouseEventArgs e) => _farmingTrackerWindowService.ToggleWindowAndSelectSummaryTab();

        private readonly ModuleLoadError _moduleLoadError = new ModuleLoadError();
        private TrackerCornerIcon _trackerCornerIcon;
        private FarmingTrackerWindowService _farmingTrackerWindowService;
        private SettingService _settingService;
        private Services _services;
        private Model _model;
    }
}
