using System.Collections.Generic;

namespace FarmingTracker
{
    public class CurrencySearcher
    {
        public static void ReplaceCoinItemWithGoldSilverCopperItems(Dictionary<int, ItemX> currencyById)
        {
            if (!currencyById.TryGetValue(COIN_CURRENCY_ID, out var coinsInCopperItem))
                return;

            var coin = new Coin(coinsInCopperItem.Count);

            var goldItem = new ItemX
            {
                Name = "Gold",
                Count = coin.Gold,
                IconAssetId = 156904,
                ApiId = -3,
                ApiIdType = ApiIdType.Currency,
            };

            var silverItem = new ItemX
            {
                Name = "Silver",
                Count = coin.Silver,
                IconAssetId = 156907,
                ApiId = -2,
                ApiIdType = ApiIdType.Currency,
            };

            var copperItem = new ItemX
            {
                Name = "Copper",
                Count = coin.Copper,
                IconAssetId = 156902,
                ApiId = -1,
                ApiIdType = ApiIdType.Currency,
            };
            currencyById.Remove(COIN_CURRENCY_ID);
            currencyById[goldItem.ApiId] = goldItem;
            currencyById[silverItem.ApiId] = silverItem;
            currencyById[copperItem.ApiId] = copperItem;
        }

        private const int COIN_CURRENCY_ID = 1;
    }
}
