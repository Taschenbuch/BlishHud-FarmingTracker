using System;
using System.Collections.Generic;
using System.Linq;

namespace FarmingTracker
{
    public class FileModelCreator
    {
        public static FileModel CreateFileModel(Model model)
        {
            var snapshot = model.Stats.StatsSnapshot;
            var items = snapshot.ItemById.Values.Where(s => s.Signed_Count != 0).ToList();
            var currencies = snapshot.CurrencyById.Values.Where(s => s.Signed_Count != 0).ToList();
            var stats = items.Concat(currencies);

            var fileStats = new List<FileStat>();

            foreach (var stat in stats)
            {
                var fileStat = new FileStat
                {
                    ApiId = stat.ApiId,
                    StatType = stat.StatType,
                    Signed_Count = stat.Signed_Count,
                };

                fileStats.Add(fileStat);
            }

            foreach (var ignoredStat in model.IgnoredStats.ToListSafe())
                SetFileStatProperty(fileStats, ignoredStat.ApiId, ignoredStat.StatType, (fileStat) => { fileStat.StatVisibility = StatVisibility.Ignored; });

            foreach (var favoriteStat in model.FavoriteStats.ToListSafe())
                SetFileStatProperty(fileStats, favoriteStat.ApiId, favoriteStat.StatType, (fileStat) => { fileStat.StatVisibility = StatVisibility.Favorite; });

            foreach (var customStatProfit in model.CustomStatProfits.ToListSafe())
                SetFileStatProperty(fileStats, customStatProfit.ApiId, customStatProfit.StatType, (fileStat) => { fileStat.CustomStatProfit = customStatProfit.Unsigned_CustomProfitInCopper; });

            return new FileModel
            {
                FileStats = fileStats,
            };
        }

        private static void SetFileStatProperty(List<FileStat> fileStats, int apiId, StatType statType, Action<FileStat> setFileStatProperty)
        {
            var matchingFileStat = fileStats.Find(f => f.ApiId == apiId && f.StatType == statType);

            if (matchingFileStat == null)
            {
                Module.Logger.Error("Could not set property because FileStat not exist. That should not be possible.");
                return;
            }

            setFileStatProperty(matchingFileStat);
        }
    }
}
