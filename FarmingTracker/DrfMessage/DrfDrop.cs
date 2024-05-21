using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace FarmingTracker
{
    public class DrfDrop
    {
        [JsonProperty("items")]
        public Dictionary<int, long> Items { get; set; } = new Dictionary<int, long>();
        [JsonProperty("curr")]
        public Dictionary<int, long> Currencies { get; set; } = new Dictionary<int, long>();
        [JsonProperty("mf")]
        public int MagicFind{ get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
