using System.Collections.Generic;

namespace FarmingTracker
{
    public class StatTooltipSetter
    {
        public static void SetTooltip(Dictionary<int, Stat> statById)
        {
            foreach (var stat in statById.Values)
                stat.Tooltip = CreateTooltip(stat);
        }

        private static string CreateTooltip(Stat stat)
        {
            if (stat.Details.State == ApiStatDetailsState.MissingBecauseApiNotCalledYet)
            {
                var errorMessage = $"Error: Api was not called for stat (id: {stat.ApiId}).";
                Module.Logger.Error(errorMessage);
                return errorMessage;
            }

            if (stat.Details.State == ApiStatDetailsState.MissingBecauseUnknownByApi)
                return
                    $"Unknown item/currency (ID: {stat.ApiId})\n" +
                    $"GW2 API has no information about it.\n" +
                    $"This issue typically occurs for items related to renown hearts.\n" +
                    $"You can look it up with the wiki if you want:\n" +
                    $"https://wiki.guildwars2.com/wiki/Special:RunQuery/Search_by_id";

            var tooltip = $"{stat.Count} {stat.Details.Name}";

            if(!string.IsNullOrWhiteSpace(stat.Details.Description))
                tooltip += $"\n{stat.Details.Description}";

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

    }
}
