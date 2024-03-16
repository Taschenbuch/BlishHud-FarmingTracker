using Gw2Sharp.WebApi.V2;
using Gw2Sharp.WebApi.V2.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FarmingTracker
{
    public class CurrencySearcher
    {
        public static IEnumerable<ItemX> GetCurrencyIdsAndCounts(IApiV2ObjectList<AccountCurrency> apiWallet)
        {
            foreach (var apiCurrency in apiWallet.Where(c => c.Value != 0))
                yield return new ItemX
                {
                    ApiId = apiCurrency.Id,
                    Count = apiCurrency.Value,
                    ApiIdType = ApiIdType.Currency,
                };
        }

        public static void ReplaceCoinItemWithGoldSilverCopperItems(List<ItemX> farmedCurrencies)
        {
            var coinsInCopperItem = farmedCurrencies.SingleOrDefault(c => c.IconAssetId == 619316);
            if (coinsInCopperItem != null)
            {
                var coin = new Coin(coinsInCopperItem.Count);

                var goldItem = new ItemX
                {
                    Name = "Gold",
                    Count = coin.Gold,
                    IconAssetId = 156904,
                    ApiId = coinsInCopperItem.ApiId, // the coin apiid is not unique anymore because of gold, silver, copper
                    ApiIdType = ApiIdType.Currency,
                };

                var silverItem = new ItemX
                {
                    Name = "Silver",
                    Count = coin.Silver,
                    IconAssetId = 156907,
                    ApiId = coinsInCopperItem.ApiId, // the coin apiid is not unique anymore because of gold, silver, copper
                    ApiIdType = ApiIdType.Currency,
                };

                var copperItem = new ItemX
                {
                    Name = "Copper",
                    Count = coin.Copper,
                    IconAssetId = 156902,
                    ApiId = coinsInCopperItem.ApiId, // the coin apiid is not unique anymore because of gold, silver, copper
                    ApiIdType = ApiIdType.Currency,
                };

                farmedCurrencies.Remove(coinsInCopperItem);
                farmedCurrencies.Insert(0, copperItem);
                farmedCurrencies.Insert(0, silverItem);
                farmedCurrencies.Insert(0, goldItem);
            }
        }
    }
}
