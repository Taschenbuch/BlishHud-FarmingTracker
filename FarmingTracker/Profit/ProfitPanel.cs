using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using MonoGame.Extended.BitmapFonts;

namespace FarmingTracker
{
    public class ProfitPanel : FlowPanel
    {
        public ProfitPanel(string suffixText, string tooltip, BitmapFont font, TextureService textureService, Container parent, int height = 0)
        {
            FlowDirection = ControlFlowDirection.SingleLeftToRight;
            BasicTooltipText = tooltip;
            WidthSizingMode = SizingMode.AutoSize;

            if(height > 0)
                Height = height;
            else
                HeightSizingMode = SizingMode.AutoSize;

            Parent = parent;

            _signLabel = new CoinSignLabel(tooltip, font, this);

            _coinsFlowPanel = new FlowPanel // this does not include the suffix Text Label
            {
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
                ControlPadding = new Vector2(5, 0),
                BasicTooltipText = tooltip,
                WidthSizingMode = SizingMode.AutoSize,
                HeightSizingMode = SizingMode.AutoSize,
                Parent = this,
            };

            _goldPanel = new CoinPanel(textureService.SmallGoldCoinTexture, Color.Gold, tooltip, font, false, _coinsFlowPanel);
            _silverPanel = new CoinPanel(textureService.SmallSilverCoinTexture, Color.LightGray, tooltip, font, true, _coinsFlowPanel);
            _copperPanel = new CoinPanel(textureService.SmallCopperCoinTexture, Color.SandyBrown, tooltip, font, true, _coinsFlowPanel);

            var hasSuffixText = !string.IsNullOrWhiteSpace(suffixText);
            if(hasSuffixText)
                new Label
                {
                    Text = $" {suffixText}", // add padding here instead of using flowPanel.Controlpadidng because sign label must not have padding.
                    Font = font,
                    BasicTooltipText = tooltip,
                    AutoSizeHeight = true,
                    AutoSizeWidth = true,
                    Parent = this,
                };
        }

        public void SetProfit(long profitInCopper)
        {
            var coin = new Coin(profitInCopper);

            _signLabel.SetSign(coin.Sign); // always show sign label to prevent moving it to the end accidently
            // order of setting gold, silver, copper is important because parent is flowpanel!
            _goldPanel.SetValue(coin.UnsignedGold, false);
            _silverPanel.SetValue(coin.UnsignedSilver, coin.UnsignedGold != 0);
            _copperPanel.SetValue(coin.UnsignedCopper, true); // always show copper
        }

        protected override void DisposeControl()
        {
            // because those may not have a parent set
            _goldPanel?.Dispose();
            _silverPanel?.Dispose();
            _copperPanel?.Dispose();

            base.DisposeControl();
        }

        private readonly FlowPanel _coinsFlowPanel;
        private readonly CoinSignLabel _signLabel;
        private readonly CoinPanel _goldPanel;
        private readonly CoinPanel _silverPanel;
        private readonly CoinPanel _copperPanel;
    }
}
