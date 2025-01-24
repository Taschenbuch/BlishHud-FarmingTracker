using Blish_HUD.Modules.Managers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FarmingTracker
{
    public class StatsSetter
    {
        public async Task SetDetailsAndProfitFromApi(Dictionary<int, Stat> statById, Gw2ApiManager gw2ApiManager)
        {
            if (HasToInitializeCache())
                _currencyDetailsByIdCache = await CurrencyDetailsSetter.CreateCacheWithAllApiCurrencies(gw2ApiManager);

            CurrencyDetailsSetter.SetCurrencyDetailsFromCache(statById, _currencyDetailsByIdCache);
            await ItemDetailsSetter.SetItemDetailsFromApi(statById, gw2ApiManager);
            StatProfitSetter.SetProfits(statById);
        }

        private bool HasToInitializeCache()
        {
            return !_currencyDetailsByIdCache.Any();
        }

        private Dictionary<int, CurrencyDetails> _currencyDetailsByIdCache = new Dictionary<int, CurrencyDetails>();
    }
}
