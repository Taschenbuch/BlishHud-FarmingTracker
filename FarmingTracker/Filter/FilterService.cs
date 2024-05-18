﻿using Gw2Sharp.WebApi.V2.Models;
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

        public static List<Stat> FilterCurrencies(List<Stat> currencies, Services services)
        {
            var countFilter = services.SettingService.CountFilterSetting.Value;
            if (countFilter.Any()) // prevents that all items are hidden, when no filter is set
                currencies = currencies.Where(c => IsShownByCountFilter(c, countFilter)).ToList();

            var currencyFilter = services.SettingService.CurrencyFilterSetting.Value;
            if (currencyFilter.Any()) // prevents that all items are hidden, when no filter is set
                currencies = currencies.Where(c => IsShownByCurrencyFilter(c, currencyFilter)).ToList();

            return currencies;
        }

        public static List<Stat> FilterItems(List<Stat> items, Services services)
        {
            var countFilter = services.SettingService.CountFilterSetting.Value;
            if (countFilter.Any()) // prevents that all items are hidden, when no filter is set
                items = items.Where(c => IsShownByCountFilter(c, countFilter)).ToList();

            var sellMethodFilter = services.SettingService.SellMethodFilterSetting.Value;
            if (sellMethodFilter.Any()) // prevents that all items are hidden, when no filter is set
                items = items.Where(i => IsShownBySellMethodFilter(i, sellMethodFilter)).ToList();

            var rarityFilter = services.SettingService.RarityStatsFilterSetting.Value;
            if (rarityFilter.Any()) // prevents that all items are hidden, when no filter is set
                items = items.Where(i => rarityFilter.Contains(i.Details.Rarity)).ToList();

            var typeFilter = services.SettingService.TypeStatsFilterSetting.Value;
            if (typeFilter.Any()) // prevents that all items are hidden, when no filter is set
                items = items.Where(i => typeFilter.Contains(i.Details.Type)).ToList();

            var flagFilter = services.SettingService.FlagStatsFilterSetting.Value;
            if (flagFilter.Any()) // prevents that all items are hidden, when no filter is set
                items = items.Where(i => IsShownByItemFlagFilter(i, flagFilter)).ToList();

            return items;
        }

        private static bool IsShownByItemFlagFilter(Stat item, List<ItemFlag> flagFilter)
        {
            if (!item.Details.Flags.Any())
                return true;

            return item.Details.Flags.List.Any(f => flagFilter.Contains(f));
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

        private static bool IsShownBySellMethodFilter(Stat stat, List<SellMethodFilter> sellMethodFilter)
        {
            if (sellMethodFilter.Contains(SellMethodFilter.SellableToVendor) && stat.Profit.CanBeSoldToVendor)
                return true;

            if (sellMethodFilter.Contains(SellMethodFilter.SellableOnTradingPost) && stat.Profit.CanBeSoldOnTradingPost)
                return true;

            if (sellMethodFilter.Contains(SellMethodFilter.NotSellable) && stat.Profit.CanNotBeSold)
                return true;

            return false;
        }

        private static bool IsShownByCountFilter(Stat stat, List<CountFilter> countFilter)
        {
            if (countFilter.Contains(CountFilter.PositiveCount) && stat.Count > 0)
                return true;

            if (countFilter.Contains(CountFilter.NegativeCount) && stat.Count < 0)
                return true;

            return false;
        }
    }
}
