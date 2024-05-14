using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using MonoGame.Extended.BitmapFonts;

namespace FarmingTracker
{
    public class ProfitPanel : FlowPanel
    {
        public ProfitPanel(string suffixText, string tooltip, BitmapFont font, Container parent)
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
            _goldPanel = new CoinPanel(Constants.GOLD_ICON_ASSET_ID, Color.Gold, tooltip, font, _rootPanel);
            _silverPanel = new CoinPanel(Constants.SILVER_ICON_ASSET_ID, Color.LightGray, tooltip, font, _rootPanel);
            _copperPanel = new CoinPanel(Constants.COPPER_ICON_ASSET_ID, Color.SandyBrown, tooltip, font, _rootPanel);

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

        public void SetProfit(int profitInCopper)
        {
            var coin = new Coin(profitInCopper);

            _signLabel.SetSign(coin.SignFaktor); // always show sign label
            // order of setting gold, silver, copper is important because parent is flowpanel!
            _goldPanel.SetValue(coin.UnsignedGold);
            _silverPanel.SetValue(coin.UnsignedSilver);
            _copperPanel.SetValue(coin.UnsignedCopper, true); // always show copper
        }

        private readonly FlowPanel _rootPanel;
        private readonly CoinSignLabel _signLabel;
        private readonly CoinPanel _goldPanel;
        private readonly CoinPanel _silverPanel;
        private readonly CoinPanel _copperPanel;
    }
}
