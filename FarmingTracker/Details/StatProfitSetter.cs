using System.Linq;
using System;
using Gw2Sharp.WebApi.V2.Models;
using System.Collections.Generic;

namespace FarmingTracker
{
    public class StatProfitSetter
    {
        public static void SetProfits(Dictionary<int, Stat> itemById)
        {
            foreach (var item in itemById.Values)
                item.Profits = CreateProfits(item.Signed_Count, item.Details);
        }

        private static Profits CreateProfits(long signed_count, ApiStatDetails details)
        {
            // 85/100 is -15% tp fee with integer rounding
            var unsigned_tpSellProfitInCopper = details.Unsigned_SellsUnitPriceInCopper * 85 / 100;
            var unsigned_tpBuyProfitInCopper = details.Unsigned_BuysUnitPriceInCopper * 85 / 100;
            var canBeSoldOnTp = unsigned_tpSellProfitInCopper > 0 || unsigned_tpBuyProfitInCopper > 0;

            var canBeSoldToVendor = details.Unsigned_VendorValueInCopper != 0 && !details.ItemFlags.Any(f => f == ItemFlag.NoSell);
            var unsigned_vendorProfitInCopper = canBeSoldToVendor
                ? details.Unsigned_VendorValueInCopper
                : 0; // it sometimes has a VendorValue even when it cannot be sold to vendor. That would distort the profit.

            return new Profits
            {
                Each = CreateProfit(unsigned_vendorProfitInCopper, unsigned_tpSellProfitInCopper, unsigned_tpBuyProfitInCopper),
                All = CreateProfit(signed_count * unsigned_vendorProfitInCopper, signed_count * unsigned_tpSellProfitInCopper, signed_count * unsigned_tpBuyProfitInCopper),
                CanBeSoldOnTp = canBeSoldOnTp,
                CanBeSoldToVendor = canBeSoldToVendor,
                CanNotBeSold = !canBeSoldToVendor && !canBeSoldOnTp
            };
        }

        private static Profit CreateProfit(long unsigned_vendorProfitInCopper, long unsigned_tpSellProfitInCopper, long unsigned_tpBuyProfitInCopper)
        {
            unsigned_vendorProfitInCopper = Math.Abs(unsigned_vendorProfitInCopper);
            unsigned_tpSellProfitInCopper = Math.Abs(unsigned_tpSellProfitInCopper);
            unsigned_tpBuyProfitInCopper = Math.Abs(unsigned_tpBuyProfitInCopper);

            var unsigned_maxTpProfitInCopper = Math.Max(unsigned_tpSellProfitInCopper, unsigned_tpBuyProfitInCopper);  // because their could be only buy orders and no sell orders or the other way around.
            var unsigned_maxProfitInCopper = Math.Max(unsigned_vendorProfitInCopper, unsigned_maxTpProfitInCopper);

            return new Profit
            {
                Unsigned_VendorProfitInCopper = unsigned_vendorProfitInCopper,
                Unsigned_TpSellProfitInCopper = unsigned_tpSellProfitInCopper,
                Unsigned_TpBuyProfitInCopper = unsigned_tpBuyProfitInCopper,
                Unsigned_MaxTpProfitInCopper = unsigned_maxTpProfitInCopper,
                Unsigned_MaxProfitInCopper = unsigned_maxProfitInCopper
            };
        }
    }
}
