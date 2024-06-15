using System.Linq;

namespace FarmingTracker
{
    public class FileModelService
    {
        public static Model CreateModel(FileModel fileModel)
        {
            var model = new Model();

            model.FarmingDuration.Elapsed = fileModel.FarmingDuration;

            foreach (var fileItem in fileModel.FileItems)
            {
                var item = new Stat
                {
                    StatType = StatType.Item,
                    ApiId = fileItem.ApiId,
                    Count = fileItem.Count,
                };

                model.ItemById.AddOrUpdate(fileItem.ApiId, item, (key, oldValue) => item);
            }
            

            model.IgnoredItemApiIds = new SafeList<int>(fileModel.IgnoredItemApiIds);

            // add ignoredItems to items to get their api data on module startup
            foreach (var ignoredItemApiId in fileModel.IgnoredItemApiIds)
            {
                var ignoredItem = new Stat
                {
                    StatType = StatType.Item,
                    ApiId = ignoredItemApiId,
                    Count = 0,
                };

                model.ItemById.TryAdd(ignoredItemApiId, ignoredItem);
            }

            foreach (var fileCurrency in fileModel.FileCurrencies)
            {
                var currency = new Stat
                {
                    StatType = StatType.Currency,
                    ApiId = fileCurrency.ApiId,
                    Count = fileCurrency.Count,
                };

                model.CurrencyById.AddOrUpdate(fileCurrency.ApiId, currency, (key, oldValue) => currency);
            }

            return model;
        }

        public static FileModel CreateFileModel(Model model)
        {
            var fileModel = new FileModel
            {
                FarmingDuration = model.FarmingDuration.Elapsed,
                IgnoredItemApiIds = model.IgnoredItemApiIds.ToListSafe(),
            };

            var items = model.ItemById.Values.Where(s => s.Count != 0);
            var currencies = model.CurrencyById.Values.Where(s => s.Count != 0);

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
