using System.Collections.Generic;

namespace FarmingTracker
{
    /// <summary>
    /// To prevent that itemById and currencyById are out of sync, a local variable of StatsSnapshot has to be created first, when trying to access both.
    /// This data is for reading. It must not be written to. Writing is done by replacing the whole snapshot instance.
    /// </summary>
    public class StatsSnapshot
    {
        public IReadOnlyDictionary<int, Stat> ItemById { get; set; } = new Dictionary<int, Stat>();
        public IReadOnlyDictionary<int, Stat> CurrencyById { get; set; } = new Dictionary<int, Stat>();
    }
}
