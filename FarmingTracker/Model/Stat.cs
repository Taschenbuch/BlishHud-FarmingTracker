using System;

namespace FarmingTracker
{
    // can be currency or item
    public class Stat
    {
        public int ApiId { get; set; }
        public StatType StatType { get; set; }
        public bool IsItem => StatType == StatType.Item;
        public bool IsCurrency => StatType == StatType.Currency;
        public StatVisibility StatVisibility { get; set; }
        public long Signed_Count { get; set; }
        public long CountSign => Math.Sign(Signed_Count);
        public bool IsSingleItem => Math.Abs(Signed_Count) == 1;
        public ApiStatDetails Details { get; set; } = new ApiStatDetails();
        public Profit Profit { get; set; } = new Profit();
        public bool IsCoin => ApiId == Coin.COIN_CURRENCY_ID && StatType == StatType.Currency;
        public bool IsCoinOrCustomCoin => IsCoin || Details.IsCustomCoinStat;
    }
}
