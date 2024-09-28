using Gw2Sharp.WebApi.V2.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FarmingTracker
{
    public class FilterService
    {
        public static bool IsUnknownFilterElement<T>(int currencyId)
        {
            return !Enum.IsDefined(typeof(T), currencyId);
        }

        public static (List<Stat> items, List<Stat> currencies) FilterStatsAndSetFunnelOpacity(
            List<Stat> items,
            List<Stat> currencies,
            List<CustomStatProfit> customStatProfits,
            StatsPanels statsPanels,
            SettingService settingService)
        {
            var currenciesCountBeforeFiltering = currencies.Count();
            var itemsCountBeforeFiltering = items.Count();

            currencies = FilterCurrencies(currencies, customStatProfits, settingService);
            items = FilterItems(items, customStatProfits, settingService);

            var noCurrenciesHiddenByFilter = currencies.Count() == currenciesCountBeforeFiltering;
            var noItemsHiddenByFilter = items.Count() == itemsCountBeforeFiltering;

            statsPanels.CurrencyFilterIcon.SetOpacity(noCurrenciesHiddenByFilter);
            statsPanels.ItemsFilterIcon.SetOpacity(noItemsHiddenByFilter);

            return (items, currencies);
        }

        private static List<Stat> FilterCurrencies(List<Stat> currencies, List<CustomStatProfit> customStatProfits, SettingService settingService)
        {
            var knownByApi = settingService.KnownByApiFilterSetting.Value.ToList();
            if (knownByApi.Any()) // prevents that all items are hidden, when no filter is set
                currencies = currencies.Where(s => IsShownByKnownByApiFilter(s, knownByApi)).ToList();

            var countFilter = settingService.CountFilterSetting.Value.ToList();
            if (countFilter.Any()) // prevents that all items are hidden, when no filter is set
                currencies = currencies.Where(s => IsShownByCountSignFilter(s, countFilter)).ToList();

            var sellMethodFilter = settingService.SellMethodFilterSetting.Value.ToList();
            if (sellMethodFilter.Any()) // prevents that all items are hidden, when no filter is set
                currencies = currencies.Where(s => IsShownBySellMethodFilter(s, sellMethodFilter, customStatProfits)).ToList();

            var currencyFilter = settingService.CurrencyFilterSetting.Value.ToList();
            if (currencyFilter.Any()) // prevents that all items are hidden, when no filter is set
                currencies = currencies.Where(s => IsShownByCurrencyFilter(s, currencyFilter)).ToList();

            return currencies;
        }

        private static List<Stat> FilterItems(List<Stat> items, List<CustomStatProfit> customStatProfits, SettingService settingService)
        {
            var knownByApi = settingService.KnownByApiFilterSetting.Value.ToList();
            if (knownByApi.Any()) // prevents that all items are hidden, when no filter is set
                items = items.Where(s => IsShownByKnownByApiFilter(s, knownByApi)).ToList();

            var countFilter = settingService.CountFilterSetting.Value.ToList();
            if (countFilter.Any()) // prevents that all items are hidden, when no filter is set
                items = items.Where(s => IsShownByCountSignFilter(s, countFilter)).ToList();

            var sellMethodFilter = settingService.SellMethodFilterSetting.Value.ToList();
            if (sellMethodFilter.Any()) // prevents that all items are hidden, when no filter is set
                items = items.Where(s => IsShownBySellMethodFilter(s, sellMethodFilter, customStatProfits)).ToList();

            var rarityFilter = settingService.RarityStatsFilterSetting.Value.ToList();
            if (rarityFilter.Any()) // prevents that all items are hidden, when no filter is set
                items = items.Where(s => rarityFilter.Contains(s.Details.Rarity)).ToList();

            var typeFilter = settingService.TypeStatsFilterSetting.Value.ToList();
            if (typeFilter.Any()) // prevents that all items are hidden, when no filter is set
                items = items.Where(s => typeFilter.Contains(s.Details.Type)).ToList();

            var flagFilter = settingService.FlagStatsFilterSetting.Value.ToList();
            if (flagFilter.Any()) // prevents that all items are hidden, when no filter is set
                items = items.Where(s => IsShownByItemFlagFilter(s, flagFilter)).ToList();

            return items;
        }

        private static bool IsShownByItemFlagFilter(Stat item, List<ItemFlag> flagFilter)
        {
            if (!item.Details.ItemFlags.Any())
                return true;

            return item.Details.ItemFlags.List.Any(f => flagFilter.Contains(f));
        }

        private static bool IsShownByCurrencyFilter(Stat c, List<CurrencyFilter> currencyFilter)
        {
            var isUnknownCurrency = IsUnknownFilterElement<CurrencyFilter>(c.ApiId); // e.g. when new currency is released
            if (isUnknownCurrency)
                return true;

            if (currencyFilter.Contains((CurrencyFilter)c.ApiId))
                return true;

            return false;
        }

        private static bool IsShownByKnownByApiFilter(Stat stat, List<KnownByApiFilter> knownByApi)
        {
            if (stat.Details.IsCustomCoinStat)
                return true;

            if (knownByApi.Contains(KnownByApiFilter.KnownByApi) && stat.Details.State == ApiStatDetailsState.SetByApi)
                return true;

            if (knownByApi.Contains(KnownByApiFilter.UnknownByApi) && stat.Details.State == ApiStatDetailsState.MissingBecauseUnknownByApi)
                return true;

            return false;
        }

        private static bool IsShownBySellMethodFilter(Stat stat, List<SellMethodFilter> sellMethodFilter, List<CustomStatProfit> customStatProfits)
        {
            if (stat.IsCoinOrCustomCoin) // always show raw gold.
                return true;

            if (sellMethodFilter.Contains(SellMethodFilter.SellableToVendor) && stat.Profits.CanBeSoldToVendor)
                return true;

            if (sellMethodFilter.Contains(SellMethodFilter.SellableOnTradingPost) && stat.Profits.CanBeSoldOnTp)
                return true;

            if (sellMethodFilter.Contains(SellMethodFilter.NotSellable) && stat.Profits.CanNotBeSold)
                return true;

            if (sellMethodFilter.Contains(SellMethodFilter.CustomProfitIsSet) && customStatProfits.Any(c => c.BelongsToStat(stat)))
                return true;

            return false;
        }

        private static bool IsShownByCountSignFilter(Stat stat, List<CountFilter> countFilter)
        {
            if (countFilter.Contains(CountFilter.PositiveCount) && stat.Count > 0)
                return true;

            if (countFilter.Contains(CountFilter.NegativeCount) && stat.Count < 0)
                return true;

            if (stat.Details.IsCustomCoinStat)
                return true;

            return false;
        }
    }
}
