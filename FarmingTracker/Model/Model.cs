namespace FarmingTracker
{
    public class Model
    {
        public Stats Stats { get; set; } = new Stats();
        public SafeList<FavoriteStat> IgnoredStats { get; set; } = new SafeList<FavoriteStat>();
        public SafeList<FavoriteStat> FavoriteStats { get; set; } = new SafeList<FavoriteStat>();
        public SafeList<CustomStatProfit> CustomStatProfits { get; set; } = new SafeList<CustomStatProfit>();
    }
}
