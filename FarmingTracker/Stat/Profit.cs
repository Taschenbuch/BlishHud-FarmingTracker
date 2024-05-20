using System;

namespace FarmingTracker
{
    public class Profit
    {
        public int MaxProfitInCopper { get; private set; }
        public int MaxTpProfitInCopper { get; private set; }
        public int TpSellProfitInCopper { get; private set; } // sell by listing a sell order
        public int TpBuyProfitInCopper { get; private set; } // sell by instant sell to buy order
        public int VendorProfitInCopper { get; private set; } // sell to npc vendor
        public bool CanNotBeSold => !CanBeSoldToVendor && !CanBeSoldOnTp;
        public bool CanBeSoldToVendor { get; set; }
        public bool CanBeSoldOnTp => TpSellProfitInCopper > 0 || TpBuyProfitInCopper > 0;

        public void SetVendorProfit(int vendorProfitInCopper)
        {
            VendorProfitInCopper = Math.Abs(vendorProfitInCopper);
            SetMaxProfit();
        }

        public void SetTpSellAndBuyProfit(int tpSellPriceInCopper, int tpBuyPriceInCopper)
        {
            // 85/100 is -15% tp fee with integer rounding
            TpSellProfitInCopper = Math.Abs(tpSellPriceInCopper) * 85 / 100;
            TpBuyProfitInCopper = Math.Abs(tpBuyPriceInCopper) * 85 / 100;
            SetMaxProfit();
        }

        private void SetMaxProfit()
        {
            var maxTpProfitInCopper = Math.Max(TpSellProfitInCopper, TpBuyProfitInCopper);  // because their could be only buy orders and no sell orders or the other way around.
            MaxTpProfitInCopper = maxTpProfitInCopper;
            MaxProfitInCopper = Math.Max(VendorProfitInCopper, maxTpProfitInCopper);
        }
    }
}
