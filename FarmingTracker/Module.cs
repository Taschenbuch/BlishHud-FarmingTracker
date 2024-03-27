using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
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
            _trackerCornerIcon?.Dispose();
            _farmingTrackerWindow?.Dispose();
        }

        private async Task<FarmingTrackerWindow> CreateFarmingTrackerWindow()
        {
            var windowWidth = 560;
            var windowHeight = 640;
            var flowPanelWidth = windowWidth - 47;

            var services = new Services(ContentsManager, Gw2ApiManager);

            var farmingTrackerWindow = new FarmingTrackerWindow(
                AsyncTexture2D.FromAssetId(155997),
                new Rectangle(25, 26, windowWidth, windowHeight),
                new Rectangle(40, 50, windowWidth - 20, windowHeight - 50),
                flowPanelWidth,
                services);

            await farmingTrackerWindow.InitAsync();
            return farmingTrackerWindow;
        }

        private TrackerCornerIcon _trackerCornerIcon;
        private FarmingTrackerWindow _farmingTrackerWindow;
    }
}
