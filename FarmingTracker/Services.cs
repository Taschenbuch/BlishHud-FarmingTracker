using Blish_HUD.Modules.Managers;
using System;
using System.Diagnostics;

namespace FarmingTracker
{
    public class Services : IDisposable
    {
        public Services(ContentsManager contentsManager, Gw2ApiManager gw2ApiManager, SettingService settingService)
        {
            Gw2ApiManager = gw2ApiManager;
            SettingService = settingService;
            Drf = new Drf(settingService); // todo ggf. nur settingService übergeben, wenn er nur den braucht?
            TextureService = new TextureService(contentsManager);
            FarmingTimeStopwatch.Restart();
        }

        public void Dispose()
        {
            Drf?.Dispose();
            TextureService?.Dispose();
        }

        public UpdateLoop UpdateLoop { get; } = new UpdateLoop();
        public FontService FontService { get; } = new FontService();
        public TextureService TextureService { get; }
        public Gw2ApiManager Gw2ApiManager { get; }
        public SettingService SettingService { get; }
        public Drf Drf { get; }
        public Stats Stats { get; } = new Stats();
        public Stopwatch FarmingTimeStopwatch { get; } = new Stopwatch();
        public string SearchTerm { get; set; } = string.Empty;
    }
}
