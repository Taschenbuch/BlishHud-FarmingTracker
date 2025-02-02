using System.Collections.Generic;
using System.Linq;

namespace FarmingTracker
{
    public class SortService
    {
        public static (List<Stat> items, List<Stat> currencies) SortStats(List<Stat> items, List<Stat> currencies, SettingService settingService)
        {
            currencies = SortCurrencies(currencies);
            items = SortItems(items, settingService);

            return (items, currencies);
        }

        private static List<Stat> SortCurrencies(List<Stat> currencies)
        {
            // WARNING: sorting currency by count will mess up gold-silver-copper-splitted-coin
            return currencies
                .OrderBy(c => c.ApiId)
                .ToList();
        }

        private static List<Stat> SortItems(List<Stat> items, SettingService settingService)
        {
            var sortByWithDirectionList = settingService.SortByWithDirectionListSetting.Value.ToList();
            if (!sortByWithDirectionList.Any())
                return items;

            // hack: converts to IOrderedEnumerable without ordering anything. This allows to only use ThenBy() afterwards.
            var orderedItems = items.OrderBy(i => 0);

            foreach (var sortByWithDirection in sortByWithDirectionList)
                orderedItems = ItemOrderBy(orderedItems, sortByWithDirection);

            return orderedItems.ToList();                
        }

        private static IOrderedEnumerable<Stat> ItemOrderBy(IOrderedEnumerable<Stat> items, SortByWithDirection sortByWithDirection)
        {
            switch (sortByWithDirection)
            {
                case SortByWithDirection.Name_Ascending:
                    return items.ThenBy(i => i.Details.Name);
                case SortByWithDirection.Name_Descending:
                    return items.ThenByDescending(i => i.Details.Name);
                case SortByWithDirection.Rarity_Ascending:
                    return items.ThenBy(i => i.Details.Rarity);
                case SortByWithDirection.Rarity_Descending:
                    return items.ThenByDescending(i => i.Details.Rarity);
                case SortByWithDirection.Count_Ascending:
                    return items.ThenBy(i => i.Signed_Count.Value);
                case SortByWithDirection.Count_Descending:
                    return items.ThenByDescending(i => i.Signed_Count.Value);
                case SortByWithDirection.PositiveAndNegativeCount_Ascending:
                    return items.ThenBy(i => i.CountSign);
                case SortByWithDirection.PositiveAndNegativeCount_Descending:
                    return items.ThenByDescending(i => i.CountSign);
                case SortByWithDirection.ApiId_Ascending:
                    return items.ThenBy(i => i.ApiId);
                case SortByWithDirection.ApiId_Descending:
                    return items.ThenByDescending(i => i.ApiId);
                case SortByWithDirection.ItemType_Ascending:
                    return items.ThenBy(i => i.Details.Type);
                case SortByWithDirection.ItemType_Descending:
                    return items.ThenByDescending(i => i.Details.Type);
                case SortByWithDirection.ProfitAll_Ascending:
                    return items.ThenBy(i => i.Signed_Count.Value * i.Profit.Unsigned_Max_ProfitInCopper);
                case SortByWithDirection.ProfitAll_Descending:
                    return items.ThenByDescending(i => i.Signed_Count.Value * i.Profit.Unsigned_Max_ProfitInCopper);
                case SortByWithDirection.ProfitPerItem_Ascending:
                    return items.ThenBy(i => i.CountSign * i.Profit.Unsigned_Max_ProfitInCopper);
                case SortByWithDirection.ProfitPerItem_Descending:
                    return items.ThenByDescending(i => i.CountSign * i.Profit.Unsigned_Max_ProfitInCopper);
                case SortByWithDirection.VendorProfitAll_Ascending:
                    return items.ThenBy(i => i.Signed_Count.Value * i.Profit.Unsigned_Vendor_ProfitInCopper.Value);
                case SortByWithDirection.VendorProfitAll_Descending:
                    return items.ThenByDescending(i => i.Signed_Count.Value * i.Profit.Unsigned_Vendor_ProfitInCopper.Value);
                case SortByWithDirection.VendorProfitPerItem_Ascending:
                    return items.ThenBy(i => i.CountSign * i.Profit.Unsigned_Vendor_ProfitInCopper.Value);
                case SortByWithDirection.VendorProfitPerItem_Descending:
                    return items.ThenByDescending(i => i.CountSign * i.Profit.Unsigned_Vendor_ProfitInCopper.Value);
                case SortByWithDirection.TradingPostProfitAll_Ascending:
                    return items.ThenBy(i => i.Signed_Count.Value * i.Profit.Unsigned_MaxTp_ProfitInCopper.Value);
                case SortByWithDirection.TradingPostProfitAll_Descending:
                    return items.ThenByDescending(i => i.Signed_Count.Value * i.Profit.Unsigned_MaxTp_ProfitInCopper.Value);
                case SortByWithDirection.TradingPostProfitPerItem_Ascending:
                    return items.ThenBy(i => i.CountSign * i.Profit.Unsigned_MaxTp_ProfitInCopper.Value);
                case SortByWithDirection.TradingPostProfitPerItem_Descending:
                    return items.ThenByDescending(i => i.CountSign * i.Profit.Unsigned_MaxTp_ProfitInCopper.Value);
                case SortByWithDirection.CustomProfitAll_Ascending:
                    return items.ThenBy(i => i.Signed_Count.Value * i.Profit.Unsigned_Custom_ProfitInCopper.GetValueOrDefault(0)); // null as 0 customProfit will end up in the middle of the sort result (-12, ..., 0, ..., +323)
                case SortByWithDirection.CustomProfitAll_Descending:
                    return items.ThenByDescending(i => i.Signed_Count.Value * i.Profit.Unsigned_Custom_ProfitInCopper.GetValueOrDefault(0));
                case SortByWithDirection.CustomProfitPerItem_Ascending:
                    return items.ThenBy(i => i.CountSign * i.Profit.Unsigned_Custom_ProfitInCopper.GetValueOrDefault(0));
                case SortByWithDirection.CustomProfitPerItem_Descending:
                    return items.ThenByDescending(i => i.CountSign * i.Profit.Unsigned_Custom_ProfitInCopper.GetValueOrDefault(0));
                default:
                    Module.Logger.Error(Helper.CreateSwitchCaseNotFoundMessage(sortByWithDirection, nameof(SortByWithDirection), "dont sort"));
                    return items;
            }
        }
    }
}
