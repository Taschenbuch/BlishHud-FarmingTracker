using Gw2Sharp.WebApi.V2;
using Gw2Sharp.WebApi.V2.Models;
using System.Collections.Generic;

namespace FarmingTracker
{
    public class CurrencySearcher
    {
        public static IEnumerable<ItemX> GetCurrencyIdsAndCounts(IApiV2ObjectList<AccountCurrency> apiWallet)
        {
            foreach (var apiCurrency in apiWallet)
                yield return new ItemX
                {
                    ApiId = apiCurrency.Id,
                    Count = apiCurrency.Value,
                    ApiIdType = ApiIdType.Currency,
                };
        }
    }
}
