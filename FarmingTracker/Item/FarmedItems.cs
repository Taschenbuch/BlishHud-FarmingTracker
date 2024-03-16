using System.Collections.Generic;
using System.Linq;

namespace FarmingTracker
{
    public class FarmedItems
    {
        public static List<ItemX> DetermineFarmedItems(List<ItemX> newItems, List<ItemX> oldItems) // todo extract class
        {
            var itemById = new Dictionary<int, ItemX>();

            foreach (var newItem in newItems)
                itemById[newItem.ApiId] = newItem;

            foreach (var oldItem in oldItems)
            {
                if (itemById.TryGetValue(oldItem.ApiId, out var item))
                    item.Count -= oldItem.Count;
                else
                    itemById[oldItem.ApiId] = new ItemX // do not set oldItem here. oldItem must not be modified
                    {
                        ApiId = oldItem.ApiId,
                        Count = -oldItem.Count,
                    };
            }

            return itemById.Values.Where(i => i.Count != 0).ToList();
        }
    }
}
