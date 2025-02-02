namespace FarmingTracker
{
    public class Profit
    {
        public bool CanNotBeSold { get; set; } = true; // default true, because currencies do not set this
        public bool CanBeSoldOnTp { get; set; } = false;
        public bool CanBeSoldToVendor { get; set; } = false;
        public bool HasCustomProfit => Unsigned_Custom_ProfitInCopper.HasValue;
        public long Unsigned_Max_ProfitInCopper => Unsigned_Custom_ProfitInCopper ?? Unsigned_MaxTpAndVendor_ProfitInCopper.Value;
        public long? Unsigned_Custom_ProfitInCopper
        {
            get
            {
                lock(_lock) // cannot use interlocked or ThreadSafeLong for nullables
                    return _unsigned_Custom_ProfitInCopper;
            }
            set
            {
                lock (_lock)
                    _unsigned_Custom_ProfitInCopper = value;
            }
        }
        public ThreadSafeLong Unsigned_MaxTpAndVendor_ProfitInCopper { get; } = new ThreadSafeLong(); // does not include CustomProfit, because that can be modified anytime by the user.
        public ThreadSafeLong Unsigned_MaxTp_ProfitInCopper { get; } = new ThreadSafeLong();
        public ThreadSafeLong Unsigned_TpSell_ProfitInCopper { get; } = new ThreadSafeLong(); // sell by listing a sell order
        public ThreadSafeLong Unsigned_TpBuy_ProfitInCopper { get; } = new ThreadSafeLong(); // sell by instant sell to buy order
        public ThreadSafeLong Unsigned_Vendor_ProfitInCopper { get; } = new ThreadSafeLong(); // sell to npc vendor
        
        private long? _unsigned_Custom_ProfitInCopper;
        private readonly object _lock = new object();
    }
}
