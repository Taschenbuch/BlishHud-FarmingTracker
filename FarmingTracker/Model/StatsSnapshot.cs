using System.Collections.Generic;

namespace FarmingTracker
{
    /// <summary>
    /// read comment of Stats class
    /// </summary>
    public class StatsSnapshot
    {
        public IReadOnlyDictionary<int, Stat> StatById { get; set; } = new Dictionary<int, Stat>();
    }
}
