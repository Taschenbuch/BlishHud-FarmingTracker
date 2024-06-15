using System.Collections.Concurrent;

namespace FarmingTracker
{
    public class Model
    {
        public FarmingDuration FarmingDuration { get; } = new FarmingDuration();
        public ConcurrentDictionary<int, Stat> ItemById { get; } = new ConcurrentDictionary<int, Stat>();
        public ConcurrentDictionary<int, Stat> CurrencyById { get; } = new ConcurrentDictionary<int, Stat>();
        public SafeList<int> IgnoredItemApiIds { get; set; } = new SafeList<int>();
    }
}
