using Blish_HUD.Modules.Managers;
using System;
using System.Threading.Tasks;

namespace FarmingTracker
{
    public class Services : IDisposable
    {
        public Services(ContentsManager contentsManager, Gw2ApiManager gw2ApiManager, SettingService settingService)
        {
            ContentsManager = contentsManager;
            Gw2ApiManager = gw2ApiManager;
            SettingService = settingService;
            Drf = new Drf(this); // todo ggf. nur settingService übergeben, wenn er nur den braucht?
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
    }
}
