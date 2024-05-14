using Blish_HUD.Controls;
using MonoGame.Extended.BitmapFonts;

namespace FarmingTracker
{
    public class CoinSignLabel : Label
    {
        public CoinSignLabel(string tooltip, BitmapFont font, Container parent)
        {
            Text = Constants.EMPTY_LABEL;
            Font = font;
            BasicTooltipText = tooltip;
            AutoSizeHeight = true;
            AutoSizeWidth = true;
            Parent = parent;
        }

        internal void SetSign(int signFaktor)
        {
            Text = signFaktor == -1
                ? "-" 
                : Constants.EMPTY_LABEL;
        }
    }
}
