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

            var tooltip = $"{stat.Count} {stat.Name}";

            if(!string.IsNullOrWhiteSpace(stat.Description))
                tooltip += $"\n{stat.Description}";

            var canBeSoldToVendor = stat.Profit.SellToVendorInCopper > 0;
            if (canBeSoldToVendor)
            {
                tooltip += "\n\nVendor:";

                var signFaktor = stat.Count < 0 ? -1 : 1;
                var coinEach = new Coin(signFaktor * stat.Profit.SellToVendorInCopper);

                if (stat.Count == 1)
                {
                    tooltip += $"\n{coinEach.CreateCoinText()}";
                }
                else
                {
                    var coinAll = new Coin(stat.Count * stat.Profit.SellToVendorInCopper);
                    tooltip += $"\n{coinEach.CreateCoinText()} each";
                    tooltip += $"\n{coinAll.CreateCoinText()} all";
                }
            }

            var canBeSoldOnTradingPost = stat.Profit.SellByListingInTradingPostInCopper > 0;
            if (canBeSoldOnTradingPost)
            {
                tooltip += "\n\nTP Sell:";

                var signFaktor = stat.Count < 0 ? -1 : 1;
                var coinEach = new Coin(signFaktor * stat.Profit.SellByListingInTradingPostInCopper);

                if (stat.Count == 1)
                {
                    tooltip += $"\n{coinEach.CreateCoinText()}";
                }
                else
                {
                    var coinAll = new Coin(stat.Count * stat.Profit.SellByListingInTradingPostInCopper);
                    tooltip += $"\n{coinEach.CreateCoinText()} each";
                    tooltip += $"\n{coinAll.CreateCoinText()} all";
                }
                tooltip += "\n(15% TP fee was deducted)";
            }

            return tooltip;
        }

        public const int BUG_TEXTURE_ASSET_ID = 157084;
    }
}
