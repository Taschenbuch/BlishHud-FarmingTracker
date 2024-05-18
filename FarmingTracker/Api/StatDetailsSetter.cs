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
        public async Task SetDetailsFromApi(Stats stats, Gw2ApiManager gw2ApiManager)
        {
            var hasToInitializeCache = !_currencyDetailsByIdCache.Any();
            if (hasToInitializeCache)
                _currencyDetailsByIdCache = await CreateCacheWithAllApiCurrencies(gw2ApiManager);

            SetCurrencyDetails(stats.CurrencyById, _currencyDetailsByIdCache);
            await SetItemDetailsFromApi(stats.ItemById, gw2ApiManager);
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
                    currencyWithoutDetails.Details.Name = currencyDetails.Description;
                    currencyWithoutDetails.Details.IconAssetId = currencyDetails.IconAssetId;
                    currencyWithoutDetails.Details.State = ApiStatDetailsState.SetByApi;
                }
                else
                {
                    missingInApiCurrencyIds.Add(currencyWithoutDetails.ApiId);
                    currencyWithoutDetails.Details.State = ApiStatDetailsState.MissingBecauseUnknownByApi;
                }
            }

            if(missingInApiCurrencyIds.Any())
                Module.Logger.Debug("currencies api miss   " + string.Join(" ", missingInApiCurrencyIds));
        }

        public static async Task SetItemDetailsFromApi(Dictionary<int, Stat> itemById, Gw2ApiManager gw2ApiManager)
        {
            var itemIdsWithoutDetails = itemById.Values.Where(i => i.Details.State == ApiStatDetailsState.MissingBecauseApiNotCalledYet).Select(i => i.ApiId).ToList();
            if (!itemIdsWithoutDetails.Any())
                return;

            Module.Logger.Debug("items      no details " + string.Join(" ", itemIdsWithoutDetails));
            var apiItemsTask = gw2ApiManager.Gw2ApiClient.V2.Items.ManyAsync(itemIdsWithoutDetails);
            var apiPricesTask = gw2ApiManager.Gw2ApiClient.V2.Commerce.Prices.ManyAsync(itemIdsWithoutDetails);
            
            IReadOnlyList<Item> apiItems = null;
            IReadOnlyList<CommercePrices> apiPrices = null;

            try
            {
                await Task.WhenAll(apiItemsTask, apiPricesTask);
            }
            catch (Exception e)
            {
                if(apiItemsTask.IsFaulted)
                {
                    if (ApiCouldNotFindIds(apiItemsTask.Exception))
                        apiItems = new List<Item>();
                    else
                        throw new Gw2ApiException($"API error: get stat details from v2.items failed", e);
                }

                if (apiPricesTask.IsFaulted)
                {
                    if (ApiCouldNotFindIds(apiPricesTask.Exception))
                        apiPrices = new List<CommercePrices>();
                    else
                        throw new Gw2ApiException($"API error: get trading post prices from v2.prices failed", e);
                }
            }

            // only set a result when task for it completed successfully otherwise it was already set in catch.
            apiItems ??= apiItemsTask.Result;
            apiPrices ??= apiPricesTask.Result;

            foreach (var apiPrice in apiPrices)
            {
                var item = itemById[apiPrice.Id];
                item.Profit.SetTpSellAndBuyProfit(apiPrice.Sells.UnitPrice, apiPrice.Buys.UnitPrice);
            }

            if (apiItems.Any())
                Module.Logger.Debug("items      from api   " + string.Join(" ", apiItems.Select(c => c.Id)));

            foreach (var apiItem in apiItems)
            {
                var item = itemById[apiItem.Id];
                item.Details.Name = apiItem.Name;
                item.Details.Description = apiItem.Description;
                item.Details.IconAssetId = GetIconAssetId(apiItem.Icon);
                item.Details.Rarity = apiItem.Rarity;
                item.Details.Flag = apiItem.Flags;
                item.Details.Type = apiItem.Type;
                var canBeSoldToVendor = !apiItem.Flags.Any(f => f == ItemFlag.NoSell) && apiItem.VendorValue != 0;
                item.Profit.CanBeSoldToVendor = canBeSoldToVendor;
                item.Profit.SellToVendorProfitInCopper = canBeSoldToVendor ? apiItem.VendorValue : 0; // it sometimes has a VendorValue even when it cannot be sold to vendor. That would distort the profit.
                item.Details.State = ApiStatDetailsState.SetByApi;
            }

            var itemsUnknownByApi = itemById.Values.Where(i => i.Details.State == ApiStatDetailsState.MissingBecauseApiNotCalledYet).ToList();
            if (itemsUnknownByApi.Any())
            {
                Module.Logger.Debug("items      api MISS   " + string.Join(" ", itemsUnknownByApi.Select(i => i.ApiId)));

                foreach (var itemNotFoundByApi in itemsUnknownByApi)
                    itemNotFoundByApi.Details.State = ApiStatDetailsState.MissingBecauseUnknownByApi;
            }
        }

        private static int GetIconAssetId(RenderUrl icon)
        {
            return int.Parse(Path.GetFileNameWithoutExtension(icon.Url.AbsoluteUri));
        }

        // can happen when something is not on the trading post (v2.prices)
        // can happen for items like heart items or deprecated items (v2.items)
        // handling NotFoundException is not enough because that occurs on random api failures too.
        private static bool ApiCouldNotFindIds(Exception exception)
        {
            return exception.InnerException.Message.Contains(GW2_API_DOES_NOT_KNOW_IDS);
        }

        private Dictionary<int, CurrencyDetails> _currencyDetailsByIdCache = new Dictionary<int, CurrencyDetails>();
        private const string GW2_API_DOES_NOT_KNOW_IDS = "all ids provided are invalid";
    }
}
