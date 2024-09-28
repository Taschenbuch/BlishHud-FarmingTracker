namespace FarmingTracker
{
    public class CustomStatProfit
    {
        public CustomStatProfit(int apiId, StatType statType)
        {
            ApiId = apiId;
            StatType = statType;
        }

        public int ApiId { get; }
        public StatType StatType { get; }
        public long CustomProfitInCopper { get; set; }

        public bool BelongsToStat(Stat stat)
        {
            return ApiId == stat.ApiId && StatType == stat.StatType;
        }
    }
}
