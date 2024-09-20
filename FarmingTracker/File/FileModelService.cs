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
                FavoriteItemApiIds = new SafeList<int>(fileModel.FavoriteItemApiIds),
                CustomStatProfits = new SafeList<CustomStatProfit>(fileModel.CustomStatProfits)
            };

            foreach (var fileCurrency in fileModel.FileCurrencies)
                model.CurrencyById[fileCurrency.ApiId] = new Stat
                {
                    ApiId = fileCurrency.ApiId,
                    StatType = StatType.Currency,
                    Count = fileCurrency.Count,
                };

            foreach (var fileItem in fileModel.FileItems)
                model.ItemById[fileItem.ApiId] = new Stat
                {
                    ApiId = fileItem.ApiId,
                    StatType = StatType.Item,
                    Count = fileItem.Count,
                };
            
            model.IgnoredItemApiIds = new SafeList<int>(fileModel.IgnoredItemApiIds);

            // add customStatProfits to items and currenciens to get their api data on module startup
            foreach (var customStatProfit in fileModel.CustomStatProfits)
                switch (customStatProfit.StatType)
                {
                    case StatType.Item:
                        AddStatIfMissing(model.ItemById, customStatProfit.ApiId, StatType.Item);
                        break;
                    case StatType.Currency:
                        AddStatIfMissing(model.CurrencyById, customStatProfit.ApiId, StatType.Currency);
                        break;
                }

            // add ignoredItems to items to get their api data on module startup
            foreach (var ignoredItemApiId in fileModel.IgnoredItemApiIds)
                AddStatIfMissing(model.ItemById, ignoredItemApiId, StatType.Item);

            model.UpdateStatsSnapshot();

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

            var snapshot = model.StatsSnapshot;
            var items = snapshot.ItemById.Values.Where(s => s.Count != 0).ToList();
            var currencies = snapshot.CurrencyById.Values.Where(s => s.Count != 0).ToList();

            foreach (var item in items)
            {
                var fileStat = new FileStat()
                {
                    ApiId = item.ApiId,
                    Count = item.Count,
                };

                fileModel.FileItems.Add(fileStat);
            }

            foreach (var currency in currencies)
            {
                var fileStat = new FileStat()
                {
                    ApiId = currency.ApiId,
                    Count = currency.Count,
                };

                fileModel.FileCurrencies.Add(fileStat);
            }

            return fileModel;
        }

        private static void AddStatIfMissing(Dictionary<int, Stat> statById, int statId, StatType statType)
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
