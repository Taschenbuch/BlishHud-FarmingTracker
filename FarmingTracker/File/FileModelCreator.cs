using System.Collections.Generic;
using System.Linq;

namespace FarmingTracker
{
    public class FileModelCreator
    {
        public static FileModel CreateFileModel(Model model)
        {
            var fileModel = new FileModel
            {
                IgnoredItemApiIds = model.IgnoredItemApiIds.ToListSafe(),
                FavoriteItemApiIds = model.FavoriteItemApiIds.ToListSafe(),
                CustomStatProfits = model.CustomStatProfits.ToListSafe(),
            };

            var snapshot = model.Stats.StatsSnapshot;
            var items = snapshot.ItemById.Values.Where(s => s.Signed_Count != 0).ToList();
            var currencies = snapshot.CurrencyById.Values.Where(s => s.Signed_Count != 0).ToList();

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
                    Count = stat.Signed_Count,
                };
        }
    }
}
