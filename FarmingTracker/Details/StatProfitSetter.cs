using System.Linq;
using System;
using Gw2Sharp.WebApi.V2.Models;

namespace FarmingTracker
{
    public class StatProfitSetter
    {
        public static void SetProfits(Stats Stats)
        {
            foreach (var stat in Stats.GetStats())
                SetProfit(stat.Profit, stat.Details);
        }

        private static void SetProfit(Profit profit, StatApiDetails details)
        {
            var canBeSoldToVendor = details.Unsigned_VendorValueInCopper != 0 && !details.ItemFlags.Any(f => f == ItemFlag.NoSell);
            
            var unsigned_vendorProfitInCopper = canBeSoldToVendor
                ? details.Unsigned_VendorValueInCopper
                : 0; // it sometimes has a VendorValue even when it cannot be sold to vendor. That would distort the profit.

            // 85/100 is -15% tp fee with integer rounding
            var unsigned_tpSellProfitInCopper = details.Unsigned_SellsUnitPriceInCopper * 85 / 100;
            var unsigned_tpBuyProfitInCopper = details.Unsigned_BuysUnitPriceInCopper * 85 / 100;
            
            var canBeSoldOnTp = unsigned_tpSellProfitInCopper > 0 || unsigned_tpBuyProfitInCopper > 0;
            
            var unsigned_maxTpProfitInCopper = Math.Max(unsigned_tpSellProfitInCopper, unsigned_tpBuyProfitInCopper);  // because their could be only buy orders and no sell orders or the other way around.
            var unsigned_maxProfitInCopper = Math.Max(unsigned_vendorProfitInCopper, unsigned_maxTpProfitInCopper);

            profit.CanBeSoldOnTp = canBeSoldOnTp;
            profit.CanBeSoldToVendor = canBeSoldToVendor;
            profit.CanNotBeSold = !canBeSoldToVendor && !canBeSoldOnTp;
            profit.Unsigned_VendorProfitInCopper.Value = unsigned_vendorProfitInCopper;
            profit.Unsigned_TpSellProfitInCopper.Value = unsigned_tpSellProfitInCopper;
            profit.Unsigned_TpBuyProfitInCopper.Value = unsigned_tpBuyProfitInCopper;
            profit.Unsigned_MaxTpProfitInCopper.Value = unsigned_maxTpProfitInCopper;
            profit.Unsigned_MaxTpAndVendorProfitInCopper.Value = unsigned_maxProfitInCopper;
        }
    }
}
