using System.Collections.Generic;

namespace FarmingTracker
{
    public class ModelCreator
    {
        public static Model CreateModel(FileModel fileModel)
        {
            var model = new Model();

            foreach (var fileStat in fileModel.FileStats)
                AddStatToModel(fileStat, model.Stats.ItemById, model.Stats.CurrencyById);

            model.Stats.UpdateStatsSnapshot();

            return model;
        }
        private static void AddStatToModel(FileStat fileStat, Dictionary<int, Stat> itemById, Dictionary<int, Stat> currencyById)
        {
            var statById = fileStat.StatType == StatType.Item
                ? itemById
                : currencyById;

            statById[fileStat.ApiId] = new Stat
            {
                ApiId = fileStat.ApiId,
                StatType = fileStat.StatType,
                Signed_Count = fileStat.Signed_Count,
                StatVisibility = fileStat.StatVisibility,
                Unsigned_CustomProfitInCopper = fileStat.Unsigned_CustomProfitInCopper,
            };
        }
    }
}
