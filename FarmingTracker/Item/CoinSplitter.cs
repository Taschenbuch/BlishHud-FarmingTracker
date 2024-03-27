using System.Collections.Generic;

namespace FarmingTracker
{
    public class CoinSplitter
    {
        public static void ReplaceCoinWithGoldSilverCopper(Dictionary<int, ItemX> currencyById)
        {
            // do NOT remove coin from currencyById!
            currencyById.Remove(GOLD_FAKE_ID);
            currencyById.Remove(SILVER_FAKE_ID);
            currencyById.Remove(COPPER_FAKE_ID);

            var hasEarnedOrLostCoin = currencyById.TryGetValue(Coin.COIN_CURRENCY_ID, out var coinsInCopperItem);
            if (!hasEarnedOrLostCoin)
                return;

            var coin = new Coin(coinsInCopperItem.Count);

            if(coin.HasToDisplayGold)
            {
                var goldItem = new ItemX
                {
                    Name = "Gold",
                    Count = coin.Gold,
                    IconAssetId = 156904,
                    ApiId = GOLD_FAKE_ID,
                };
                
                currencyById[goldItem.ApiId] = goldItem;
            }

            if(coin.HasToDisplaySilver) 
            {
                var silverItem = new ItemX
                {
                    Name = "Silver",
                    Count = coin.Silver,
                    IconAssetId = 156907,
                    ApiId = SILVER_FAKE_ID,
                };

                currencyById[silverItem.ApiId] = silverItem;
            }
            
            if(coin.HasToDisplayCopper) 
            {
                var copperItem = new ItemX
                {
                    Name = "Copper",
                    Count = coin.Copper,
                    IconAssetId = 156902,
                    ApiId = COPPER_FAKE_ID,
                };

                currencyById[copperItem.ApiId] = copperItem;
            }
        }

        private const int GOLD_FAKE_ID = -3;
        private const int SILVER_FAKE_ID = -2;
        private const int COPPER_FAKE_ID = -1;
    }
}
