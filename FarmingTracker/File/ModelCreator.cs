namespace FarmingTracker
{
    public class ModelCreator
    {
        public static Model CreateModel(FileModel fileModel)
        {
            var model = new Model();

            foreach (var fileStat in fileModel.FileStats)
                AddStatToModel(fileStat, model.Stats);

            return model;
        }
        private static void AddStatToModel(FileStat fileStat, Stats stats)
        {
            var stat = new Stat
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

            stats.AddStat(stat);
        }
    }
}
