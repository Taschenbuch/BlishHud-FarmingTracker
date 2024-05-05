﻿namespace FarmingTracker
{
    // can be currency or item
    public class Stat
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Tooltip { get; set; } = string.Empty;
        public int Count { get; set; }
        public int ApiId { get; set; }
        public int IconAssetId { get; set; }
        public bool ApiDetailsAreMissing => IconAssetId == 0;
        public bool NotFoundByApi => IconAssetId == IconAssetIdAndTooltipSetter.BUG_TEXTURE_ASSET_ID;
        public bool IsCoin => ApiId == Coin.COIN_CURRENCY_ID;
    }
}