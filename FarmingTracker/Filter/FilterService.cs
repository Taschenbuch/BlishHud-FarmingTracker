using Blish_HUD.Controls;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FarmingTracker
{
    public class FilterService
    {
        public static IEnumerable<Stat> FilterCurrencies(IEnumerable<Stat> currencies, Services services)
        {
            currencies = currencies.Where(c => !c.IsCoin);

            var countFilter = services.SettingService.CountFilterSetting.Value;
            if (countFilter.Any()) // prevents that all items are hidden, when no filter is set
                currencies = currencies.Where(c => IsShownByCountFilter(c, countFilter));

            var currencyFilter = services.SettingService.CurrencyFilterSetting.Value;
            if (currencyFilter.Any()) // prevents that all items are hidden, when no filter is set
                currencies = currencies.Where(c => IsShownByCurrencyFilter(c, currencyFilter));

            return currencies;
        }

        public static IEnumerable<Stat> FilterItems(IEnumerable<Stat> items, Services services)
        {
            var countFilter = services.SettingService.CountFilterSetting.Value;
            if (countFilter.Any()) // prevents that all items are hidden, when no filter is set
                items = items.Where(c => IsShownByCountFilter(c, countFilter));

            var sellMethodFilter = services.SettingService.SellMethodFilterSetting.Value;
            if (sellMethodFilter.Any()) // prevents that all items are hidden, when no filter is set
                items = items.Where(i => IsShownBySellMethodFilter(i, sellMethodFilter));

            var rarityFilter = services.SettingService.RarityStatsFilterSetting.Value;
            if (rarityFilter.Any()) // prevents that all items are hidden, when no filter is set
                items = items.Where(i => rarityFilter.Contains(i.Details.Rarity));

            var typeFilter = services.SettingService.TypeStatsFilterSetting.Value;
            if (typeFilter.Any()) // prevents that all items are hidden, when no filter is set
                items = items.Where(i => typeFilter.Contains(i.Details.Type));

            var flagFilter = services.SettingService.FlagStatsFilterSetting.Value;
            if (flagFilter.Any()) // prevents that all items are hidden, when no filter is set
                items = items.Where(i => i.Details.Flag.List.Any(f => flagFilter.Contains(f)));

            return items;
        }

        private static bool IsShownByCurrencyFilter(Stat c, List<CurrencyFilter> currencyFilter)
        {
            var currencyUnknownToModule = !Enum.IsDefined(typeof(CurrencyFilter), c.ApiId); // e.g. when new currency is released
            if (currencyUnknownToModule)
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
