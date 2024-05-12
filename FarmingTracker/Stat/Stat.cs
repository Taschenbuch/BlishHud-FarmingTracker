namespace FarmingTracker
{
    // can be currency or item
    public class Stat
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Tooltip { get; set; } = string.Empty;
        public Profit Profit { get; set; } = new Profit();
        public int Count { get; set; }
        public int ApiId { get; set; }
        public int IconAssetId { get; set; } = Constants.BUG_TEXTURE_ASSET_ID; // prevents that FromAssetId() returns null and crashes new Image()
        public StatDetailsState DetailsState { get; set; } = StatDetailsState.MissingBecauseApiNotCalledYet;
        public bool IsCoin => ApiId == Coin.COIN_CURRENCY_ID;
    }
}
