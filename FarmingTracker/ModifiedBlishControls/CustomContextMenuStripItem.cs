using System;
using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarmingTracker 
{
    // Custom to be able to change the indention of the bullet point. Removed all stuff that is not used
    public class CustomContextMenuStripItem : Control 
    {
        private const int BULLET_SIZE        = 18;
        private const int HORIZONTAL_PADDING = 6;
        private const int TEXT_LEFTPADDING = HORIZONTAL_PADDING + BULLET_SIZE + HORIZONTAL_PADDING;
        private const string NOT_CLICKABLE_HEADER_TOOLTIP = "Header cannot be clicked. Click on one of the other entries instead.";
        private readonly AsyncTexture2D _textureBullet = AsyncTexture2D.FromAssetId(155038);
        private readonly bool _bulletIsVisible;
        private string _text = string.Empty;

        public string Text {
            get => _text;
            set => SetProperty(ref _text, value, true);
        }

        public CustomContextMenuStripItem(string text, Container parent, bool isHeader = false) 
        {
            EffectBehind = new Blish_HUD.Controls.Effects.ScrollingHighlightEffect(this); // this is yellow mouse over highlight.
            Text = text;
            Parent = parent;
            Enabled = !isHeader;
            _bulletIsVisible = !isHeader;

            if (isHeader)
                BasicTooltipText = NOT_CLICKABLE_HEADER_TOOLTIP;
        }

        public override void RecalculateLayout() 
        {
            var textSize = GameService.Content.DefaultFont14.MeasureString(_text);
            int nWidth   = (int)textSize.Width + TEXT_LEFTPADDING + TEXT_LEFTPADDING;

            var parent = Parent;

            if (parent != null) {
                Width = Math.Max(parent.Width - 4, nWidth);
            } else {
                Width = nWidth;
            }
        }

        protected override void OnClick(MouseEventArgs e) 
        {
            // It is important that we handle this first to avoid mistakenly removing
            // the handlers if the control is disposed of during the click.
            base.OnClick(e);
            Parent?.Hide();
        }

        protected override void OnMouseEntered(MouseEventArgs e) 
        {
            base.OnMouseEntered(e);
        }

        protected override void OnMouseLeft(MouseEventArgs e) 
        {
            base.OnMouseLeft(e);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) 
        {
            var modifierTint = Enabled 
                ? MouseOver
                    ? StandardColors.Tinted
                    : StandardColors.Default
                : StandardColors.DisabledText;

            if(_bulletIsVisible)
                spriteBatch.DrawOnCtrl(
                    this,
                    _textureBullet,
                    new Rectangle(HORIZONTAL_PADDING, _size.Y / 2 - BULLET_SIZE / 2, BULLET_SIZE, BULLET_SIZE),
                    modifierTint);

            var textLeftPadding = _bulletIsVisible ? TEXT_LEFTPADDING : HORIZONTAL_PADDING;

            // Draw shadow
            spriteBatch.DrawStringOnCtrl(
                this,
                _text,
                Content.DefaultFont14,
                new Rectangle(textLeftPadding + 1, 0 + 1, _size.X - TEXT_LEFTPADDING - HORIZONTAL_PADDING, _size.Y),
                StandardColors.Shadow);

            spriteBatch.DrawStringOnCtrl(
                this,
                _text,
                Content.DefaultFont14,
                new Rectangle(textLeftPadding, 0, _size.X - TEXT_LEFTPADDING - HORIZONTAL_PADDING, _size.Y),
                _enabled ? StandardColors.Default : StandardColors.DisabledText);
        }
    }
}