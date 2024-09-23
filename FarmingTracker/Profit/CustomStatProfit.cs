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
        public int CustomProfitInCopper { get; set; }
    }
}
