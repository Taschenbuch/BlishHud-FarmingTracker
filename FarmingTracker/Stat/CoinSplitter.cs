using System.Collections.Generic;

namespace FarmingTracker
{
    public class CoinSplitter
    {
        public static void SplitCoinIntoGoldSilverCopperStats(Dictionary<int, Stat> currencyById)
        {
            // do NOT remove coin from currencyById!
            currencyById.Remove(GOLD_FAKE_ID);
            currencyById.Remove(SILVER_FAKE_ID);
            currencyById.Remove(COPPER_FAKE_ID);

            var hasEarnedOrLostCoin = currencyById.TryGetValue(Coin.COIN_CURRENCY_ID, out var coinsInCopperItem);
            if (!hasEarnedOrLostCoin)
                return;

            var coin = new Coin(coinsInCopperItem.Count);

            if(coin.HasToDisplayGold)
            {
                var goldStat = new Stat
                {
                    ApiId = GOLD_FAKE_ID,
                    Count = coin.Gold,
                    Details =
                    {
                        Name = "Gold",
                        IconAssetId = Constants.GOLD_ICON_ASSET_ID,
                        State = ApiStatDetailsState.SetByApi // prevents that module tries to get api details for it
                    },
                };
                
                currencyById[goldStat.ApiId] = goldStat;
            }

            if(coin.HasToDisplaySilver) 
            {
                var silverStat = new Stat
                {
                    ApiId = SILVER_FAKE_ID,
                    Count = coin.Silver,
                    Details =
                    { 
                        Name = "Silver",
                        IconAssetId = Constants.SILVER_ICON_ASSET_ID,
                        State = ApiStatDetailsState.SetByApi // prevents that module tries to get api details for it
                    },
                };

                currencyById[silverStat.ApiId] = silverStat;
            }
            
            if(coin.HasToDisplayCopper) 
            {
                var copperStat = new Stat
                {
                    ApiId = COPPER_FAKE_ID,
                    Count = coin.Copper,
                    Details =
                    {
                        Name = "Copper",
                        IconAssetId = Constants.COPPER_ICON_ASSET_ID,
                        State = ApiStatDetailsState.SetByApi // prevents that module tries to get api details for it
                    },
                };

                currencyById[copperStat.ApiId] = copperStat;
            }
        }

        private const int GOLD_FAKE_ID = -3;
        private const int SILVER_FAKE_ID = -2;
        private const int COPPER_FAKE_ID = -1;
    }
}
