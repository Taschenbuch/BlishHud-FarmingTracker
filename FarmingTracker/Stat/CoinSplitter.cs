using System.Collections.Generic;
using System.Linq;

namespace FarmingTracker
{
    public class CoinSplitter
    {
        public static List<Stat> ReplaceCoinWithGoldSilverCopperStats(List<Stat> currencies)
        {
            var coinStat = currencies.SingleOrDefault(c => c.IsCoin);
            var noCoinsEarnedOrLost = coinStat == null;
            if (noCoinsEarnedOrLost)
                return currencies;

            var localizedCoinName = coinStat.Details.Name; // for wiki because "coin" in spanish will not work in english wiki
            var coin = new Coin(coinStat.Count);

            if(coin.HasToDisplayGold)
            {
                var goldStat = CreateCoinStat("Gold", coin.Gold, GOLD_FAKE_API_ID, ApiStatDetailsState.GoldCoinCustomStat, localizedCoinName);
                currencies.Add(goldStat);
            }

            if(coin.HasToDisplaySilver) 
            {
                var silverStat = CreateCoinStat("Silver", coin.Silver, SILVER_FAKE_API_ID, ApiStatDetailsState.SilveCoinCustomStat, localizedCoinName);
                currencies.Add(silverStat);
            }
            
            if(coin.HasToDisplayCopper)
            {
                var copperStat = CreateCoinStat("Copper", coin.Copper, COPPER_FAKE_API_ID, ApiStatDetailsState.CopperCoinCustomStat, localizedCoinName);
                currencies.Add(copperStat);
            }
            
            currencies.Remove(coinStat);
            return currencies;
        }

        private static Stat CreateCoinStat(string name, long count, int apiId, ApiStatDetailsState apiStatDetailsState, string localizedCoinName)
        {
            return new Stat
            {
                ApiId = apiId,
                Count = count,
                Details =
                {
                    Name = name,
                    WikiSearchTerm = localizedCoinName,
                    State = apiStatDetailsState // to get get correct coin texture later
                },
            };
        }

        // used by enum, do not change value without handling potential issues caused by changing ist
        public const int GOLD_FAKE_API_ID = -3; 
        public const int SILVER_FAKE_API_ID = -2;
        public const int COPPER_FAKE_API_ID = -1;
    }
}
