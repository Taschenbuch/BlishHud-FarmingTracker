using System;

namespace FarmingTracker
{
    public class Profit
    {
        public long MaxProfitInCopper { get; set; }
        public long MaxTpProfitInCopper { get; set; }
        public long TpSellProfitInCopper { get; set; } // sell by listing a sell order
        public long TpBuyProfitInCopper { get; set; } // sell by instant sell to buy order
        public long VendorProfitInCopper { get; set; } // sell to npc vendor
    }
}
