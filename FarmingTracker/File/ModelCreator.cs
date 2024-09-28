using System.Collections.Generic;

namespace FarmingTracker
{
    public class ModelCreator
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
