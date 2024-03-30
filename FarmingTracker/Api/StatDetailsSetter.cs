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
        public async Task SetDetailsFromApi(Dictionary<int, ItemX> currencyById, Dictionary<int, ItemX> itemById, Gw2ApiManager gw2ApiManager)
        {
            if (!_apiCurrencyById.Any())
                await CacheAllCurrencyDetailsFromApi(_apiCurrencyById, gw2ApiManager);

            SetCurrencyDetails(currencyById, _apiCurrencyById);
            await SetItemDetailsFromApi(itemById, gw2ApiManager);
        }

        // Caching does work for currencies (<100). But for items it would need like 5 minutes (>60k).
        public static async Task CacheAllCurrencyDetailsFromApi(Dictionary<int, Currency> apiCurrencyById, Gw2ApiManager gw2ApiManager)
        {
            IReadOnlyList<Currency> apiCurrencies;

            try
            {
                apiCurrencies = await gw2ApiManager.Gw2ApiClient.V2.Currencies.AllAsync();
            }
            catch (Exception e)
            {
                throw new Gw2ApiException($"API error: {nameof(CacheAllCurrencyDetailsFromApi)}", e);
            }

            foreach (var apiCurrency in apiCurrencies)
                apiCurrencyById[apiCurrency.Id] = apiCurrency;  
        }

        public static void SetCurrencyDetails(Dictionary<int, ItemX> currencyById, Dictionary<int, Currency> apiCurrencyById)
        {
            var currenciesWithoutDetails = currencyById.Values.Where(c => c.ApiDetailsAreMissing).ToList();
            if (!currenciesWithoutDetails.Any()) // todo weg sobald logging von missing weg ist.
                return;

            Module.Logger.Info("currencies id=0       " + string.Join(" ", currenciesWithoutDetails.Select(c => c.ApiId))); // todo weg

            var missingInApiCurrencyIds = new List<int>();  // todo weg

            foreach (var currencyWithoutDetails in currenciesWithoutDetails)
            {
                if (apiCurrencyById.TryGetValue(currencyWithoutDetails.ApiId, out var apiCurrency))
                {
                    currencyWithoutDetails.Name = apiCurrency.Name;
                    currencyWithoutDetails.Description = apiCurrency.Description;
                    currencyWithoutDetails.IconAssetId = GetIconAssetId(apiCurrency.Icon);
                }
                else
                    missingInApiCurrencyIds.Add(currencyWithoutDetails.ApiId);
            }

            if(missingInApiCurrencyIds.Any())
                Module.Logger.Info("currencies api miss   " + string.Join(" ", missingInApiCurrencyIds)); // todo weg
        }

        public static async Task SetItemDetailsFromApi(Dictionary<int, ItemX> itemById, Gw2ApiManager gw2ApiManager)
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

        private readonly Dictionary<int, Currency> _apiCurrencyById = new Dictionary<int, Currency>();
        private const string GW2_API_DOES_NOT_KNOW_IDS = "all ids provided are invalid";
    }
}
