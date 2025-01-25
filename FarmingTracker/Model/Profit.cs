namespace FarmingTracker
{
    public class Profit
    {
        // todo x rename: -> Unsigned_TpBuy_Profit...   und Unsigned_Max_TpAndVendor_ProfitInCopper
        public bool CanNotBeSold { get; set; } = true; // default true, because currencies do not set this
        public bool CanBeSoldOnTp { get; set; } = false;
        public bool CanBeSoldToVendor { get; set; } = false;
        public bool HasCustomProfit => Unsigned_CustomProfitInCopper.HasValue;
        public long Unsigned_MaxProfitInCopper => Unsigned_CustomProfitInCopper ?? Unsigned_MaxTpAndVendorProfitInCopper;
        public long? Unsigned_CustomProfitInCopper { get; set; } = null; // set by user
        public long Unsigned_MaxTpAndVendorProfitInCopper { get; set; } // does not include CustomProfit, because that can be modified anytime by the user.
        public long Unsigned_MaxTpProfitInCopper { get; set; }
        public long Unsigned_TpSellProfitInCopper { get; set; } // sell by listing a sell order
        public long Unsigned_TpBuyProfitInCopper { get; set; } // sell by instant sell to buy order
        public long Unsigned_VendorProfitInCopper { get; set; } // sell to npc vendor
    }
}
