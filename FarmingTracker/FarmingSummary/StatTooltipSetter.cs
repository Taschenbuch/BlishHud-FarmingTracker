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

            if (stat.Profit.CanBeSoldToVendor)
                tooltip += CreateSellPricesTooltip("Vendor", stat.Profit.SellToVendorProfitInCopper, stat.Count);

            if (stat.Profit.CanBeSoldOnTradingPost)
            {
                tooltip += CreateSellPricesTooltip("TP sell", stat.Profit.SellByListingInTradingPostProfitInCopper, stat.Count);
                tooltip += CreateSellPricesTooltip("TP buy", stat.Profit.SellToBuyOrderInTradingPostProfitInCopper, stat.Count);
            }

            return tooltip;
        }

        private static string CreateSellPricesTooltip(string tooltipHeader, int singleSellPrice, int count)
        {
            var tooltip = $"\n\n{tooltipHeader}:";

            var signFaktor = count < 0 ? -1 : 1;
            var coinEach = new Coin(signFaktor * singleSellPrice);
            var coinEachText = coinEach.CreateCoinText();

            if (count == 1)
            {
                tooltip += $"\n{coinEachText}";
            }
            else
            {
                var coinAll = new Coin(count * singleSellPrice);
                tooltip += $"\n{coinAll.CreateCoinText()} (all)";
                tooltip += $"\n{coinEachText} (each)";
            }

            return tooltip;
        }
    }
}
