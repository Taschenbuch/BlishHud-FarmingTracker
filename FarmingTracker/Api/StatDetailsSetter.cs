using Blish_HUD.Modules.Managers;
using Gw2Sharp.WebApi;
using Gw2Sharp.WebApi.V2.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FarmingTracker
{
    public class StatDetailsSetter
    {
        public async Task SetDetailsFromApi(Dictionary<int, Stat> currencyById, Dictionary<int, Stat> itemById, Gw2ApiManager gw2ApiManager)
        {
            var hasToInitializeCache = !_currencyDetailsByIdCache.Any();
            if (hasToInitializeCache)
                _currencyDetailsByIdCache = await CreateCacheWithAllApiCurrencies(gw2ApiManager);

            SetCurrencyDetails(currencyById, _currencyDetailsByIdCache);
            await SetItemDetailsFromApi(itemById, gw2ApiManager);
        }

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
                    IconAssetId = GetIconAssetId(apiCurrency.Icon),
                };

            return currencyDetailsById;
        }

        public static void SetCurrencyDetails(Dictionary<int, Stat> currencyById, Dictionary<int, CurrencyDetails> currencyDetailsByIdCache)
        {
            var currenciesWithoutDetails = currencyById.Values.Where(c => c.ApiDetailsAreMissing).ToList();
            if (!currenciesWithoutDetails.Any())
                return;

            Module.Logger.Info("currencies id=0       " + string.Join(" ", currenciesWithoutDetails.Select(c => c.ApiId))); // todo weg

            var missingInApiCurrencyIds = new List<int>();  // todo weg

            foreach (var currencyWithoutDetails in currenciesWithoutDetails)
            {
                if (currencyDetailsByIdCache.TryGetValue(currencyWithoutDetails.ApiId, out var currencyDetails))
                {
                    currencyWithoutDetails.Name = currencyDetails.Name;
                    currencyWithoutDetails.Description = currencyDetails.Description;
                    currencyWithoutDetails.IconAssetId = currencyDetails.IconAssetId;
                }
                else
                    missingInApiCurrencyIds.Add(currencyWithoutDetails.ApiId);
            }

            if(missingInApiCurrencyIds.Any())
                Module.Logger.Info("currencies api miss   " + string.Join(" ", missingInApiCurrencyIds)); // todo weg
        }

        public static async Task SetItemDetailsFromApi(Dictionary<int, Stat> itemById, Gw2ApiManager gw2ApiManager)
        {
            var itemsWithoutDetails = itemById.Values.Where(i => i.ApiDetailsAreMissing).ToList();
            if (itemsWithoutDetails.Any())
            {
                Module.Logger.Info("items      id=0       " + string.Join(" ", itemsWithoutDetails.Select(c => c.ApiId))); // todo weg
                IReadOnlyList<Item> apiItems;

                try
                {
                    apiItems = await gw2ApiManager.Gw2ApiClient.V2.Items.ManyAsync(itemsWithoutDetails.Select(c => c.ApiId));
                    Module.Logger.Info("items      api        " + string.Join(" ", apiItems.Select(c => c.Id))); // todo weg
                }
                catch (Exception e) when(e.Message.Contains(GW2_API_DOES_NOT_KNOW_IDS))
                {
                    apiItems = new List<Item>(); // handling NotFoundException is not enough because that occurs on random api failures too.
                }
                catch (Exception e)
                {
                    throw new Gw2ApiException($"API error: {nameof(SetItemDetailsFromApi)}", e);
                }

                foreach (var apiItem in apiItems)
                {
                    var item = itemById[apiItem.Id];
                    item.Name = apiItem.Name;
                    item.Description = apiItem.Description;
                    item.IconAssetId = GetIconAssetId(apiItem.Icon);
                }
            }
        }

        private static int GetIconAssetId(RenderUrl icon)
        {
            return int.Parse(Path.GetFileNameWithoutExtension(icon.Url.AbsoluteUri));
        }

        private Dictionary<int, CurrencyDetails> _currencyDetailsByIdCache = new Dictionary<int, CurrencyDetails>();
        private const string GW2_API_DOES_NOT_KNOW_IDS = "all ids provided are invalid";
    }
}
