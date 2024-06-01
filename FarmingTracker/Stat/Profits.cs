namespace FarmingTracker
{
    public class Profits
    {
        public bool CanNotBeSold { get; set; } = true; // default true, because currencies do not set this
        public bool CanBeSoldOnTp { get; set; } = true;
        public bool CanBeSoldToVendor { get; set; } = true;
        public Profit Each { get; set; } = new Profit();
        public Profit All { get; set; } = new Profit();
    }
}
