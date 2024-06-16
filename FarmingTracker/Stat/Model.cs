using System.Collections.Generic;

namespace FarmingTracker
{
    public class Model
    {
        public FarmingDuration FarmingDuration { get; } = new FarmingDuration();
        public Dictionary<int, Stat> ItemById { get; set; } = new Dictionary<int, Stat>(); // todo x rename
        public Dictionary<int, Stat> ItemByIdA { get; } = new Dictionary<int, Stat>(); // todo x rename
        public SafeList<int> IgnoredItemApiIds { get; set; } = new SafeList<int>();
    }
}
