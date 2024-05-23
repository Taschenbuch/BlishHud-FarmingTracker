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
                var goldStat = CreateCoinStat("Gold", coin.Gold, GOLD_FAKE_API_ID, ApiStatDetailsState.GoldCoinCustomStat);
                currencyById[goldStat.ApiId] = goldStat;
            }

            if(coin.HasToDisplaySilver) 
            {
                var silverStat = CreateCoinStat("Silver", coin.Silver, SILVER_FAKE_API_ID, ApiStatDetailsState.SilveCoinCustomStat);
                currencyById[silverStat.ApiId] = silverStat;
            }
            
            if(coin.HasToDisplayCopper)
            {
                var copperStat = CreateCoinStat("Copper", coin.Copper, COPPER_FAKE_API_ID, ApiStatDetailsState.CopperCoinCustomStat);
                currencyById[copperStat.ApiId] = copperStat;
            }
        }

        private static Stat CreateCoinStat(string name, long count, int apiId, ApiStatDetailsState apiStatDetailsState)
        {
            return new Stat
            {
                ApiId = apiId,
                Count = count,
                Details =
                {
                    Name = name,
                    State = apiStatDetailsState // prevents that module tries to get api details for it.
                },
            };
        }

        // used by enum, do not change value without handling potential issues caused by changing ist
        public const int GOLD_FAKE_API_ID = -3; 
        public const int SILVER_FAKE_API_ID = -2;
        public const int COPPER_FAKE_API_ID = -1;
    }
}
