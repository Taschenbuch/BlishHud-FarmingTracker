namespace FarmingTracker
{
    public class ItemX // todogw2sharp hat auch Item. rename später mal. ggf. einfach "stat" nennen? weil für currency und item benutzt
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Tooltip { get; set; } = string.Empty;
        public int Count { get; set; }
        public int ApiId { get; set; }
        public int IconAssetId { get; set; }
        public bool IsApiInfoMissing => IconAssetId == 0 || IconAssetId == IconAssetIdAndTooltipSetter.BUG_TEXTURE_ASSET_ID;
    }
}
