using Blish_HUD.Content;
using Blish_HUD.Modules.Managers;
using Gw2Sharp.WebApi;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;

namespace FarmingTracker
{
    public class TextureService : IDisposable
    {
        public TextureService(ContentsManager contentsManager)
        {
            // dispose necessary:
            WindowEmblemTexture = contentsManager.GetTexture("window-emblem.png");
            DrfTexture = contentsManager.GetTexture("drf.png");
            FilterTabIconTexture = contentsManager.GetTexture("filter-tab-icon.png");
            SortTabIconTexture = contentsManager.GetTexture("sort-tab-icon.png");
            TimelineTabIconTexture = contentsManager.GetTexture("timeline-tab-icon.png");
            SummaryTabIconTexture = contentsManager.GetTexture("summary-tab-icon.png");
            CustomStatProfitTabIconTexture = contentsManager.GetTexture("custom-stat-profit-tab-icon.png");
            IgnoredItemsTabIconTexture = contentsManager.GetTexture("ignored-items-tab-icon.png");
            IgnoredItemsPanelIconTexture = contentsManager.GetTexture("ignored-items-panel-icon.png");
            CornerIconTexture = contentsManager.GetTexture("corner-icon.png");
            CornerIconHoverTexture = contentsManager.GetTexture("corner-icon-hover.png");
            GoldCoinTexture = contentsManager.GetTexture("coin-gold.png");
            SilverCoinTexture = contentsManager.GetTexture("coin-silver.png");
            CopperCoinTexture = contentsManager.GetTexture("coin-copper.png");
            OpenLinkTexture = contentsManager.GetTexture("open-link.png");
            FallbackTexture = contentsManager.GetTexture("fallback_157084.png");
            DebugTabIconTexture = contentsManager.GetTexture("debug-tab-icon.png");

            // NO dispose allowed:
            SmallGoldCoinTexture = GetTextureFromAssetCacheOrFallback(156904);
            SmallSilverCoinTexture = GetTextureFromAssetCacheOrFallback(156907);
            SmallCopperCoinTexture = GetTextureFromAssetCacheOrFallback(156902);
            SettingsTabIconTexture = GetTextureFromAssetCacheOrFallback(156737);
            WindowBackgroundTexture = GetTextureFromAssetCacheOrFallback(155997);
            InventorySlotBackgroundTexture = GetTextureFromAssetCacheOrFallback(1318622);
            MerchantTexture = GetTextureFromAssetCacheOrFallback(156761);
            ItemsTexture = GetTextureFromAssetCacheOrFallback(157098);
            FavoriteTexture = GetTextureFromAssetCacheOrFallback(156331);
            TradingPostTexture = GetTextureFromAssetCacheOrFallback(255379);
        }

        public void Dispose()
        {
            WindowEmblemTexture?.Dispose();
            DrfTexture?.Dispose();
            FilterTabIconTexture?.Dispose();
            SortTabIconTexture?.Dispose();
            TimelineTabIconTexture?.Dispose();
            SummaryTabIconTexture?.Dispose();
            CustomStatProfitTabIconTexture?.Dispose();
            IgnoredItemsTabIconTexture?.Dispose();
            IgnoredItemsPanelIconTexture?.Dispose();
            CornerIconTexture?.Dispose();
            CornerIconHoverTexture?.Dispose();
            GoldCoinTexture?.Dispose();
            SilverCoinTexture?.Dispose();
            CopperCoinTexture?.Dispose();
            OpenLinkTexture?.Dispose();
            FallbackTexture?.Dispose();
            DebugTabIconTexture?.Dispose();
        }

        public static int GetIconAssetId(RenderUrl icon)
        {
            return int.Parse(Path.GetFileNameWithoutExtension(icon.Url.AbsoluteUri));
        }

        public AsyncTexture2D GetTextureFromAssetCacheOrFallback(int assetId)
        {
            if (assetId == 0) // happens regularily for stats unknown by gw2 api. Prevents that log is spammed with log messages from blish core AsyncTexture2D.TryFromAssetId(). 
                return FallbackTexture;

            try
            {
                if (AsyncTexture2D.TryFromAssetId(assetId, out AsyncTexture2D texture))
                    return texture;
            }
            catch (Exception)
            {
                return FallbackTexture;
            }

            return FallbackTexture;
        }

        public Texture2D WindowEmblemTexture { get; }
        public Texture2D DrfTexture { get; }
        public Texture2D FilterTabIconTexture { get; }
        public Texture2D SortTabIconTexture { get; }
        public Texture2D TimelineTabIconTexture { get; }
        public Texture2D SummaryTabIconTexture { get; }
        public Texture2D CustomStatProfitTabIconTexture { get; }
        public Texture2D IgnoredItemsTabIconTexture { get; }
        public Texture2D IgnoredItemsPanelIconTexture { get; }
        public Texture2D CornerIconTexture { get; }
        public Texture2D CornerIconHoverTexture { get; }
        public Texture2D FallbackTexture { get; }
        public Texture2D OpenLinkTexture { get; }
        public Texture2D GoldCoinTexture { get; }
        public Texture2D SilverCoinTexture { get; }
        public Texture2D CopperCoinTexture { get; }
        public AsyncTexture2D SmallGoldCoinTexture { get; } // because the big ones look ugly when scaled down
        public AsyncTexture2D SmallSilverCoinTexture { get; }
        public AsyncTexture2D SmallCopperCoinTexture { get; }
        public AsyncTexture2D SettingsTabIconTexture { get; }
        public AsyncTexture2D DebugTabIconTexture { get; }
        public AsyncTexture2D WindowBackgroundTexture { get; }
        public AsyncTexture2D InventorySlotBackgroundTexture { get; }
        public AsyncTexture2D MerchantTexture { get; }
        public AsyncTexture2D ItemsTexture { get; }
        public AsyncTexture2D FavoriteTexture { get; }
        public AsyncTexture2D TradingPostTexture { get; }

        public const int MISSING_ASSET_ID = 0;
    }
}
