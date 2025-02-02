using System.Collections.Generic;
using System.Linq;

namespace FarmingTracker
{
    public class Stats
    {
        public Stat GetStat(int apiId, StatType statType)
        {
            var key = CreateKey(apiId, statType);

            lock (_statsLock)
                return _statById[key];
        }

        public List<Stat> GetStats()
        {
            lock (_statsLock)
                return _statById.Values.ToList();
        }

        public void ResetCounts()
        {
            foreach (var stat in GetStats())
                stat.Signed_Count.Value = 0;
        }

        public void AddStat(Stat stat)
        {
            var key = CreateKey(stat);

            lock (_statsLock)
            {
                if(_statById.ContainsKey(key))
                {
                    Module.Logger.Error("Cannot add stat to model because a stat with that key already exists");
                    return;
                }

                _statById[key] = stat;
            }
        }

        public void UpdateCountOrAddNewStat(Stat newStat)
        {
            var key = CreateKey(newStat);

            // the lock could use a smaller scope, because only this method adds stats after startup is finished and no method removes stats. But future updates may change that.
            lock (_statsLock)
            {
                if (_statById.TryGetValue(key, out var stat))
                    stat.Signed_Count.Add(newStat.Signed_Count);
                else
                    _statById[key] = newStat;
            }
        }

        private static int CreateKey(Stat stat)
        {
            return CreateKey(stat.ApiId, stat.StatType);
        }

        private static int CreateKey(int apiId, StatType statType)
        {
            return statType == StatType.Currency
                ? -apiId
                : apiId;
        }

        private readonly Dictionary<int, Stat> _statById = new Dictionary<int, Stat>();
        private readonly object _statsLock = new object();
    }
}
