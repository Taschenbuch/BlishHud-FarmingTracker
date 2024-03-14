using Gw2Sharp.WebApi.V2;
using Gw2Sharp.WebApi.V2.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FarmingTracker
{
    public class ItemSearchService
    {
        public static List<ItemX> GetItemIdsAndCounts(
            Task<IApiV2ObjectList<Character>> charactersTask,
            Task<IApiV2ObjectList<AccountItem>> bankTask,
            Task<IApiV2ObjectList<AccountItem>> sharedInventoryTask,
            Task<IApiV2ObjectList<AccountMaterial>> materialStorageTask)
        {
            var inventoryItems = GetItems(GetAllCharactersInventoryItems(charactersTask));
            var bankItems = GetItems(bankTask.Result.ToList());
            var sharedInventoryItems = GetItems(sharedInventoryTask.Result.ToList());
            var materialStorageItems = GetItems(materialStorageTask.Result.ToList());

            var items = new List<ItemX>();
            items.AddRange(inventoryItems);
            items.AddRange(bankItems);
            items.AddRange(sharedInventoryItems);
            items.AddRange(materialStorageItems);

            var distinctItems = items
                .GroupBy(i => i.ApiItemId)
                .Select(g => new ItemX
                {
                    ApiItemId = g.Key,
                    Count = g.Sum(i => i.Count)
                });

            return distinctItems.ToList();
        }

        private static IEnumerable<ItemX> GetItems(List<AccountItem> accountItems)
        {
            foreach (var accountItem in accountItems.Where(IsNotEmptyItemSlot))
                yield return new ItemX()
                {
                    ApiItemId = accountItem.Id,
                    Count = accountItem.Count,
                };
        }

        private static IEnumerable<ItemX> GetItems(List<AccountMaterial> materialItems)
        {
            foreach (var materialItem in materialItems)
                yield return new ItemX()
                {
                    ApiItemId = materialItem.Id,
                    Count = materialItem.Count,
                };
        }

        private static List<AccountItem> GetAllCharactersInventoryItems(Task<IApiV2ObjectList<Character>> charactersTask)
        {
            var allCharactersInventoryItems = new List<AccountItem>();

            foreach (var character in charactersTask.Result)
            {
                if (character.Bags == null)
                    continue;

                var singleCharacterInventoryItems = GetSingleCharacterInventoryItems(character);
                allCharactersInventoryItems.AddRange(singleCharacterInventoryItems);
            }

            return allCharactersInventoryItems;
        }

        private static List<AccountItem> GetSingleCharacterInventoryItems(Character character)
        {
            return character.Bags
                .Where(IsNotEmptyBagSlot)
                .Select(b => b.Inventory)
                .SelectMany(i => i)
                .Where(IsNotEmptyItemSlot)
                .ToList();
        }

        private static bool IsNotEmptyBagSlot(CharacterInventoryBag bag) => bag != null;
        private static bool IsNotEmptyItemSlot(AccountItem itemSlot) => itemSlot != null;
    }
}