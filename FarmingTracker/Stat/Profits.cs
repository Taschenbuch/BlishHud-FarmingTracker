using Gw2Sharp.WebApi.V2.Models;
using System.Linq;

namespace FarmingTracker
{
    public class Profits
    {
        public bool CanNotBeSold { get; private set; }
        public bool CanBeSoldOnTp { get; private set; }
        public bool CanBeSoldToVendor { get; private set; }
        public Profit Each { get; set; } = new Profit();
        public Profit All { get; set; } = new Profit();
        public long ApiVendorValueInCopper { get; set; }
        public long ApiSellsUnitPriceInCopper { get; set; }
        public long ApiBuysUnitPriceInCopper { get; set; }

        public void SetProfits(long count, ApiFlags<ItemFlag> itemFlags)
        {
            // 85/100 is -15% tp fee with integer rounding
            var tpSellProfitInCopper = ApiSellsUnitPriceInCopper * 85 / 100;
            var tpBuyProfitInCopper = ApiBuysUnitPriceInCopper * 85 / 100;
            var canBeSoldOnTp = tpSellProfitInCopper > 0 || tpBuyProfitInCopper > 0;
            
            var canBeSoldToVendor = ApiVendorValueInCopper != 0 && !itemFlags.Any(f => f == ItemFlag.NoSell);
            var vendorProfitInCopper = canBeSoldToVendor
                ? ApiVendorValueInCopper
                : 0; // it sometimes has a VendorValue even when it cannot be sold to vendor. That would distort the profit.

            Each.SetProfits(vendorProfitInCopper, tpSellProfitInCopper, tpBuyProfitInCopper);
            All.SetProfits(count * vendorProfitInCopper, count * tpSellProfitInCopper, count * tpBuyProfitInCopper);

            CanBeSoldOnTp = canBeSoldOnTp;
            CanBeSoldToVendor = canBeSoldToVendor;
            CanNotBeSold = !canBeSoldToVendor && !canBeSoldOnTp;
        }
    }
}
