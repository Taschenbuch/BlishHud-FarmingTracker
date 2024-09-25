using System;

namespace FarmingTracker
{
    // can be currency or item
    public class Stat
    {
        public int ApiId { get; set; }
        public StatType StatType { get; set; }
        public long Count { get; set; }
        public long CountSign => Math.Sign(Count);
        public bool IsSingleItem => Math.Abs(Count) == 1;
        public ApiStatDetails Details { get; set; } = new ApiStatDetails();
        public Profits Profits { get; set; } = new Profits();
        public bool IsCoin => ApiId == Coin.COIN_CURRENCY_ID;
        public bool IsCoinOrCustomCoin => IsCoin || Details.IsCustomCoinStat;
    }
}
