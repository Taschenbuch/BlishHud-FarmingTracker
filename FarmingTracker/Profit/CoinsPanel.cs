using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using MonoGame.Extended.BitmapFonts;

namespace FarmingTracker
{
    public class CoinsPanel : FlowPanel
    {
        public CoinsPanel(Tooltip? tooltip, BitmapFont font, TextureService textureService, Container parent, int height = 0)
        {
            FlowDirection = ControlFlowDirection.SingleLeftToRight;
            Tooltip = tooltip;
            WidthSizingMode = SizingMode.AutoSize;

            if(height > 0)
                Height = height;
            else
                HeightSizingMode = SizingMode.AutoSize;

            Parent = parent;

            _signLabel = new CoinSignLabel(tooltip, font, this);

            var coinsFlowPanel = new FlowPanel // this does not include the suffix Text Label
            {
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
                ControlPadding = new Vector2(5, 0),
                Tooltip = tooltip,
                WidthSizingMode = SizingMode.AutoSize,
                HeightSizingMode = SizingMode.AutoSize,
                Parent = this,
            };

            _goldPanel = new CoinPanel(textureService.SmallGoldCoinTexture, Color.Gold, tooltip, font, false, coinsFlowPanel);
            _silverPanel = new CoinPanel(textureService.SmallSilverCoinTexture, Color.LightGray, tooltip, font, true, coinsFlowPanel);
            _copperPanel = new CoinPanel(textureService.SmallCopperCoinTexture, Color.SandyBrown, tooltip, font, true, coinsFlowPanel);
        }

        public void SetCoins(long coinsInCopper)
        {
            var coin = new Coin(coinsInCopper);

            _signLabel.SetSign(coin.Sign); // always show sign label to prevent moving it to the end accidently
            // order of setting gold, silver, copper is important because parent is flowpanel!
            _goldPanel.SetValue(coin.Gold, false);
            _silverPanel.SetValue(coin.Silver, coin.Gold > 0);
            _copperPanel.SetValue(coin.Copper, true); // always show copper
        }

        protected override void DisposeControl()
        {
            // because those may not have a parent set
            _goldPanel?.Dispose();
            _silverPanel?.Dispose();
            _copperPanel?.Dispose();

            base.DisposeControl();
        }

        private readonly CoinSignLabel _signLabel;
        private readonly CoinPanel _goldPanel;
        private readonly CoinPanel _silverPanel;
        private readonly CoinPanel _copperPanel;
    }
}
