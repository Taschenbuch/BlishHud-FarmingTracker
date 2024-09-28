using System.Collections.Generic;
using System.Linq;

namespace FarmingTracker
{
    public class FileModelService
    {
        public static Model CreateModel(FileModel fileModel)
        {
            var model = new Model
            {
                IgnoredItemApiIds = new SafeList<int>(fileModel.IgnoredItemApiIds),
                FavoriteItemApiIds = new SafeList<int>(fileModel.FavoriteItemApiIds),
                CustomStatProfits = new SafeList<CustomStatProfit>(fileModel.CustomStatProfits)
            };

            AddStatsToModel(model.Stats.CurrencyById, fileModel.FileCurrencies, StatType.Currency);
            AddStatsToModel(model.Stats.ItemById, fileModel.FileItems, StatType.Item);

            // add customStatProfits to items and currenciens to get their api data on module startup
            foreach (var customStatProfit in fileModel.CustomStatProfits)
            {
                var statById = customStatProfit.StatType == StatType.Item
                    ? model.Stats.ItemById
                    : model.Stats.CurrencyById;

                AddStatToModelIfMissing(statById, customStatProfit.ApiId, customStatProfit.StatType);
            }

            // add ignoredItems to items to get their api data on module startup
            foreach (var ignoredItemApiId in fileModel.IgnoredItemApiIds)
                AddStatToModelIfMissing(model.Stats.ItemById, ignoredItemApiId, StatType.Item);

            model.Stats.UpdateStatsSnapshot();

            return model;
        }

        public static FileModel CreateFileModel(Model model)
        {
            var fileModel = new FileModel
            {
                IgnoredItemApiIds = model.IgnoredItemApiIds.ToListSafe(),
                FavoriteItemApiIds = model.FavoriteItemApiIds.ToListSafe(),
                CustomStatProfits = model.CustomStatProfits.ToListSafe(),
            };

            var snapshot = model.Stats.StatsSnapshot;
            var items = snapshot.ItemById.Values.Where(s => s.Count != 0).ToList();
            var currencies = snapshot.CurrencyById.Values.Where(s => s.Count != 0).ToList();

            var fileItems = CreateFileStats(items);
            var fileCurrencies = CreateFileStats(currencies);

            fileModel.FileItems.AddRange(fileItems);
            fileModel.FileCurrencies.AddRange(fileCurrencies);

            return fileModel;
        }

        private static IEnumerable<FileStat> CreateFileStats(List<Stat> stats)
        {
            foreach (var stat in stats)
                yield return new FileStat()
                {
                    ApiId = stat.ApiId,
                    Count = stat.Count,
                };
        }

        private static void AddStatsToModel(Dictionary<int, Stat> statById, List<FileStat> fileStats, StatType statType)
        {
            foreach (var fileStat in fileStats)
                statById[fileStat.ApiId] = new Stat
                {
                    ApiId = fileStat.ApiId,
                    StatType = statType,
                    Count = fileStat.Count,
                };
        }

        private static void AddStatToModelIfMissing(Dictionary<int, Stat> statById, int statId, StatType statType)
        {
            if (statById.ContainsKey(statId))
                return;

            statById[statId] = new Stat
            {
                ApiId = statId,
                StatType = statType,
                Count = 0,
            };
        }
    }
}
