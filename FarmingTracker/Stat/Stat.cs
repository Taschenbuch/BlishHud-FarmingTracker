using System;

namespace FarmingTracker
{
    // can be currency or item
    public class Stat
    {
        public int ApiId { get; set; }
        public long Count { get; set; }
        public long CountSign => Math.Sign(Count);
        public ApiStatDetails Details { get; set; } = new ApiStatDetails();
        public Profit ProfitEach { get; set; } = new Profit();
        public Profit ProfitAll { get; set; } = new Profit();
        public string Tooltip { get; set; } = string.Empty;
        public bool IsCoin => ApiId == Coin.COIN_CURRENCY_ID;
    }
}
