using System.Linq;

namespace FarmingTracker
{
    public class FavoriteStatService
    {
        public static void RemoveFromFavoriteStats(Stat stat, Model model, Services services)
        {
            var apiId = ReplaceApiIdIfCustomCoin(stat);
            var matchingStat = model.Stats.GetStats().FirstOrDefault(s => s.StatType == stat.StatType && s.ApiId == apiId);

            if (matchingStat == null || matchingStat.StatVisibility != StatVisibility.Favorite)
            {
                Module.Logger.Error("Cannot remove stat from favorites. It does not exist or is no favorite.");
                return;
            }

            matchingStat.StatVisibility = StatVisibility.Regular;
            
            services.UpdateLoop.TriggerUpdateUi();
            services.UpdateLoop.TriggerSaveModel();
        }

        public static void AddToFavoriteStats(Stat stat, Model model, Services services)
        {
            var apiId = ReplaceApiIdIfCustomCoin(stat);
            var matchingStat = model.Stats.GetStats().FirstOrDefault(s => s.StatType == stat.StatType && s.ApiId == apiId);

            if (matchingStat == null || matchingStat.StatVisibility == StatVisibility.Favorite)
            {
                Module.Logger.Error("Cannot add stat to favorites. Stat does not exist or is already a favorite.");
                return;
            }

            matchingStat.StatVisibility = StatVisibility.Favorite;

            services.UpdateLoop.TriggerUpdateUi();
            services.UpdateLoop.TriggerSaveModel();
        }

        /// <summary>
        /// Easier to handle one coin id than 3 custom ids. And those custom coin ids do not exist yet durchin favorite split in ui update.
        /// </summary>
        private static int ReplaceApiIdIfCustomCoin(Stat stat)
        {
            return stat.Details.IsCustomCoinStat
                ? Coin.COIN_CURRENCY_ID
                : stat.ApiId;
        }
    }
}
