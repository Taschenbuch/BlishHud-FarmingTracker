namespace FarmingTracker
{
    public class Profit
    {
        public long Unsigned_MaxProfitInCopper { get; set; }
        public long Unsigned_MaxTpProfitInCopper { get; set; }
        public long Unsigned_TpSellProfitInCopper { get; set; } // sell by listing a sell order
        public long Unsigned_TpBuyProfitInCopper { get; set; } // sell by instant sell to buy order
        public long Unsigned_VendorProfitInCopper { get; set; } // sell to npc vendor
    }
}
