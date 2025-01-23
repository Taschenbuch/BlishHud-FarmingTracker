using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;

namespace FarmingTracker
{
    public class StatContainer : Control
    {
        public StatContainer(
            Stat stat, 
            PanelType panelType, 
            Model model, 
            Services services)
        {
            _services = services;
            
            _countText = stat.Signed_Count.ToString();
            var countFont = services.FontService.Fonts[services.SettingService.CountFontSizeSetting.Value];
            _countFont = countFont;
            _countBackgroundColor = services.SettingService.CountBackgroundColorSetting.Value.GetColor() * (services.SettingService.CountBackgroundOpacitySetting.Value / 255f);
            _countColor = stat.Signed_Count >= 0
                ? services.SettingService.PositiveCountTextColorSetting.Value.GetColor()
                : services.SettingService.NegativeCountTextColorSetting.Value.GetColor();
            
            _inventorySlotTexture = services.TextureService.InventorySlotBackgroundTexture;
            _statIconTexture = GetStatIconTexture(stat, services);
            _rarityBorderColor = ColorService.GetRarityBorderColor(stat.Details.Rarity);
            _statIconOpacity = stat.Signed_Count > 0 
                ? 1f 
                : (services.SettingService.NegativeCountIconOpacitySetting.Value / 255f);

            Tooltip = new StatTooltip(stat, _statIconTexture, panelType, services);

            var statIconOrigin = INVENTORY_SLOT_MARGIN + STAT_ICON_MARGIN;
            var statIconSize = (int)services.SettingService.StatIconSizeSetting.Value;
            var inventorySlotSize = statIconSize + 2 * STAT_ICON_MARGIN;
            Size = new Point(inventorySlotSize + 2 * INVENTORY_SLOT_MARGIN);

            _rarityBorderLength = inventorySlotSize;
            _inventorySlotBounds = new Rectangle(INVENTORY_SLOT_MARGIN, INVENTORY_SLOT_MARGIN, inventorySlotSize, inventorySlotSize);
            _statIconBounds = new Rectangle(statIconOrigin, statIconOrigin, statIconSize, statIconSize);
            _countTextBounds = new Rectangle(statIconOrigin, statIconOrigin + 1, statIconSize - 5, statIconSize - 2);
            _countBackgroundBounds = new Rectangle(statIconOrigin, statIconOrigin, statIconSize, countFont.LineHeight);
          
            if (panelType != PanelType.IgnoredStats)
                RightMouseButtonPressed += (s, e) =>
                {
                    var contextMenuStrip = new StatContextMenuStrip(stat, panelType, model, services);
                    contextMenuStrip.Hidden += (s, e) => contextMenuStrip.Dispose();
                    contextMenuStrip.Show(GameService.Input.Mouse.Position);
                };
        }

        private static AsyncTexture2D GetStatIconTexture(Stat stat, Services services)
        {
            return stat.Details.State switch
            {
                ApiStatDetailsState.GoldCoinCustomStat => services.TextureService.GoldCoinTexture,
                ApiStatDetailsState.SilveCoinCustomStat => services.TextureService.SilverCoinTexture,
                ApiStatDetailsState.CopperCoinCustomStat => services.TextureService.CopperCoinTexture,
                _ => services.TextureService.GetTextureFromAssetCacheOrFallback(stat.Details.IconAssetId),
            };
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            DrawInventorySlot(spriteBatch);
            DrawStatIcon(spriteBatch);
            DrawCountBackground(spriteBatch); // before rarity border to prevent overlaying rarity border, because rarity border is thicker than 1 pixel and thus overlaps with the stat icon
            DrawCountText(spriteBatch);

            if (_services.SettingService.RarityIconBorderIsVisibleSetting.Value)
                DrawRarityBorder(spriteBatch);
        }

        private void DrawInventorySlot(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawOnCtrl(this, _inventorySlotTexture, _inventorySlotBounds, _inventorySlotTexture.Texture.Bounds, Color.White, 0f, Vector2.Zero, default);
        }

        private void DrawStatIcon(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawOnCtrl(this, _statIconTexture, _statIconBounds, _statIconTexture.Texture.Bounds, Color.White * _statIconOpacity, 0f, Vector2.Zero, default);
        }

        private void DrawCountBackground(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, _countBackgroundBounds, Rectangle.Empty, _countBackgroundColor);
        }

        private void DrawCountText(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawStringOnCtrl(
                this,
                _countText,
                _countFont,
                _countTextBounds,
                _countColor,
                false,
                true,
                1,
                _services.SettingService.CountHoritzontalAlignmentSetting.Value,
                VerticalAlignment.Top);
        }

        private void DrawRarityBorder(SpriteBatch spriteBatch)
        {
            var thickness = 2;
            var left = INVENTORY_SLOT_MARGIN;
            var top = INVENTORY_SLOT_MARGIN;
            var right = left + _rarityBorderLength - thickness;
            var bottom = top + _rarityBorderLength - thickness;

            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(left, top, _rarityBorderLength, thickness), Rectangle.Empty, _rarityBorderColor);    // Top
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(left, top, thickness, _rarityBorderLength), Rectangle.Empty, _rarityBorderColor);    // Left
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(left, bottom, _rarityBorderLength, thickness), Rectangle.Empty, _rarityBorderColor); // Bottom
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(right, top, thickness, _rarityBorderLength), Rectangle.Empty, _rarityBorderColor);   // Right
        }

        protected override void DisposeControl()
        {
            Tooltip?.Dispose();
            base.DisposeControl();
        }

        private readonly Services _services;
        private readonly string _countText;
        private readonly BitmapFont _countFont;
        private readonly Color _countColor;
        private readonly AsyncTexture2D _inventorySlotTexture;
        private readonly Color _countBackgroundColor;
        private readonly Rectangle _countTextBounds;
        private readonly Rectangle _countBackgroundBounds;
        private readonly Color _rarityBorderColor;
        private readonly int _rarityBorderLength;
        private readonly float _statIconOpacity;
        private readonly AsyncTexture2D _statIconTexture;
        private readonly Rectangle _inventorySlotBounds;
        private readonly Rectangle _statIconBounds;
        private const int STAT_ICON_MARGIN = 1;
        private const int INVENTORY_SLOT_MARGIN = 1;
    }
}
