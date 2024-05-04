using System;

namespace FarmingTracker
{
    public class Coin
    {
        public Coin(int coinsInCopper)
        {
            var signFactor = coinsInCopper < 0 ? -1 : 1;
            var unsignedValueInCopper = Math.Abs(coinsInCopper);
            var unsignedGold = unsignedValueInCopper / 10000;
            var unsignedSilver = (unsignedValueInCopper - unsignedGold * 10000) / 100;
            var unsignedCopper = unsignedValueInCopper % 100;

            Gold = signFactor * unsignedGold;
            Silver = signFactor * unsignedSilver;
            Copper = signFactor * unsignedCopper;
        }

        public int Gold { get; }
        public int Silver { get; }
        public int Copper { get; }
        public bool HasToDisplayGold => Gold != 0;
        public bool HasToDisplaySilver => Gold != 0 || Silver != 0;
        public bool HasToDisplayCopper => Gold != 0|| Silver != 0 || Copper != 0;
        
        public const int COIN_CURRENCY_ID = 1;
    }
}
