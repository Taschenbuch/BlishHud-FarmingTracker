namespace FarmingTracker
{
    public class FavoriteStatService
    {
        public static void RemoveFromFavoriteStats(Stat stat, SafeList<FavoriteStat> favoriteStats, Services services)
        {
            var apiId = ReplaceApiIdIfCustomCoin(stat);
            var matchingFavoriteStat = favoriteStats.FirstOrDefaultSafe(f => f.StatType == stat.StatType && f.ApiId == apiId);

            if (matchingFavoriteStat == null)
            {
                Module.Logger.Error("Item is not a favorite item. It shouldnt have been displayed in the first place.");
                return;
            }

            favoriteStats.RemoveSafe(matchingFavoriteStat);
            services.UpdateLoop.TriggerUpdateUi();
            services.UpdateLoop.TriggerSaveModel();
        }

        public static void AddToFavoriteStats(Stat stat, SafeList<FavoriteStat> favoriteStats, Services services)
        {
            var apiId = ReplaceApiIdIfCustomCoin(stat);

            if (favoriteStats.AnySafe(f => f.StatType == stat.StatType && f.ApiId == apiId))
            {
                Module.Logger.Error("Item is already a favorite item. It shouldnt have been displayed in the first place.");
                return;
            }

            var favoriteStat = new FavoriteStat()
            {
                StatType = stat.StatType,
                ApiId = apiId
            };

            favoriteStats.AddSafe(favoriteStat);
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
