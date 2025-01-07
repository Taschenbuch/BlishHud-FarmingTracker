using System.Collections.Generic;
using System.Linq;

namespace FarmingTracker
{
    public class CoinSplitter
    {
        public static List<Stat> ReplaceCoinWithGoldSilverCopperStats(List<Stat> stats)
        {
            var coinStat = stats.SingleOrDefault(c => c.IsCoin);
            if (coinStat == null) // no coins earned or lost
                return stats;

            var localizedCoinName = coinStat.Details.Name; // for wiki because "coin" in spanish will not work in english wiki
            var coin = new Coin(coinStat.Signed_Count);
            
            if (coin.HasToDisplayCopper)
            {
                var copperStat = CreateCoinStat("Copper", coin.Sign * coin.Unsigned_Copper, COPPER_FAKE_API_ID, ApiStatDetailsState.CopperCoinCustomStat, localizedCoinName);
                stats.Insert(0, copperStat); 
                // insert() instead of add() because otherwise the custom coins are not at the beginning of favorites anymore.
                // non favorite currencies are automatically sorted by api id, which causes the custom coins to be at the beginning.
                // though that may change in the future.
            }

            if(coin.HasToDisplaySilver) 
            {
                var silverStat = CreateCoinStat("Silver", coin.Sign * coin.Unsigned_Silver, SILVER_FAKE_API_ID, ApiStatDetailsState.SilveCoinCustomStat, localizedCoinName);
                stats.Insert(0, silverStat);
            }

            if (coin.HasToDisplayGold)
            {
                var goldStat = CreateCoinStat("Gold", coin.Sign * coin.Unsigned_Gold, GOLD_FAKE_API_ID, ApiStatDetailsState.GoldCoinCustomStat, localizedCoinName);
                stats.Insert(0, goldStat);
            }

            stats.Remove(coinStat);
            return stats;
        }

        private static Stat CreateCoinStat(string name, long signed_count, int apiId, ApiStatDetailsState apiStatDetailsState, string localizedCoinName)
        {
            return new Stat
            {
                ApiId = apiId,
                StatType = StatType.Currency,
                Signed_Count = signed_count,
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
