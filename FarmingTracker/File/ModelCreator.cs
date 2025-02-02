using System.Collections.Generic;

namespace FarmingTracker
{
    public class ModelCreator
    {
        public static Model CreateModel(FileModel fileModel)
        {
            var model = new Model();

            foreach (var fileStat in fileModel.FileStats)
                AddStatToModel(fileStat, model.Stats.StatById);

            return model;
        }
        private static void AddStatToModel(FileStat fileStat, Dictionary<int, Stat> statById)
        {
            var key = fileStat.StatType == StatType.Item
                ? fileStat.ApiId 
                : -fileStat.ApiId;

            statById[key] = new Stat
            {
                ApiId = fileStat.ApiId,
                StatType = fileStat.StatType,
                Signed_Count =
                {
                    Value = fileStat.Signed_Count,
                },
                StatVisibility = fileStat.StatVisibility,
                Profit = 
                { 
                    Unsigned_CustomProfitInCopper = fileStat.Unsigned_CustomProfitInCopper 
                },
            };
        }
    }
}
