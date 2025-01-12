namespace FarmingTracker
{
    public class CustomStatProfit
    {
        public CustomStatProfit()
        {
        }

        public CustomStatProfit(int apiId, StatType statType)
        {
            ApiId = apiId;
            StatType = statType;
        }

        public int ApiId { get; set; }
        
        public StatType StatType { get; set; }
        
        public long Unsigned_CustomProfitInCopper { get; set; }
        
        public bool BelongsToStat(Stat stat)
        {
            return ApiId == stat.ApiId && StatType == stat.StatType;
        }
    }
}
