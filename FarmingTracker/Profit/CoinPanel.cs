using Blish_HUD.Content;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using MonoGame.Extended.BitmapFonts;

namespace FarmingTracker
{
    public class CoinPanel : FlowPanel
    {
        public CoinPanel(AsyncTexture2D coinTexture, Color textColor, string tooltip, BitmapFont font, bool widthFixed, Container parent)
        {
            _parent = parent;

            FlowDirection = ControlFlowDirection.SingleLeftToRight;
            WidthSizingMode = SizingMode.AutoSize;
            HeightSizingMode = SizingMode.AutoSize;
            Parent = parent;

            _coinLabel = new Label
            {
                Text = "?", // not "0" to see difference between initialisation and setting of 0.
                BasicTooltipText = tooltip,
                Font = font,
                TextColor = textColor,
                AutoSizeHeight = true,
                HorizontalAlignment = HorizontalAlignment.Right, // for better alignment of silver and copper
                Parent = this,
            };

            if(widthFixed)
            {
                _coinLabel.Width = 20; // for better alignment of silver and copper
            }
            else
            {
                _coinLabel.AutoSizeWidth = true;
            }


            _coinImage = new Image(coinTexture)
            {
                Size = new Point(_coinLabel.Height * 11 / 10),
                BasicTooltipText = tooltip,
                Parent = this,
            };
        }

        // coin parameter is gold OR silver OR copper.
        public void SetValue(long unsignedCoinValue, bool isZeroValueVisible)
        {
            Parent = null;

            if (isZeroValueVisible || unsignedCoinValue != 0)
            {
                _coinLabel.Text =  unsignedCoinValue > MAX_COIN_DISPLAY_VALUE 
                    ? $">{MAX_COIN_DISPLAY_VALUE}" 
                    : unsignedCoinValue.ToString();

                _coinLabel.Parent = this;
                _coinImage.Parent = this;
                Parent = _parent;
            }
        }

        private readonly Label _coinLabel;
        private readonly Image _coinImage;
        private readonly Container _parent;
        private const long MAX_COIN_DISPLAY_VALUE = 1_000_000;
    }
}
