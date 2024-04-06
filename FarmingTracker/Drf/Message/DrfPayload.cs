namespace FarmingTracker
{
    public class DrfPayload
    {
        public string Character { get; set; } = string.Empty;
        public DrfDrop Drop { get; set; } = new DrfDrop();
    }
}
