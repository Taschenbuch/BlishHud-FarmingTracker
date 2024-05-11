﻿using Blish_HUD.Content;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using MonoGame.Extended.BitmapFonts;

namespace FarmingTracker
{
    public class CoinPanel : FlowPanel
    {
        public CoinPanel(int coinIconAssetId, Color textColor, string tooltip, BitmapFont font, Container parent)
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
                AutoSizeWidth = true,
                Parent = this,
            };

            _coinImage = new Image(AsyncTexture2D.FromAssetId(coinIconAssetId))
            {
                Size = new Point(_coinLabel.Height * 11 / 10),
                BasicTooltipText = tooltip,
                Parent = this,
            };
        }

        public void SetValue(int unsignedCoinValue, bool isZeroValueVisible = false)
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
        private const int MAX_COIN_DISPLAY_VALUE = 1_000_000;
    }
}