using Blish_HUD.Modules.Managers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FarmingTracker
{
    public class StatDetailsSetter
    {
        public async Task SetDetailsAndProfitFromApi(Model model, Gw2ApiManager gw2ApiManager)
        {
            if (HasToInitializeCache())
                _currencyDetailsByIdCache = await CurrencyDetailsSetter.CreateCacheWithAllApiCurrencies(gw2ApiManager);

            CurrencyDetailsSetter.SetCurrencyDetailsFromCache(model.CurrencyById, _currencyDetailsByIdCache);
            await ItemDetailsSetter.SetItemDetailsFromApi(model.ItemById, gw2ApiManager);
            StatProfitSetter.SetProfits(model.ItemById);
        }

        private bool HasToInitializeCache()
        {
            return !_currencyDetailsByIdCache.Any();
        }

        private Dictionary<int, CurrencyDetails> _currencyDetailsByIdCache = new Dictionary<int, CurrencyDetails>();
    }
}
