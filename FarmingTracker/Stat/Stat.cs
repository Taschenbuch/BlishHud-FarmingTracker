namespace FarmingTracker
{
    // can be currency or item
    public class Stat
    {
        public int ApiId { get; set; }
        public int Count { get; set; }
        public ApiStatDetails Details { get; set; } = new ApiStatDetails();
        public Profit Profit { get; set; } = new Profit();
        public string Tooltip { get; set; } = string.Empty;
        public bool IsCoin => ApiId == Coin.COIN_CURRENCY_ID;
    }
}
