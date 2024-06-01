namespace FarmingTracker
{
    public class DrfMessage
    {
        public string Kind { get; set; } = string.Empty;
        public DrfPayload Payload { get; set; } = new DrfPayload();
    }
}
