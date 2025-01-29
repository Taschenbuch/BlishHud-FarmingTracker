using System.Threading;

namespace FarmingTracker
{
    public class Profit
    {
        // todo x rename: -> Unsigned_TpBuy_Profit...   und Unsigned_Max_TpAndVendor_ProfitInCopper
        public bool CanNotBeSold { get; set; } = true; // default true, because currencies do not set this
        public bool CanBeSoldOnTp { get; set; } = false;
        public bool CanBeSoldToVendor { get; set; } = false;
        public bool HasCustomProfit => Unsigned_CustomProfitInCopper.HasValue;
        public long Unsigned_MaxProfitInCopper => Unsigned_CustomProfitInCopper ?? Unsigned_MaxTpAndVendorProfitInCopper.Value;
        public long? Unsigned_CustomProfitInCopper
        {
            get
            {
                lock(_lock) // cannot use interlocked or ThreadSafeLong for nullables
                    return _unsigned_CustomProfitInCopper;
            }
            set
            {
                lock (_lock)
                    _unsigned_CustomProfitInCopper = value;
            }
        }
        public ThreadSafeLong Unsigned_MaxTpAndVendorProfitInCopper { get; } = new ThreadSafeLong(); // does not include CustomProfit, because that can be modified anytime by the user.
        public ThreadSafeLong Unsigned_MaxTpProfitInCopper { get; } = new ThreadSafeLong();
        public ThreadSafeLong Unsigned_TpSellProfitInCopper { get; } = new ThreadSafeLong(); // sell by listing a sell order
        public ThreadSafeLong Unsigned_TpBuyProfitInCopper { get; } = new ThreadSafeLong(); // sell by instant sell to buy order
        public ThreadSafeLong Unsigned_VendorProfitInCopper { get; } = new ThreadSafeLong(); // sell to npc vendor
        private long? _unsigned_CustomProfitInCopper;
        private readonly object _lock = new object();
    }
}
