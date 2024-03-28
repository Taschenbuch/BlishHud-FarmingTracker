using System.Collections.Generic;

namespace FarmingTracker
{
    public class IconAssetIdAndTooltipSetter
    {
        public static void SetTooltipAndMissingIconAssetIds(Dictionary<int, ItemX> itemById)
        {
            foreach (var item in itemById.Values)
            {
                item.Tooltip = CreateTooltip(item);

                if (item.ApiDetailsAreMissing)
                    item.IconAssetId = BUG_TEXTURE_ASSET_ID; // otherwise AsyncTexture2d.FromAssetId will return null for AssetId = 0
            }
        }

        private static string CreateTooltip(ItemX item)
        {
            if (item.ApiDetailsAreMissing)
                return
                    $"Unknown item (ID: {item.ApiId})\n" +
                    $"GW2 API has no information about it.\n" +
                    $"This issue typically occurs for items related to renown hearts.\n" +
                    $"You can look it up with the wiki if you want:\n" +
                    $"https://wiki.guildwars2.com/wiki/Special:RunQuery/Search_by_id";

            return string.IsNullOrWhiteSpace(item.Description)
                ? $"{item.Count} {item.Name}"
                : $"{item.Count} {item.Name}\n{item.Description}";
        }

        public const int BUG_TEXTURE_ASSET_ID = 157084;
    }
}
