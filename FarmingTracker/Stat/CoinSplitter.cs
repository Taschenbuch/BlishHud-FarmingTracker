using System.Collections.Generic;

namespace FarmingTracker
{
    public class CoinSplitter
    {
        public static void SplitCoinIntoGoldSilverCopperStats(Dictionary<int, Stat> currencyById)
        {
            // do NOT remove coin from currencyById! Removing coin would reset the coin count to 0 before every stats update!
            currencyById.Remove(GOLD_FAKE_API_ID);
            currencyById.Remove(SILVER_FAKE_API_ID);
            currencyById.Remove(COPPER_FAKE_API_ID);

            var hasEarnedOrLostCoin = currencyById.TryGetValue(Coin.COIN_CURRENCY_ID, out var coinsInCopperItem);
            if (!hasEarnedOrLostCoin)
                return;

            var coin = new Coin(coinsInCopperItem.Count);

            if(coin.HasToDisplayGold)
            {
                var goldStat = CreateCoinStat("Gold", coin.Gold, GOLD_FAKE_API_ID, Constants.GOLD_ICON_ASSET_ID);
                currencyById[goldStat.ApiId] = goldStat;
            }

            if(coin.HasToDisplaySilver) 
            {
                var silverStat = CreateCoinStat("Silver", coin.Silver, SILVER_FAKE_API_ID, Constants.SILVER_ICON_ASSET_ID);
                currencyById[silverStat.ApiId] = silverStat;
            }
            
            if(coin.HasToDisplayCopper)
            {
                var copperStat = CreateCoinStat("Copper", coin.Copper, COPPER_FAKE_API_ID, Constants.COPPER_ICON_ASSET_ID);
                currencyById[copperStat.ApiId] = copperStat;
            }
        }

        private static Stat CreateCoinStat(string name, long count, int apiId, int iconAssetId)
        {
            return new Stat
            {
                ApiId = apiId,
                Count = count,
                Tooltip = "Changes in 'raw gold'.\nIn other words coins spent or gained.",
                Details =
                    {
                        Name = name,
                        IconAssetId = iconAssetId,
                        State = ApiStatDetailsState.SetByApi // prevents that module tries to get api details for it.
                    },
            };
        }

        // used by enum, do not change value without handling potential issues caused by changing ist
        public const int GOLD_FAKE_API_ID = -3; 
        public const int SILVER_FAKE_API_ID = -2;
        public const int COPPER_FAKE_API_ID = -1;
    }
}
