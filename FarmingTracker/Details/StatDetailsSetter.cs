using Blish_HUD.Modules.Managers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FarmingTracker
{
    public class StatDetailsSetter
    {
        public async Task SetDetailsAndProfitFromApi(Stats stats, Gw2ApiManager gw2ApiManager)
        {
            var hasToInitializeCache = !_currencyDetailsByIdCache.Any();
            if (hasToInitializeCache)
                _currencyDetailsByIdCache = await CurrencyDetailsSetter.CreateCacheWithAllApiCurrencies(gw2ApiManager);

            CurrencyDetailsSetter.SetCurrencyDetailsFromCache(stats.CurrencyById, _currencyDetailsByIdCache);
            await ItemDetailsSetter.SetItemDetailsFromApi(stats.ItemById, gw2ApiManager);
            SetProfits(stats.ItemById);
        }

        private void SetProfits(Dictionary<int, Stat> itemById)
        {
            foreach (var item in itemById.Values)
                item.SetProfits();
        }

        private Dictionary<int, CurrencyDetails> _currencyDetailsByIdCache = new Dictionary<int, CurrencyDetails>();
    }
}
