using System.Collections.Generic;
using System.Linq;

namespace FarmingTracker
{
    public class SortService
    {
        public static List<Stat> SortCurrencies(List<Stat> currencies)
        {
            return currencies
                .OrderBy(c => c.ApiId)
                .ToList();
        }

        public static List<Stat> SortItems(List<Stat> items, Services services)
        {
            var sortByWithDirectionList = services.SettingService.SortByWithDirectionListSetting.Value;

            if (!sortByWithDirectionList.Any())
                return items;

            var isFirst = false;

            var orderedItems = items.OrderBy(i => 0); // no ordering.

            foreach (var sortByWithDirection in sortByWithDirectionList)
            {
                if (isFirst)
                {
                    isFirst = false;
                    orderedItems = OrderByOrThenBy(orderedItems, sortByWithDirection, false);
                }
                else
                    orderedItems = OrderByOrThenBy(orderedItems, sortByWithDirection, true);
            }

            return orderedItems.ToList();                
        }

        private static IOrderedEnumerable<Stat> OrderByOrThenBy(IOrderedEnumerable<Stat> items, SortByWithDirection sortByWithDirection, bool isThenBy)
        {
            switch (sortByWithDirection)
            {
                case SortByWithDirection.NameAsc:
                    return isThenBy
                        ? items.OrderBy(i => i.Details.Name)
                        : items.ThenBy(i => i.Details.Name);
                case SortByWithDirection.NameDes:
                    return isThenBy
                        ? items.OrderByDescending(i => i.Details.Name)
                        : items.ThenByDescending(i => i.Details.Name);
                case SortByWithDirection.CountAsc:
                    return isThenBy
                        ? items.OrderBy(i => i.Count)
                        : items.ThenBy(i => i.Count);
                case SortByWithDirection.CountDes:
                    return isThenBy
                        ? items.OrderByDescending(i => i.Count)
                        : items.ThenByDescending(i => i.Count);
                case SortByWithDirection.PositiveAndNegativeCountAsc:
                    return isThenBy
                       ? items.OrderBy(i => i.Count >= 0 ? 1 : -1)
                       : items.ThenBy(i => i.Count >= 0 ? 1 : -1);
                case SortByWithDirection.PositiveAndNegativeCountDes:
                    return isThenBy
                        ? items.OrderByDescending(i => i.Count >= 0 ? 1 : -1)
                        : items.ThenByDescending(i => i.Count >= 0 ? 1 : -1);
                case SortByWithDirection.ApiIdAsc:
                    return isThenBy
                        ? items.OrderBy(i => i.ApiId)
                        : items.ThenBy(i => i.ApiId);
                case SortByWithDirection.ApiIdDes:
                    return isThenBy
                        ? items.OrderByDescending(i => i.ApiId)
                        : items.ThenByDescending(i => i.ApiId);
                case SortByWithDirection.ItemTypeAsc:
                    return isThenBy
                        ? items.OrderBy(i => i.Details.Type)
                        : items.ThenBy(i => i.Details.Type);
                case SortByWithDirection.ItemTypeDes:
                    return isThenBy
                        ? items.OrderByDescending(i => i.Details.Type)
                        : items.ThenByDescending(i => i.Details.Type);
                case SortByWithDirection.ProfitAllAsc:
                    return isThenBy
                        ? items.OrderBy(i => i.ProfitAll.MaxProfitInCopper)
                        : items.ThenBy(i => i.ProfitAll.MaxProfitInCopper);
                case SortByWithDirection.ProfitAllDes:
                    return isThenBy
                        ? items.OrderByDescending(i => i.ProfitAll.MaxProfitInCopper)
                        : items.ThenByDescending(i => i.ProfitAll.MaxProfitInCopper);
                case SortByWithDirection.ProfitPerItemAsc:
                    return isThenBy
                        ? items.OrderBy(i => i.ProfitEach.MaxProfitInCopper)
                        : items.ThenBy(i => i.ProfitEach.MaxProfitInCopper);
                case SortByWithDirection.ProfitPerItemDes:
                    return isThenBy
                        ? items.OrderByDescending(i => i.ProfitEach.MaxProfitInCopper)
                        : items.ThenByDescending(i => i.ProfitEach.MaxProfitInCopper);
                case SortByWithDirection.VendorProfitAllAsc:
                    return isThenBy
                        ? items.OrderBy(i => i.ProfitAll.SellToVendorProfitInCopper)
                        : items.ThenBy(i => i.ProfitAll.SellToVendorProfitInCopper);
                case SortByWithDirection.VendorProfitAllDes:
                    return isThenBy
                        ? items.OrderByDescending(i => i.ProfitAll.SellToVendorProfitInCopper)
                        : items.ThenByDescending(i => i.ProfitAll.SellToVendorProfitInCopper);
                case SortByWithDirection.VendorProfitPerItemAsc:
                    return isThenBy
                        ? items.OrderBy(i => i.ProfitEach.SellToVendorProfitInCopper)
                        : items.ThenBy(i => i.ProfitEach.SellToVendorProfitInCopper);
                case SortByWithDirection.VendorProfitPerItemDes:
                    return isThenBy
                        ? items.OrderByDescending(i => i.ProfitEach.SellToVendorProfitInCopper)
                        : items.ThenByDescending(i => i.ProfitEach.SellToVendorProfitInCopper);
                case SortByWithDirection.TradingPostProfitAllAsc:
                    return isThenBy
                        ? items.OrderBy(i => i.ProfitAll.MaxTradingPostProfitInCopper)
                        : items.ThenBy(i => i.ProfitAll.MaxTradingPostProfitInCopper);
                case SortByWithDirection.TradingPostProfitAllDes:
                    return isThenBy
                        ? items.OrderByDescending(i => i.ProfitAll.MaxTradingPostProfitInCopper)
                        : items.ThenByDescending(i => i.ProfitAll.MaxTradingPostProfitInCopper);
                case SortByWithDirection.TradingPostProfitPerItemAsc:
                    return isThenBy
                        ? items.OrderBy(i => i.ProfitEach.MaxTradingPostProfitInCopper)
                        : items.ThenBy(i => i.ProfitEach.MaxTradingPostProfitInCopper);
                case SortByWithDirection.TradingPostProfitPerItemDes:
                    return isThenBy
                        ? items.OrderByDescending(i => i.ProfitEach.MaxTradingPostProfitInCopper)
                        : items.ThenByDescending(i => i.ProfitEach.MaxTradingPostProfitInCopper);
                default:
                    Module.Logger.Error($"Fallback: dont sort. Because switch case missing or should not be be handled here: {nameof(SortByWithDirection)}.{sortByWithDirection}.");
                    return items;
            }
        }
    }
}
