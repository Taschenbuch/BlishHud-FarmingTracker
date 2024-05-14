using System;

namespace FarmingTracker
{
    public class Profit
    {
        public int MaxProfitInCopper { get; set; }

        public int SellToVendorInCopper
        {
            get => _sellToVendorInCopper;
            set
            {
                _sellToVendorInCopper = value;
                SetMaxProfit();
            }
        }
        public int SellByListingInTradingPostInCopper // includes listing and exchange fee (15%)
        {
            get => _sellByListingInTradingPostInCopper;
            set
            {
                _sellByListingInTradingPostInCopper = value * 85 / 100; // 85/100 is -15% tp fee with integer rounding
                SetMaxProfit();
            }
        }

        public void SetMaxProfit()
        {
            MaxProfitInCopper = Math.Max(_sellToVendorInCopper, _sellByListingInTradingPostInCopper);
        }

        private int _sellByListingInTradingPostInCopper;
        private int _sellToVendorInCopper;
    }
}
