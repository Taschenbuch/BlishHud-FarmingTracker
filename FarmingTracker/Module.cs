using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace FarmingTracker
{
    [Export(typeof(Blish_HUD.Modules.Module))]
    public class Module : Blish_HUD.Modules.Module
    {
        public static readonly Logger Logger = Logger.GetLogger<Module>();

        internal SettingsManager SettingsManager => this.ModuleParameters.SettingsManager;
        internal ContentsManager ContentsManager => this.ModuleParameters.ContentsManager;
        internal DirectoriesManager DirectoriesManager => this.ModuleParameters.DirectoriesManager;
        internal Gw2ApiManager Gw2ApiManager => this.ModuleParameters.Gw2ApiManager;

        [ImportingConstructor]
        public Module([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters)
        {
        }

        protected override void DefineSettings(SettingCollection settings)
        {
            _services = new Services(ContentsManager, Gw2ApiManager, new SettingService(settings));
        }

        public override IView GetSettingsView()
        {
            // hack: "return new ModuleSettingsView();" would make more sense
            // but currently there is the blish core bug that SettingsView.Unload() is not called. Because of that this hack is required
            // to unsubscribe everytime View.Build() is called. See details in ModuleSettingsView.Build();
            _moduleSettingsView ??= new ModuleSettingsView(_services); 
            return _moduleSettingsView;
        }

        protected override async Task LoadAsync()
        {
            _farmingTrackerWindow = await CreateFarmingTrackerWindow();
            _trackerCornerIcon = new TrackerCornerIcon(ContentsManager, _farmingTrackerWindow);
        }

        protected override void Update(GameTime gameTime)
        {
            _farmingTrackerWindow.Update2(gameTime);
        }

        protected override void Unload()
        {
            _moduleSettingsView?.Dispose();
            _trackerCornerIcon?.Dispose();
            _farmingTrackerWindow?.Dispose();
            _services?.Dispose();
        }

        private async Task<FarmingTrackerWindow> CreateFarmingTrackerWindow()
        {
            var windowWidth = 560;
            var windowHeight = 640;
            var flowPanelWidth = windowWidth - 47;

            var farmingTrackerWindow = new FarmingTrackerWindow(
                AsyncTexture2D.FromAssetId(155997),
                new Rectangle(25, 26, windowWidth, windowHeight),
                new Rectangle(40, 50, windowWidth - 20, windowHeight - 50),
                flowPanelWidth,
                _services);

            //await farmingTrackerWindow.InitAsync(); // todo weg?
            return farmingTrackerWindow;
        }

        private TrackerCornerIcon _trackerCornerIcon;
        private FarmingTrackerWindow _farmingTrackerWindow;
        private Services _services;
        private ModuleSettingsView _moduleSettingsView;
    }
}
