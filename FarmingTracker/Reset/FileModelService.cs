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
                model.ItemById[fileItem.ApiId] = new Stat
                {
                    ApiId = fileItem.ApiId,
                    Count = fileItem.Count,
                };

            foreach (var fileCurrency in fileModel.FileCurrencies)
                model.CurrencyById[fileCurrency.ApiId] = new Stat
                {
                    ApiId = fileCurrency.ApiId,
                    Count = fileCurrency.Count,
                };

            return model;
        }

        public static FileModel CreateFileModel(Model model)
        {
            var items = model.ItemById.Values.ToList().Where(s => s.Count != 0);
            var currencies = model.CurrencyById.Values.ToList().Where(s => s.Count != 0);

            var fileModel = new FileModel
            {
                FarmingDuration = model.FarmingDuration.Elapsed
            };

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
