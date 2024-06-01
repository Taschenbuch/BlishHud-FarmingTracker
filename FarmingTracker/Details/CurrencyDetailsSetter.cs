using Blish_HUD.Modules.Managers;
using Gw2Sharp.WebApi.V2.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace FarmingTracker
{
    public class CurrencyDetailsSetter
    {
        // Caching does work for currencies (<100). But for items it would need like 5 minutes (>60k).
        public static async Task<Dictionary<int, CurrencyDetails>> CreateCacheWithAllApiCurrencies(Gw2ApiManager gw2ApiManager)
        {
            IReadOnlyList<Currency> apiCurrencies = new List<Currency>();

            try
            {
                apiCurrencies = await gw2ApiManager.Gw2ApiClient.V2.Currencies.AllAsync();
            }
            catch (Exception e)
            {
                throw new Gw2ApiException($"API error: {nameof(CreateCacheWithAllApiCurrencies)}", e);
            }

            var currencyDetailsById = new Dictionary<int, CurrencyDetails>();

            foreach (var apiCurrency in apiCurrencies)
                currencyDetailsById[apiCurrency.Id] = new CurrencyDetails
                {
                    Name = apiCurrency.Name,
                    Description = apiCurrency.Description,
                    IconAssetId = TextureService.GetIconAssetId(apiCurrency.Icon),
                };

            return currencyDetailsById;
        }

        public static void SetCurrencyDetailsFromCache(Dictionary<int, Stat> currencyById, Dictionary<int, CurrencyDetails> currencyDetailsByIdCache)
        {
            var currenciesWithoutDetails = currencyById.Values.Where(c => c.Details.State == ApiStatDetailsState.MissingBecauseApiNotCalledYet).ToList();
            if (!currenciesWithoutDetails.Any())
                return;

            Module.Logger.Debug("currencies no details " + string.Join(" ", currenciesWithoutDetails.Select(c => c.ApiId)));

            var missingInApiCurrencyIds = new List<int>();

            foreach (var currencyWithoutDetails in currenciesWithoutDetails)
            {
                if (currencyDetailsByIdCache.TryGetValue(currencyWithoutDetails.ApiId, out var currencyDetails))
                {
                    currencyWithoutDetails.Details.Name = currencyDetails.Name;
                    currencyWithoutDetails.Details.Description = currencyDetails.Description;
                    currencyWithoutDetails.Details.IconAssetId = currencyDetails.IconAssetId;
                    currencyWithoutDetails.Details.WikiSearchTerm = currencyDetails.Name;
                    currencyWithoutDetails.Details.State = ApiStatDetailsState.SetByApi;
                }
                else
                {
                    missingInApiCurrencyIds.Add(currencyWithoutDetails.ApiId);
                    currencyWithoutDetails.Details.State = ApiStatDetailsState.MissingBecauseUnknownByApi;
                }
            }

            if (missingInApiCurrencyIds.Any())
                Module.Logger.Debug("currencies api miss   " + string.Join(" ", missingInApiCurrencyIds));
        }
    }
}
