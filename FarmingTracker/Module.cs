using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Blish_HUD;
using Blish_HUD.Graphics.UI;
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
            return new ModuleSettingsView(_farmingTrackerWindowService);
        }

        protected override async Task LoadAsync()
        {
            _farmingTrackerWindowService = new FarmingTrackerWindowService(_services);
            _trackerCornerIcon = new TrackerCornerIcon(ContentsManager, _farmingTrackerWindowService);
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
        }


        private TrackerCornerIcon _trackerCornerIcon;
        private FarmingTrackerWindowService _farmingTrackerWindowService;
        private Services _services;
    }
}
