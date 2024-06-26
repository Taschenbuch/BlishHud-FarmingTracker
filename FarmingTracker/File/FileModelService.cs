﻿using System.Linq;

namespace FarmingTracker
{
    public class FileModelService
    {
        public static Model CreateModel(FileModel fileModel)
        {
            var model = new Model
            {
                FavoriteItemApiIds = new SafeList<int>(fileModel.FavoriteItemApiIds)
            };

            foreach (var fileCurrency in fileModel.FileCurrencies)
                model.CurrencyById[fileCurrency.ApiId] = new Stat
                {
                    ApiId = fileCurrency.ApiId,
                    Count = fileCurrency.Count,
                };

            foreach (var fileItem in fileModel.FileItems)
                model.ItemById[fileItem.ApiId] = new Stat
                {
                    ApiId = fileItem.ApiId,
                    Count = fileItem.Count,
                };
            
            model.IgnoredItemApiIds = new SafeList<int>(fileModel.IgnoredItemApiIds);

            // add ignoredItems to items to get their api data on module startup
            foreach (var ignoredItemApiId in fileModel.IgnoredItemApiIds)
                if (!model.ItemById.ContainsKey(ignoredItemApiId))
                    model.ItemById[ignoredItemApiId] = new Stat
                    {
                        ApiId = ignoredItemApiId,
                        Count = 0,
                    };

            model.UpdateStatsSnapshot();

            return model;
        }

        public static FileModel CreateFileModel(Model model)
        {
            var fileModel = new FileModel
            {
                IgnoredItemApiIds = model.IgnoredItemApiIds.ToListSafe(),
                FavoriteItemApiIds = model.FavoriteItemApiIds.ToListSafe(),
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
    }
}
