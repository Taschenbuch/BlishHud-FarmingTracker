using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace FarmingTracker.DrfWebsocket
{
    public class DrfMessage
    {
        public DrfPayload Payload { get; set; } = new DrfPayload();
    }

    public class DrfPayload
    {
        public string Character { get; set; } = string.Empty;
        public DrfDrop Drop { get; set; } = new DrfDrop();
    }

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
