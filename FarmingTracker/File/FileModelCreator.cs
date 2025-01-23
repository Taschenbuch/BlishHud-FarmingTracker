using System.Linq;

namespace FarmingTracker
{
    public class FileModelCreator
    {
        public static FileModel CreateFileModel(Model model)
        {
            var stats = model.Stats.ItemById.Values
                .Concat(model.Stats.CurrencyById.Values)
                .Where(s => s.Signed_Count != 0 || s.StatVisibility != StatVisibility.Regular) // prevents saving too many not tracked stats due to stats reset.
                .ToList();

            var fileModel = new FileModel();

            foreach (var stat in stats)
            {
                var fileStat = new FileStat
                {
                    ApiId = stat.ApiId,
                    StatType = stat.StatType,
                    Signed_Count = stat.Signed_Count,
                    StatVisibility = stat.StatVisibility,
                    Unsigned_CustomProfitInCopper = stat.Unsigned_CustomProfitInCopper,
                };

                fileModel.FileStats.Add(fileStat);
            }

            return fileModel;
        }
    }
}
