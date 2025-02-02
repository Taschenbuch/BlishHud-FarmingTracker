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
            var coin = new Coin(coinStat.Signed_Count.Value);
            
            if (coin.HasToDisplayCopper)
            {
                var copperStat = CreateCoinStat("Copper", coin.Sign * coin.Unsigned_Copper, COPPER_FAKE_API_ID, StatApiDetailsState.CopperCoinCustomStat, localizedCoinName);
                stats.Insert(0, copperStat); 
                // insert() instead of add() because otherwise the custom coins are not at the beginning of favorites anymore.
                // non favorite currencies are automatically sorted by api id, which causes the custom coins to be at the beginning.
                // though that may change in the future.
            }

            if(coin.HasToDisplaySilver) 
            {
                var silverStat = CreateCoinStat("Silver", coin.Sign * coin.Unsigned_Silver, SILVER_FAKE_API_ID, StatApiDetailsState.SilveCoinCustomStat, localizedCoinName);
                stats.Insert(0, silverStat);
            }

            if (coin.HasToDisplayGold)
            {
                var goldStat = CreateCoinStat("Gold", coin.Sign * coin.Unsigned_Gold, GOLD_FAKE_API_ID, StatApiDetailsState.GoldCoinCustomStat, localizedCoinName);
                stats.Insert(0, goldStat);
            }

            stats.Remove(coinStat);
            return stats;
        }

        private static Stat CreateCoinStat(string name, long signed_count, int apiId, StatApiDetailsState statApiDetailsState, string localizedCoinName)
        {
            return new Stat
            {
                ApiId = apiId,
                StatType = StatType.Currency,
                Signed_Count = 
                {
                    Value = signed_count,
                },
                Details =
                {
                    Name = name,
                    WikiSearchTerm = localizedCoinName,
                    State = statApiDetailsState // to get get correct coin texture later
                },
            };
        }

        // large negative number to prevent potential overlap in the future with negative currencyId used as key in StatById dict. 
        public const int GOLD_FAKE_API_ID = -3000;
        public const int SILVER_FAKE_API_ID = -2000;
        public const int COPPER_FAKE_API_ID = -1000;
    }
}
