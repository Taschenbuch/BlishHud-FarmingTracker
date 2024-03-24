using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FarmingTracker
{
    public static class Drf
    {
        // sometimes drf dll fails to get wallet snapshot after map change
        public static List<DrfMessage> RemoveInvalidMessages(List<DrfMessage> drfMessages)
        {
            return drfMessages.Where(m => m.Payload.Drop.Currencies.Count <= 10).ToList();
        }
    }

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
