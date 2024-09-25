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

        public static bool ProfitBelongsToStat(CustomStatProfit customStatProfit, Stat stat)
        {
            return customStatProfit.ApiId == stat.ApiId && customStatProfit.StatType == stat.StatType;
        }
    }
}
