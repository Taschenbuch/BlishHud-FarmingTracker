using System.Collections.Generic;
using System.Linq;

namespace FarmingTracker
{
    public class ModelCreator
    {
        public static Model CreateModel(FileModel fileModel)
        {
            var ignoredStats = CreateFavoriteStats(fileModel.FileStats.Where(f => f.IsIgnored)).ToList();
            var favoriteStats = CreateFavoriteStats(fileModel.FileStats.Where(f => f.IsFavorite)).ToList();
            var customStatProfits = CreateCustomStatProfits(fileModel.FileStats.Where(f => f.CustomStatProfit != null)).ToList();

            var model = new Model
            {
                IgnoredStats = new SafeList<FavoriteStat>(ignoredStats),
                FavoriteStats = new SafeList<FavoriteStat>(favoriteStats),
                CustomStatProfits = new SafeList<CustomStatProfit>(customStatProfits)
            };

            foreach (var fileStat in fileModel.FileStats)
                AddStatToModel(fileStat, model.Stats.ItemById, model.Stats.CurrencyById);

            model.Stats.UpdateStatsSnapshot();

            return model;
        }

        private static IEnumerable<CustomStatProfit> CreateCustomStatProfits(IEnumerable<FileStat> fileStats)
        {
            foreach (var fileStat in fileStats)
                yield return new CustomStatProfit
                {
                    ApiId = fileStat.ApiId,
                    StatType = fileStat.StatType,
                    Unsigned_CustomProfitInCopper = fileStat.CustomStatProfit ?? 1, // "?? 1" will no happen but compiler is happy. type interference from null guard 
                };
        }

        private static IEnumerable<FavoriteStat> CreateFavoriteStats(IEnumerable<FileStat> fileStats)
        {
            foreach (var fileStat in fileStats)
                yield return new FavoriteStat
                {
                    ApiId = fileStat.ApiId,
                    StatType = fileStat.StatType,
                };
        }

        private static void AddStatToModel(FileStat fileStat, Dictionary<int, Stat> itemById, Dictionary<int, Stat> currencyById)
        {
            var statById = fileStat.StatType == StatType.Item
                ? itemById
                : currencyById;

            statById[fileStat.ApiId] = new Stat
            {
                ApiId = fileStat.ApiId,
                StatType = fileStat.StatType,
                Signed_Count = fileStat.Signed_Count,
            };
        }
    }
}
