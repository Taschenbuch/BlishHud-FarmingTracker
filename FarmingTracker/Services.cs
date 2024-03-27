using Blish_HUD.Modules.Managers;

namespace FarmingTracker
{
    public class Services
    {

        public FontService FontService { get; } = new FontService();
        public ContentsManager ContentsManager { get; set; }
        public Gw2ApiManager Gw2ApiManager { get; set; }
        public SettingService SettingService { get; set; }
    }
}
