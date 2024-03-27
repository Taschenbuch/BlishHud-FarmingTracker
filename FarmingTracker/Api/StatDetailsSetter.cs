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
        // todo 2x fast identischer code für currency und item. irgendwie verallgemeinern? auf jedenfall in class auslagern. müllt hier alles zu
        public static async Task SetCurrencyDetailsFromApi(Dictionary<int, ItemX> currencyById, Services services)
        {
            var currenciesWithoutDetails = currencyById.Values.Where(c => c.IsApiInfoMissing).ToList();
            if (currenciesWithoutDetails.Any())
            {
                Module.Logger.Info("currencies no AssetID " + string.Join(" ", currenciesWithoutDetails.Select(c => c.ApiId))); // todo weg
                IReadOnlyList<Currency> apiCurrencies;

                try
                {
                    apiCurrencies = await services.Gw2ApiManager.Gw2ApiClient.V2.Currencies.ManyAsync(currenciesWithoutDetails.Select(c => c.ApiId));
                    Module.Logger.Info("apiCurrencies         " + string.Join(" ", apiCurrencies.Select(c => c.Id))); // todo weg
                }
                catch (Exception e)
                {
                    if (e.Message.Contains(GW2_API_DOES_NOT_KNOW_IDS)) // handling NotFoundException is not enough because that occurs on random api failures too.
                        apiCurrencies = new List<Currency>();
                    else
                        throw new Gw2ApiException("API error: update currencies", e);
                }

                foreach (var apiCurrency in apiCurrencies)
                {
                    var currency = currencyById[apiCurrency.Id];
                    currency.Name = apiCurrency.Name;
                    currency.Description = apiCurrency.Description;
                    currency.IconAssetId = int.Parse(Path.GetFileNameWithoutExtension(apiCurrency.Icon.Url.AbsoluteUri));
                }
            }
        }

        public static async Task SetItemDetailsFromApi(Dictionary<int, ItemX> itemById, Services services)
        {
            var itemsWithoutDetails = itemById.Values.Where(i => i.IsApiInfoMissing).ToList();
            if (itemsWithoutDetails.Any())
            {
                Module.Logger.Info("items no AssetID      " + string.Join(" ", itemsWithoutDetails.Select(c => c.ApiId))); // todo weg
                IReadOnlyList<Item> apiItems;

                try
                {
                    apiItems = await services.Gw2ApiManager.Gw2ApiClient.V2.Items.ManyAsync(itemsWithoutDetails.Select(c => c.ApiId));
                    Module.Logger.Info("apiItems              " + string.Join(" ", apiItems.Select(c => c.Id))); // todo weg
                }
                catch (Exception e)
                {
                    if (e.Message.Contains(GW2_API_DOES_NOT_KNOW_IDS)) // handling NotFoundException is not enough because that occurs on random api failures too.
                        apiItems = new List<Item>();
                    else
                        throw new Gw2ApiException("API error: update items", e);
                }

                foreach (var apiItem in apiItems)
                {
                    var item = itemById[apiItem.Id];
                    item.Name = apiItem.Name;
                    item.Description = apiItem.Description;
                    item.IconAssetId = int.Parse(Path.GetFileNameWithoutExtension(apiItem.Icon.Url.AbsoluteUri));
                }
            }
        }

        private const string GW2_API_DOES_NOT_KNOW_IDS = "all ids provided are invalid";
    }
}
