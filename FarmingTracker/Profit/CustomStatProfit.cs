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
        public StatType StatType { get; } = StatType.Item;
        public int CustomProfitInCopper { get; set; }
    }
}
