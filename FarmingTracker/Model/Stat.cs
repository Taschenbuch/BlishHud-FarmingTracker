using System;

namespace FarmingTracker
{
    // can be currency or item
    public class Stat
    {
        public int ApiId { get; set; }
        public StatType StatType { get; set; }
        public StatVisibility StatVisibility { get; set; }
        public long Signed_Count { get; set; }
        public ApiStatDetails Details { get; } = new ApiStatDetails();
        public Profit Profit { get; } = new Profit();
        public long CountSign => Math.Sign(Signed_Count);
        public bool IsSingleItem => Math.Abs(Signed_Count) == 1;
        public bool IsItem => StatType == StatType.Item;
        public bool IsCurrency => StatType == StatType.Currency;
        public bool IsCoin => ApiId == Coin.COIN_CURRENCY_ID && IsCurrency;
        public bool IsCoinOrCustomCoin => IsCoin || Details.IsCustomCoinStat;
    }
}
