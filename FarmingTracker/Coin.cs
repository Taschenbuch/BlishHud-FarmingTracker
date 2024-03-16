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
    }
}
