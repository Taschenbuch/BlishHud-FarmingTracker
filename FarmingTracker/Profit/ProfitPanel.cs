using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using MonoGame.Extended.BitmapFonts;

namespace FarmingTracker
{
    public class ProfitPanel : FlowPanel
    {
        public ProfitPanel(string suffixText, string tooltip, BitmapFont font, Services services, Container parent)
        {
            FlowDirection = ControlFlowDirection.SingleLeftToRight;
            ControlPadding = new Vector2(5, 0);
            BasicTooltipText = tooltip;
            WidthSizingMode = SizingMode.AutoSize;
            HeightSizingMode = SizingMode.AutoSize;
            Parent = parent;

            _rootPanel = new FlowPanel
            {
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
                ControlPadding = new Vector2(5, 0),
                BasicTooltipText = tooltip,
                WidthSizingMode = SizingMode.AutoSize,
                HeightSizingMode = SizingMode.AutoSize,
                Parent = this,
            };

            _signLabel = new CoinSignLabel(tooltip, font, _rootPanel);
            _goldPanel = new CoinPanel(services.TextureService.SmallGoldCoinTexture, Color.Gold, tooltip, font, _rootPanel);
            _silverPanel = new CoinPanel(services.TextureService.SmallSilverCoinTexture, Color.LightGray, tooltip, font, _rootPanel);
            _copperPanel = new CoinPanel(services.TextureService.SmallCopperCoinTexture, Color.SandyBrown, tooltip, font, _rootPanel);

            new Label
            {
                Text = suffixText,
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

            _signLabel.SetSign(coin.Sign); // always show sign label
            // order of setting gold, silver, copper is important because parent is flowpanel!
            _goldPanel.SetValue(coin.UnsignedGold, false);
            _silverPanel.SetValue(coin.UnsignedSilver, coin.UnsignedGold != 0);
            _copperPanel.SetValue(coin.UnsignedCopper, true); // always show copper
        }

        private readonly FlowPanel _rootPanel;
        private readonly CoinSignLabel _signLabel;
        private readonly CoinPanel _goldPanel;
        private readonly CoinPanel _silverPanel;
        private readonly CoinPanel _copperPanel;
    }
}
