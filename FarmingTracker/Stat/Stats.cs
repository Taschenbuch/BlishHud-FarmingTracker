using System.Collections.Generic;

namespace FarmingTracker
{
    public class Stats
    {
        public Dictionary<int, Stat> ItemById { get; } = new Dictionary<int, Stat>();
        public Dictionary<int, Stat> CurrencyById { get; } = new Dictionary<int, Stat>();
    }
}
