using System;

namespace FarmingTracker
{
    public class Coin
    {
        public Coin(int coinsInCopper)
        {
            var sign = Math.Sign(coinsInCopper);
            var unsignedValueInCopper = Math.Abs(coinsInCopper);
            var unsignedGold = unsignedValueInCopper / 10000;
            var unsignedSilver = (unsignedValueInCopper - unsignedGold * 10000) / 100;
            var unsignedCopper = unsignedValueInCopper % 100;

            UnsignedGold = unsignedGold;
            UnsignedSilver = unsignedSilver;
            UnsignedCopper = unsignedCopper;
            Gold = sign * unsignedGold;
            Silver = sign * unsignedSilver;
            Copper = sign * unsignedCopper;
            Sign = sign;
        }

        public int Sign { get; }
        public int UnsignedGold { get; }
        public int UnsignedSilver { get; }
        public int UnsignedCopper { get; }
        public int Gold { get; }
        public int Silver { get; }
        public int Copper { get; }
        public bool HasToDisplayGold => Gold != 0;
        public bool HasToDisplaySilver => Gold != 0 || Silver != 0;
        public bool HasToDisplayCopper => Gold != 0|| Silver != 0 || Copper != 0;
        public const int COIN_CURRENCY_ID = 1;

        public object CreateCoinText()
        {
            var coinText = Sign == -1 ? "-" : "";

            if (Gold != 0)
                coinText += $"{UnsignedGold} g ";

            if (Silver != 0)
                coinText += $"{UnsignedSilver} s ";

            coinText += $"{UnsignedCopper} c";

            return coinText;
        }
    }
}
