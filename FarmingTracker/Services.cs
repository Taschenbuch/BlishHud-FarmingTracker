using Blish_HUD.Modules.Managers;
using System;
using System.Diagnostics;

namespace FarmingTracker
{
    public class Services : IDisposable
    {
        public Services(ContentsManager contentsManager, Gw2ApiManager gw2ApiManager, SettingService settingService)
        {
            ContentsManager = contentsManager;
            Gw2ApiManager = gw2ApiManager;
            SettingService = settingService;
            Drf = new Drf(settingService); // todo ggf. nur settingService übergeben, wenn er nur den braucht?
            FarmingTimeStopwatch.Restart();
        }

        public void Dispose()
        {
            Drf.Dispose();
        }

        public FontService FontService { get; } = new FontService();
        public ContentsManager ContentsManager { get; }
        public Gw2ApiManager Gw2ApiManager { get; }
        public SettingService SettingService { get; }
        public Drf Drf { get; set; }
        public Stats Stats { get; } = new Stats();
        public Stopwatch FarmingTimeStopwatch { get; } = new Stopwatch();
    }
}
