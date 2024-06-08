using System.Collections.Generic;

namespace FarmingTracker
{
    public class Model
    {
        public FarmingDuration FarmingDuration { get; } = new FarmingDuration();
        public Dictionary<int, Stat> ItemById { get; } = new Dictionary<int, Stat>();
        public Dictionary<int, Stat> CurrencyById { get; } = new Dictionary<int, Stat>();
        public List<int> IgnoredItemApiIds { get; set; } = new List<int>();
    }
}
