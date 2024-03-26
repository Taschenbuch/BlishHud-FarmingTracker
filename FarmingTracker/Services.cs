using Blish_HUD.Modules.Managers;

namespace FarmingTracker
{
    public class Services
    {
        public Services(ContentsManager contentsManager, Gw2ApiManager gw2ApiManager)
        {
            ContentsManager = contentsManager;
            Gw2ApiManager = gw2ApiManager;
        }

        public ContentsManager ContentsManager { get; }
        public Gw2ApiManager Gw2ApiManager { get; }
    }
}
