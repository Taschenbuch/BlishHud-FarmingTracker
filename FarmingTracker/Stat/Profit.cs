using System;

namespace FarmingTracker
{
    public class Profit
    {
        public long MaxProfitInCopper { get; private set; }
        public long MaxTpProfitInCopper { get; private set; }
        public long TpSellProfitInCopper { get; private set; } // sell by listing a sell order
        public long TpBuyProfitInCopper { get; private set; } // sell by instant sell to buy order
        public long VendorProfitInCopper { get; private set; } // sell to npc vendor

        public void SetProfits(long vendorProfitInCopper, long tpSellProfitInCopper, long tpBuyProfitInCopper)
        {
            VendorProfitInCopper = Math.Abs(vendorProfitInCopper);
            TpSellProfitInCopper = Math.Abs(tpSellProfitInCopper);
            TpBuyProfitInCopper = Math.Abs(tpBuyProfitInCopper);
     
            var maxTpProfitInCopper = Math.Max(TpSellProfitInCopper, TpBuyProfitInCopper);  // because their could be only buy orders and no sell orders or the other way around.
            MaxTpProfitInCopper = maxTpProfitInCopper;
            MaxProfitInCopper = Math.Max(VendorProfitInCopper, maxTpProfitInCopper);
        }
    }
}
