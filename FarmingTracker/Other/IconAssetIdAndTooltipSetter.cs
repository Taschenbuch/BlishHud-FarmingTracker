﻿using System.Collections.Generic;

namespace FarmingTracker
{
    public class IconAssetIdAndTooltipSetter
    {
        public static void SetTooltipAndMissingIconAssetIds(Dictionary<int, Stat> statById)
        {
            foreach (var stat in statById.Values)
            {
                stat.Tooltip = CreateTooltip(stat);

                if (stat.ApiDetailsAreMissing)
                    stat.IconAssetId = BUG_TEXTURE_ASSET_ID; // otherwise AsyncTexture2d.FromAssetId will return null for AssetId = 0
            }
        }

        private static string CreateTooltip(Stat stat)
        {
            if (stat.ApiDetailsAreMissing)
                return
                    $"Unknown item (ID: {stat.ApiId})\n" +
                    $"GW2 API has no information about it.\n" +
                    $"This issue typically occurs for items related to renown hearts.\n" +
                    $"You can look it up with the wiki if you want:\n" +
                    $"https://wiki.guildwars2.com/wiki/Special:RunQuery/Search_by_id";

            return string.IsNullOrWhiteSpace(stat.Description)
                ? $"{stat.Count} {stat.Name}"
                : $"{stat.Count} {stat.Name}\n{stat.Description}";
        }

        public const int BUG_TEXTURE_ASSET_ID = 157084;
    }
}
