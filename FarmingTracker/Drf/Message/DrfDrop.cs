using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace FarmingTracker
{
    public class DrfDrop
    {
        [JsonProperty("items")]
        public Dictionary<int, int> Items { get; set; } = new Dictionary<int, int>();
        [JsonProperty("curr")]
        public Dictionary<int, int> Currencies { get; set; } = new Dictionary<int, int>();
        [JsonProperty("mf")]
        public int MagicFind{ get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
