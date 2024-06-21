using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace FarmingTracker
{
    public class Model
    {
        public FarmingDuration FarmingDuration { get; } = new FarmingDuration();
        public SafeList<int> IgnoredItemApiIds { get; set; } = new SafeList<int>();
        public SafeList<int> FavoriteItemApiIds { get; set; } = new SafeList<int>();
        public Dictionary<int, Stat> CurrencyById { get; } = new Dictionary<int, Stat>(); 
        public Dictionary<int, Stat> ItemById { get; } = new Dictionary<int, Stat>();
        public StatsSnapshot StatsSnapshot { get; set; } = new StatsSnapshot();
        
        public void UpdateStatsSnapshot()
        {
            var newSnapshot = new StatsSnapshot
            {
                ItemById = ItemById,
                CurrencyById = CurrencyById
            };

            StatsSnapshot = JsonConvert.DeserializeObject<StatsSnapshot>(JsonConvert.SerializeObject(newSnapshot));
        }
    }
}
