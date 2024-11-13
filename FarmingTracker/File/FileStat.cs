using Newtonsoft.Json;

namespace FarmingTracker
{
    // WARNING: before modifying, see FileModel comment
    public class FileStat
    {
        [JsonProperty("ApiId")]
        public int ApiId { get; set; }

        [JsonProperty("Count")]
        public long Count { get; set; }
    }
}
