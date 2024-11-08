using System;

namespace FarmingTracker
{
    public class Coin
    {
        public Coin(long signed_coinsInCopper)
        {
            var unsigned_coinsInCopper = Math.Abs(signed_coinsInCopper);

            Sign = Math.Sign(signed_coinsInCopper);
            Unsigned_Gold = unsigned_coinsInCopper / 10000;
            Unsigned_Silver = (unsigned_coinsInCopper % 10000) / 100;
            Unsigned_Copper = unsigned_coinsInCopper % 100;
        }

        public long Sign { get; }
        public long Unsigned_Gold { get; }
        public long Unsigned_Silver { get; }
        public long Unsigned_Copper { get; }
        public bool HasToDisplayGold => Unsigned_Gold != 0;
        public bool HasToDisplaySilver => Unsigned_Gold != 0 || Unsigned_Silver != 0;
        public bool HasToDisplayCopper => Unsigned_Gold != 0|| Unsigned_Silver != 0 || Unsigned_Copper != 0;
        public const int COIN_CURRENCY_ID = 1;

        public object CreateCoinText()
        {
            var coinText = Sign == -1 ? "-" : "";

            if (Unsigned_Gold != 0)
                coinText += $"{Unsigned_Gold} g ";

            if (Unsigned_Silver != 0)
                coinText += $"{Unsigned_Silver} s ";

            coinText += $"{Unsigned_Copper} c";

            return coinText;
        }
    }
}
