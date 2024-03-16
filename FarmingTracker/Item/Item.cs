namespace FarmingTracker
{
    public class ItemX // todogw2sharp hat auch Item. rename später mal. ggf. einfach "stat" nennen?
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Count { get; set; }
        public int ApiId { get; set; }
        public int IconAssetId { get; set; }
        public ApiIdType ApiIdType { get; set; } // todo überflüssig?
    }
}
