using Blish_HUD.Controls;
using MonoGame.Extended.BitmapFonts;

namespace FarmingTracker
{
    public class CoinSignLabel : Label
    {
        public CoinSignLabel(Tooltip tooltip, BitmapFont font, Container parent)
        {
            Text = Constants.FULL_HEIGHT_EMPTY_LABEL;
            Font = font;
            Tooltip = tooltip;
            AutoSizeHeight = true;
            AutoSizeWidth = true;
            Parent = parent;
        }

        internal void SetSign(long sign)
        {
            Text = sign == -1
                ? "-" 
                : Constants.FULL_HEIGHT_EMPTY_LABEL;
        }
    }
}
