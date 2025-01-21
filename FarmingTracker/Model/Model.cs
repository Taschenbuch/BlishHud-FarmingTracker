namespace FarmingTracker
{
    public class Model
    {
        public Stats Stats { get; set; } = new Stats();
        public SafeList<CustomStatProfit> CustomStatProfits { get; set; } = new SafeList<CustomStatProfit>();
    }
}
