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
            
            var unsigned_vendor_ProfitInCopper = canBeSoldToVendor
                ? details.Unsigned_VendorValueInCopper
                : 0; // it sometimes has a VendorValue even when it cannot be sold to vendor. That would distort the profit.

            // 85/100 is -15% tp fee with integer rounding
            var unsigned_tpSell_ProfitInCopper = details.Unsigned_SellsUnitPriceInCopper * 85 / 100;
            var unsigned_tpBuy_ProfitInCopper = details.Unsigned_BuysUnitPriceInCopper * 85 / 100;
            
            var canBeSoldOnTp = unsigned_tpSell_ProfitInCopper > 0 || unsigned_tpBuy_ProfitInCopper > 0;
            
            var unsigned_maxTp_ProfitInCopper = Math.Max(unsigned_tpSell_ProfitInCopper, unsigned_tpBuy_ProfitInCopper);  // because their could be only buy orders and no sell orders or the other way around.
            var unsigned_maxTpAndVendor_ProfitInCopper = Math.Max(unsigned_vendor_ProfitInCopper, unsigned_maxTp_ProfitInCopper);

            profit.CanBeSoldOnTp = canBeSoldOnTp;
            profit.CanBeSoldToVendor = canBeSoldToVendor;
            profit.CanNotBeSold = !canBeSoldToVendor && !canBeSoldOnTp;
            profit.Unsigned_Vendor_ProfitInCopper.Value = unsigned_vendor_ProfitInCopper;
            profit.Unsigned_TpSell_ProfitInCopper.Value = unsigned_tpSell_ProfitInCopper;
            profit.Unsigned_TpBuy_ProfitInCopper.Value = unsigned_tpBuy_ProfitInCopper;
            profit.Unsigned_MaxTp_ProfitInCopper.Value = unsigned_maxTp_ProfitInCopper;
            profit.Unsigned_MaxTpAndVendor_ProfitInCopper.Value = unsigned_maxTpAndVendor_ProfitInCopper;
        }
    }
}
