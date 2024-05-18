using System;

namespace FarmingTracker
{
    public class Profit
    {
        public int MaxProfitInCopper { get; set; }
        public bool CanNotBeSold => !CanBeSoldToVendor && !CanBeSoldOnTradingPost;
        public bool CanBeSoldToVendor { get; set; }
        public bool CanBeSoldOnTradingPost => SellByListingInTradingPostProfitInCopper > 0 || SellToBuyOrderInTradingPostProfitInCopper > 0;
        public int SellByListingInTradingPostProfitInCopper => _sellByListingInTradingPostProfitInCopper;
        public int SellToBuyOrderInTradingPostProfitInCopper => _sellToBuyOrderInTradingPostProfitInCopper;
        
        public int SellToVendorProfitInCopper
        {
            get => _sellToVendorProfitInCopper;
            set
            {
                _sellToVendorProfitInCopper = value;
                SetMaxProfit();
            }
        }

        public void SetTpSellAndBuyProfit(int tpSellPriceInCopper, int tpBuyPriceInCopper)
        {
            // 85/100 is -15% tp fee with integer rounding
            _sellByListingInTradingPostProfitInCopper = tpSellPriceInCopper * 85 / 100;
            _sellToBuyOrderInTradingPostProfitInCopper = tpBuyPriceInCopper * 85 / 100; 
            SetMaxProfit();
        }

        private void SetMaxProfit()
        {
            var maxProfitInCopper = Math.Max(_sellToBuyOrderInTradingPostProfitInCopper, _sellByListingInTradingPostProfitInCopper); // because their could be only buy orders and no sell orders.
            maxProfitInCopper = Math.Max(maxProfitInCopper, _sellToVendorProfitInCopper);
            MaxProfitInCopper = maxProfitInCopper;
        }

        private int _sellByListingInTradingPostProfitInCopper;
        private int _sellToBuyOrderInTradingPostProfitInCopper;
        private int _sellToVendorProfitInCopper;
    }
}
