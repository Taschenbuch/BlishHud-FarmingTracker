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
        public Profits Profits { get; set; } = new Profits();
        public bool IsCoin => ApiId == Coin.COIN_CURRENCY_ID;

        public void SetProfits()
        {
            Profits.SetProfits(Count, Details.Flags);
        }
    }
}
