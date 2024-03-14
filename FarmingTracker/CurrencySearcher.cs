using Gw2Sharp.WebApi.V2;
using Gw2Sharp.WebApi.V2.Models;
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
    }
}
