using System;

namespace FarmingTracker
{
    public class Coin
    {
        public Coin(long coinsInCopper)
        {
            var unsignedCoinsInCopper = Math.Abs(coinsInCopper);

            Sign = Math.Sign(coinsInCopper);
            Gold = unsignedCoinsInCopper / 10000;
            Silver = (unsignedCoinsInCopper % 10000) / 100;
            Copper = unsignedCoinsInCopper % 100;
        }

        public long Sign { get; }
        public long Gold { get; }
        public long Silver { get; }
        public long Copper { get; }
        public bool HasToDisplayGold => Gold != 0;
        public bool HasToDisplaySilver => Gold != 0 || Silver != 0;
        public bool HasToDisplayCopper => Gold != 0|| Silver != 0 || Copper != 0;
        public const int COIN_CURRENCY_ID = 1;

        public object CreateCoinText()
        {
            var coinText = Sign == -1 ? "-" : "";

            if (Gold != 0)
                coinText += $"{Gold} g ";

            if (Silver != 0)
                coinText += $"{Silver} s ";

            coinText += $"{Copper} c";

            return coinText;
        }
    }
}
