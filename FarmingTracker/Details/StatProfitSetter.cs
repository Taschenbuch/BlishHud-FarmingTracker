using System.Linq;
using System;
using Gw2Sharp.WebApi.V2.Models;
using System.Collections.Generic;

namespace FarmingTracker
{
    public class StatProfitSetter
    {
        public static void SetProfits(Dictionary<int, Stat> statById)
        {
            foreach (var stat in statById.Values)
                SetProfit(stat.Profit, stat.Signed_Count, stat.Details);
        }

        private static void SetProfit(Profit profit, long signed_count, ApiStatDetails details)
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
            profit.Unsigned_VendorProfitInCopper = unsigned_vendorProfitInCopper;
            profit.Unsigned_TpSellProfitInCopper = unsigned_tpSellProfitInCopper;
            profit.Unsigned_TpBuyProfitInCopper = unsigned_tpBuyProfitInCopper;
            profit.Unsigned_MaxTpProfitInCopper = unsigned_maxTpProfitInCopper;
            profit.Unsigned_MaxTpAndVendorProfitInCopper = unsigned_maxProfitInCopper;
        }
    }
}
