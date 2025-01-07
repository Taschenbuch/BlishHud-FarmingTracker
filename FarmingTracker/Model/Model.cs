namespace FarmingTracker
{
    public class Model
    {
        public Stats Stats { get; set; } = new Stats();
        public SafeList<int> IgnoredItemApiIds { get; set; } = new SafeList<int>();
        public SafeList<FavoriteStat> FavoriteStats { get; set; } = new SafeList<FavoriteStat>();
        public SafeList<CustomStatProfit> CustomStatProfits { get; set; } = new SafeList<CustomStatProfit>();
    }
}
