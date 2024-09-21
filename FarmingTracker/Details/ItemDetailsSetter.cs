using Blish_HUD.Modules.Managers;
using Gw2Sharp.WebApi.V2.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace FarmingTracker
{
    public class ItemDetailsSetter
    {
        public static async Task SetItemDetailsFromApi(Dictionary<int, Stat> itemById, Gw2ApiManager gw2ApiManager)
        {
            var itemIdsWithoutDetails = itemById.Values
                .Where(i => i.Details.State == ApiStatDetailsState.MissingBecauseApiNotCalledYet)
                .Select(i => i.ApiId)
                .ToList();
            
            if (!itemIdsWithoutDetails.Any())
                return;

            if(DebugMode.DebugLoggingRequired)
                Module.Logger.Debug("items      no details " + string.Join(" ", itemIdsWithoutDetails));

            var apiItemsTask = gw2ApiManager.Gw2ApiClient.V2.Items.ManyAsync(itemIdsWithoutDetails);
            var apiPricesTask = gw2ApiManager.Gw2ApiClient.V2.Commerce.Prices.ManyAsync(itemIdsWithoutDetails);

            IReadOnlyList<Item>? apiItems = null;
            IReadOnlyList<CommercePrices>? apiPrices = null;

            try
            {
                await Task.WhenAll(apiItemsTask, apiPricesTask);
            }
            catch (Exception e)
            {
                if (apiItemsTask.IsFaulted)
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
                item.Details.SellsUnitPriceInCopper = apiPrice.Sells.UnitPrice;
                item.Details.BuysUnitPriceInCopper = apiPrice.Buys.UnitPrice;
            }

            if (apiItems.Any() && DebugMode.DebugLoggingRequired)
                Module.Logger.Debug("items      from api   " + string.Join(" ", apiItems.Select(c => c.Id)));

            foreach (var apiItem in apiItems)
            {
                var item = itemById[apiItem.Id];
                item.Details.Name = apiItem.Name;
                item.Details.Description = apiItem.Description ?? "";
                item.Details.IconAssetId = TextureService.GetIconAssetId(apiItem.Icon);
                item.Details.Rarity = apiItem.Rarity;
                item.Details.ItemFlags = apiItem.Flags;
                item.Details.Type = apiItem.Type;
                item.Details.WikiSearchTerm = apiItem.ChatLink;
                item.Details.VendorValueInCopper = apiItem.VendorValue;
                item.Details.State = ApiStatDetailsState.SetByApi;
            }

            var itemsUnknownByApi = itemById.Values.Where(i => i.Details.State == ApiStatDetailsState.MissingBecauseApiNotCalledYet).ToList();
            if (itemsUnknownByApi.Any())
            {
                if(DebugMode.DebugLoggingRequired)
                    Module.Logger.Debug("items      api MISS   " + string.Join(" ", itemsUnknownByApi.Select(i => i.ApiId)));

                foreach (var itemNotFoundByApi in itemsUnknownByApi)
                    itemNotFoundByApi.Details.State = ApiStatDetailsState.MissingBecauseUnknownByApi;
            }
        }

        // can happen when something is not on the trading post (v2.prices)
        // can happen for items like heart items or deprecated items (v2.items)
        // handling NotFoundException is not enough because that occurs on random api failures too.
        private static bool ApiCouldNotFindIds(Exception exception)
        {
            return exception.InnerException.Message.Contains(GW2_API_DOES_NOT_KNOW_IDS);
        }

        private const string GW2_API_DOES_NOT_KNOW_IDS = "all ids provided are invalid";
    }
}
