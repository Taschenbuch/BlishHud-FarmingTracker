using Newtonsoft.Json;

namespace FarmingTracker
{
    // WARNING: before modifying, see FileModel comment. 
    // maybe it was a bad idea to not intro duce FileCustomStatProfit.
    public class CustomStatProfit
    {
        public CustomStatProfit(int apiId, StatType statType)
        {
            ApiId = apiId;
            StatType = statType;
        }

        [JsonProperty("ApiId")]
        public int ApiId { get; }
        
        [JsonProperty("StatType")]
        public StatType StatType { get; }
        
        [JsonProperty("Unsigned_CustomProfitInCopper")]
        public long Unsigned_CustomProfitInCopper { get; set; }
        
        public bool BelongsToStat(Stat stat)
        {
            return ApiId == stat.ApiId && StatType == stat.StatType;
        }
    }
}
