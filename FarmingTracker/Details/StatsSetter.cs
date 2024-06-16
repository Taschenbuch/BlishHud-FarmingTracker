using Blish_HUD.Modules.Managers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FarmingTracker
{
    public class StatsSetter
    {
        public async Task SetDetailsAndProfitFromApi(
            ConcurrentDictionary<int, Stat> itemById, 
            ConcurrentDictionary<int, Stat> currencyById, 
            Gw2ApiManager gw2ApiManager)
        {
            if (HasToInitializeCache())
                _currencyDetailsByIdCache = await CurrencyDetailsSetter.CreateCacheWithAllApiCurrencies(gw2ApiManager);

            CurrencyDetailsSetter.SetCurrencyDetailsFromCache(currencyById, _currencyDetailsByIdCache);
            await ItemDetailsSetter.SetItemDetailsFromApi(itemById, gw2ApiManager);
            StatProfitSetter.SetProfits(itemById);
        }

        private bool HasToInitializeCache()
        {
            return !_currencyDetailsByIdCache.Any();
        }

        private Dictionary<int, CurrencyDetails> _currencyDetailsByIdCache = new Dictionary<int, CurrencyDetails>();
    }
}
