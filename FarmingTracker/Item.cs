namespace FarmingTracker
{
    public class ItemX // todo xgw2sharp hat auch Item. rename später mal
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Count { get; set; }
        public int ApiItemId { get; set; }
        public int AssetId { get; set; }
    }
}
