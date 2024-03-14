using Gw2Sharp.WebApi.V2;
using Gw2Sharp.WebApi.V2.Models;
using System.Collections.Generic;
using System.Linq;

namespace FarmingTracker
{
    public class ItemSearcher
    {
        public static List<ItemX> GetItemIdsAndCounts(
            IApiV2ObjectList<Character> apiCharacters,
            IApiV2ObjectList<AccountItem> apiBankItems,
            IApiV2ObjectList<AccountItem> apiSharedInventoryItems,
            IApiV2ObjectList<AccountMaterial> apiMaterialStorageItems)
        {
            var inventoryItems = GetItems(GetAllCharactersInventoryItems(apiCharacters));
            var bankItems = GetItems(apiBankItems.ToList());
            var sharedInventoryItems = GetItems(apiSharedInventoryItems.ToList());
            var materialStorageItems = GetItems(apiMaterialStorageItems.ToList());

            var items = new List<ItemX>();
            items.AddRange(inventoryItems);
            items.AddRange(bankItems);
            items.AddRange(sharedInventoryItems);
            items.AddRange(materialStorageItems);

            var distinctItems = items
                .GroupBy(i => i.ApiId)
                .Select(g => new ItemX
                {
                    ApiId = g.Key,
                    Count = g.Sum(i => i.Count),
                    ApiIdType = ApiIdType.Item,
                });

            return distinctItems.Where(i => i.Count != 0).ToList(); // e.g. materialStorage returns items even when count is 0.
        }

        private static IEnumerable<ItemX> GetItems(List<AccountItem> accountItems)
        {
            foreach (var accountItem in accountItems.Where(IsNotEmptyItemSlot))
                yield return new ItemX()
                {
                    ApiId = accountItem.Id,
                    Count = accountItem.Count,
                };
        }

        private static IEnumerable<ItemX> GetItems(List<AccountMaterial> materialItems)
        {
            foreach (var materialItem in materialItems)
                yield return new ItemX()
                {
                    ApiId = materialItem.Id,
                    Count = materialItem.Count,
                };
        }

        private static List<AccountItem> GetAllCharactersInventoryItems(IApiV2ObjectList<Character> apiCharacters)
        {
            var allCharactersInventoryItems = new List<AccountItem>();

            foreach (var apiCharacter in apiCharacters)
            {
                if (apiCharacter.Bags == null)
                    continue;

                var singleCharacterInventoryItems = GetSingleCharacterInventoryItems(apiCharacter);
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