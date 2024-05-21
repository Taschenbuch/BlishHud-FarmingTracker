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

            var isSingleItem = stat.Count == 1;

            if (stat.ProfitEach.CanBeSoldToVendor)
            {
                tooltip += CreateProfitTooltipPart("Vendor", isSingleItem, stat.CountSign, stat.ProfitEach.VendorProfitInCopper, stat.ProfitAll.VendorProfitInCopper);
            }

            if (stat.ProfitEach.CanBeSoldOnTp)
            {
                tooltip += CreateProfitTooltipPart("TP sell", isSingleItem, stat.CountSign, stat.ProfitEach.TpSellProfitInCopper, stat.ProfitAll.TpSellProfitInCopper);
                tooltip += CreateProfitTooltipPart("TP buy", isSingleItem, stat.CountSign, stat.ProfitEach.TpBuyProfitInCopper, stat.ProfitAll.TpBuyProfitInCopper);
                tooltip += "\n\n(15% trading post fee is already deducted from TP sell/buy.)";
            }

            return tooltip;
        }

        private static string CreateProfitTooltipPart(string tooltipHeader, bool isSingleItem, long sign, long profitInCopperEach, long profitInCopperAll)
        {
            var coinEachText = new Coin(sign * profitInCopperEach).CreateCoinText();
            var coinAllText = new Coin(sign * profitInCopperAll).CreateCoinText();
            var tooltip = $"\n\n{tooltipHeader}:";

            if (isSingleItem)
                tooltip += $"\n{coinEachText}";
            else
            {
                tooltip += $"\n{coinAllText} (all)";
                tooltip += $"\n{coinEachText} (each)";
            }

            return tooltip;
        }
    }
}
