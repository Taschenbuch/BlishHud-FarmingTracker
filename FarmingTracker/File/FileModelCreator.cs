using System;
using System.Collections.Generic;
using System.Linq;

namespace FarmingTracker
{
    public class FileModelCreator
    {
        public static FileModel CreateFileModel(Model model)
        {
            var stats = model.Stats.ItemById.Values
                .Concat(model.Stats.CurrencyById.Values)
                .Where(s => s.Signed_Count != 0)
                .ToList();

            var fileStats = new List<FileStat>();

            foreach (var stat in stats)
            {
                var fileStat = new FileStat
                {
                    ApiId = stat.ApiId,
                    StatType = stat.StatType,
                    Signed_Count = stat.Signed_Count,
                    StatVisibility = stat.StatVisibility,
                };

                fileStats.Add(fileStat);
            }

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
