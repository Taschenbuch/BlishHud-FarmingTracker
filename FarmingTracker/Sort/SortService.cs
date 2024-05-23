﻿using System.Collections.Generic;
using System.Linq;

namespace FarmingTracker
{
    public class SortService
    {
        public static List<Stat> SortCurrencies(List<Stat> currencies)
        {
            // WARNING: sorting currency by count will mess up gold-silver-copper-splitted-coin
            return currencies
                .OrderBy(c => c.ApiId)
                .ToList();
        }

        public static List<Stat> SortItems(List<Stat> items, Services services)
        {
            var sortByWithDirectionList = services.SettingService.SortByWithDirectionListSetting.Value.ToList();
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
                    return items.ThenBy(i => i.Count);
                case SortByWithDirection.Count_Descending:
                    return items.ThenByDescending(i => i.Count);
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
                    return items.ThenBy(i => i.CountSign * i.ProfitAll.MaxProfitInCopper);
                case SortByWithDirection.ProfitAll_Descending:
                    return items.ThenByDescending(i => i.CountSign * i.ProfitAll.MaxProfitInCopper);
                case SortByWithDirection.ProfitPerItem_Ascending:
                    return items.ThenBy(i => i.CountSign * i.ProfitEach.MaxProfitInCopper);
                case SortByWithDirection.ProfitPerItem_Descending:
                    return items.ThenByDescending(i => i.CountSign * i.ProfitEach.MaxProfitInCopper);
                case SortByWithDirection.VendorProfitAll_Ascending:
                    return items.ThenBy(i => i.CountSign * i.ProfitAll.VendorProfitInCopper);
                case SortByWithDirection.VendorProfitAll_Descending:
                    return items.ThenByDescending(i => i.CountSign * i.ProfitAll.VendorProfitInCopper);
                case SortByWithDirection.VendorProfitPerItem_Ascending:
                    return items.ThenBy(i => i.CountSign * i.ProfitEach.VendorProfitInCopper);
                case SortByWithDirection.VendorProfitPerItem_Descending:
                    return items.ThenByDescending(i => i.CountSign * i.ProfitEach.VendorProfitInCopper);
                case SortByWithDirection.TradingPostProfitAll_Ascending:
                    return items.ThenBy(i => i.CountSign * i.ProfitAll.MaxTpProfitInCopper);
                case SortByWithDirection.TradingPostProfitAll_Descending:
                    return items.ThenByDescending(i => i.CountSign * i.ProfitAll.MaxTpProfitInCopper);
                case SortByWithDirection.TradingPostProfitPerItem_Ascending:
                    return items.ThenBy(i => i.CountSign * i.ProfitEach.MaxTpProfitInCopper);
                case SortByWithDirection.TradingPostProfitPerItem_Descending:
                    return items.ThenByDescending(i => i.CountSign * i.ProfitEach.MaxTpProfitInCopper);
                default:
                    Module.Logger.Error(Helper.CreateSwitchCaseNotFoundMessage(sortByWithDirection, nameof(SortByWithDirection), "dont sort"));
                    return items;
            }
        }
    }
}
