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
                item.Profits = CreateProfits(item.Count, item.Details);
        }

        private static Profits CreateProfits(long count, ApiStatDetails details)
        {
            // 85/100 is -15% tp fee with integer rounding
            var tpSellProfitInCopper = details.SellsUnitPriceInCopper * 85 / 100;
            var tpBuyProfitInCopper = details.BuysUnitPriceInCopper * 85 / 100;
            var canBeSoldOnTp = tpSellProfitInCopper > 0 || tpBuyProfitInCopper > 0;

            var canBeSoldToVendor = details.VendorValueInCopper != 0 && !details.ItemFlags.Any(f => f == ItemFlag.NoSell);
            var vendorProfitInCopper = canBeSoldToVendor
                ? details.VendorValueInCopper
                : 0; // it sometimes has a VendorValue even when it cannot be sold to vendor. That would distort the profit.

            return new Profits
            {
                Each = CreateProfit(vendorProfitInCopper, tpSellProfitInCopper, tpBuyProfitInCopper),
                All = CreateProfit(count * vendorProfitInCopper, count * tpSellProfitInCopper, count * tpBuyProfitInCopper),
                CanBeSoldOnTp = canBeSoldOnTp,
                CanBeSoldToVendor = canBeSoldToVendor,
                CanNotBeSold = !canBeSoldToVendor && !canBeSoldOnTp
            };
        }

        private static Profit CreateProfit(long vendorProfitInCopper, long tpSellProfitInCopper, long tpBuyProfitInCopper)
        {
            vendorProfitInCopper = Math.Abs(vendorProfitInCopper);
            tpSellProfitInCopper = Math.Abs(tpSellProfitInCopper);
            tpBuyProfitInCopper = Math.Abs(tpBuyProfitInCopper);

            var maxTpProfitInCopper = Math.Max(tpSellProfitInCopper, tpBuyProfitInCopper);  // because their could be only buy orders and no sell orders or the other way around.
            var maxProfitInCopper = Math.Max(vendorProfitInCopper, maxTpProfitInCopper);

            return new Profit
            {
                VendorProfitInCopper = vendorProfitInCopper,
                TpSellProfitInCopper = tpSellProfitInCopper,
                TpBuyProfitInCopper = tpBuyProfitInCopper,
                MaxTpProfitInCopper = maxTpProfitInCopper,
                MaxProfitInCopper = maxProfitInCopper
            };
        }
    }
}
