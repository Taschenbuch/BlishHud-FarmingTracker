using System.Collections.Generic;

namespace FarmingTracker
{
    public class ModelCreator
    {
        public static Model CreateModel(FileModel fileModel)
        {
            var model = new Model
            {
                IgnoredStats = new SafeList<FavoriteStat>(fileModel.IgnoredStats),
                FavoriteStats = new SafeList<FavoriteStat>(fileModel.FavoriteStats),
                CustomStatProfits = new SafeList<CustomStatProfit>(fileModel.CustomStatProfits)
            };

            AddStatsToModel(model.Stats.CurrencyById, fileModel.FileCurrencies, StatType.Currency);
            AddStatsToModel(model.Stats.ItemById, fileModel.FileItems, StatType.Item);

            // add customStatProfits to items and currenciens to get their api data on module startup
            foreach (var customStatProfit in fileModel.CustomStatProfits)
                AddStatToModelIfMissing(customStatProfit.ApiId, customStatProfit.StatType, model.Stats.ItemById, model.Stats.CurrencyById);

            // add ignored stats to stats to get their api data on module startup
            foreach (var ignoredStat in fileModel.IgnoredStats)
                AddStatToModelIfMissing(ignoredStat.ApiId, ignoredStat.StatType, model.Stats.ItemById, model.Stats.CurrencyById);

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
                    Signed_Count = fileStat.Count,
                };
        }

        private static void AddStatToModelIfMissing(int statApiId, StatType statType, Dictionary<int, Stat> itemById, Dictionary<int, Stat> currencyById)
        {
            var statById = statType == StatType.Item
                ? itemById
                : currencyById;

            if (statById.ContainsKey(statApiId))
                return;

            statById[statApiId] = new Stat
            {
                ApiId = statApiId,
                StatType = statType,
                Signed_Count = 0,
            };
        }
    }
}
