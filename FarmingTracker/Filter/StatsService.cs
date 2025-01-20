using System.Collections.Generic;
using System.Linq;

namespace FarmingTracker
{
    public class StatsService
    {
        public static (List<Stat> items, List<Stat> currencies) ShallowCopyStatsToPreventModification(StatsSnapshot snapshot)
        {
            var items = snapshot.ItemById.Values.ToList();
            var currencies = snapshot.CurrencyById.Values.ToList();
            return (items, currencies);
        }

        // normally after an api error, the UI is not updated. So stats that did not get api details yet, do not show up in the UI until the next success api call.
        // But when the UI is updated due to a user action (changed sort, changed filter, ...), those missing-details-stats would be displayed without name, icon, tooltip.
        // this method prevents that they are displayed.
        public static (List<Stat> items, List<Stat> currencies) RemoveStatsNotUpdatedYetDueToApiError(List<Stat> items, List<Stat> currencies)
        {
            items = items.Where(s => s.Details.State != ApiStatDetailsState.MissingBecauseApiNotCalledYet).ToList();
            currencies = currencies.Where(s => s.Details.State != ApiStatDetailsState.MissingBecauseApiNotCalledYet).ToList();
            return (items, currencies);
        }

        public static (List<Stat> items, List<Stat> currencies) RemoveZeroCountStats(List<Stat> items, List<Stat> currencies)
        {
            items = items.Where(s => s.Signed_Count != 0).ToList();
            currencies = currencies.Where(s => s.Signed_Count != 0).ToList();
            return (items, currencies);
        }

        public static void ResetCounts(Dictionary<int, Stat> statById)
        {
            foreach (var stat in statById.Values)
                stat.Signed_Count = 0;
        }

        public static (List<Stat> items, List<Stat> currencies) RemoveIgnoredStats(List<Stat> items, List<Stat> currencies, Model model)
        {
            var notIgnoredStats = model.Stats.ItemById.Values // todo x lock?
                .Concat(model.Stats.CurrencyById.Values)
                .Where(s => s.StatVisibility != StatVisibility.Ignored);

            items = notIgnoredStats.Where(s => s.StatType == StatType.Item).ToList();
            currencies = notIgnoredStats.Where(s => s.StatType == StatType.Currency).ToList();
            return (items, currencies);
        }

        public static (List<Stat> items, List<Stat> currencies, List<Stat> favorites) SplitFavoritesFromCurrenciesAndItems(
            List<Stat> items, 
            List<Stat> currencies, 
            List<FavoriteStat> favoriteStats)
        {
            if (favoriteStats.IsEmpty())
                return (items, currencies, new List<Stat>());  

            var favorites = new List<Stat>();

            foreach (var favoriteStat in favoriteStats.OrderBy(f => f.StatType)) // OrderBy: to show always show currencies first.
                switch (favoriteStat.StatType)
                {
                    case StatType.Item:
                        MoveToFavorites(items, favorites, favoriteStat.ApiId);
                        break;
                    case StatType.Currency:
                        MoveToFavorites(currencies, favorites, favoriteStat.ApiId);
                        break;
                }

            return (items, currencies, favorites);
        }

        private static void MoveToFavorites(List<Stat> stats, List<Stat> favorites, int favoriteApiId)
        {
            var matchingStat = stats.Find(c => c.ApiId == favoriteApiId);
            if (matchingStat == null) // stat not farmed in this session
                return;

            stats.Remove(matchingStat);
            favorites.Add(matchingStat);
        }
    }
}
