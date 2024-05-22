using Blish_HUD.Content;
using Blish_HUD.Modules.Managers;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace FarmingTracker
{
    public class TextureService : IDisposable
    {
        public TextureService(ContentsManager contentsManager)
        {
            WindowEmblemTexture = contentsManager.GetTexture("window-emblem.png");
            HelpTabIconTexture = contentsManager.GetTexture("help-tab-icon.png");
            FilterTabIconTexture = contentsManager.GetTexture("filter-tab-icon.png");
            SortTabIconTexture = contentsManager.GetTexture("sort-tab-icon.png");
            TimelineTabIconTexture = contentsManager.GetTexture("timeline-tab-icon.png");
            SummaryTabIconTexture = contentsManager.GetTexture("summary-tab-icon.png");
            SearchTabIconTexture = contentsManager.GetTexture("search-tab-icon.png");
            CornerIconTexture = contentsManager.GetTexture("corner-icon.png");
            CornerIconHoverTexture = contentsManager.GetTexture("corner-icon-hover.png");
            FallbackTexture = contentsManager.GetTexture("fallback_157084.png");

            // no dispose necessary:
            SettingsTabIconTexture = GetTextureFromAssetCacheOrFallback(156737);
            WindowBackgroundTexture = GetTextureFromAssetCacheOrFallback(155997);
            GoldCoinTexture = GetTextureFromAssetCacheOrFallback(GOLD_ICON_ASSET_ID);
            SilverCoinTexture = GetTextureFromAssetCacheOrFallback(SILVER_ICON_ASSET_ID);
            CopperCoinTexture = GetTextureFromAssetCacheOrFallback(COPPER_ICON_ASSET_ID);
        }

        public void Dispose()
        {
            WindowEmblemTexture?.Dispose();
            SummaryTabIconTexture?.Dispose();
            FilterTabIconTexture?.Dispose();
            SortTabIconTexture?.Dispose();
            TimelineTabIconTexture?.Dispose();
            HelpTabIconTexture?.Dispose();
            SearchTabIconTexture?.Dispose();
            CornerIconTexture?.Dispose();
            CornerIconHoverTexture?.Dispose();
            FallbackTexture?.Dispose();
        }

        public AsyncTexture2D GetTextureFromAssetCacheOrFallback(int assetId)
        {
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
        public Texture2D HelpTabIconTexture { get; }
        public Texture2D FilterTabIconTexture { get; }
        public Texture2D SortTabIconTexture { get; }
        public Texture2D TimelineTabIconTexture { get; }
        public Texture2D SummaryTabIconTexture { get; }
        public Texture2D SearchTabIconTexture { get; }
        public Texture2D CornerIconTexture { get; }
        public Texture2D CornerIconHoverTexture { get; }
        public Texture2D FallbackTexture { get; }
        public AsyncTexture2D SettingsTabIconTexture { get; }
        public AsyncTexture2D WindowBackgroundTexture { get; }
        public AsyncTexture2D GoldCoinTexture { get; }
        public AsyncTexture2D SilverCoinTexture { get; }
        public AsyncTexture2D CopperCoinTexture { get; }
        public const int MISSING_ASSET_ID = 0;
        public const int GOLD_ICON_ASSET_ID = 156904;
        public const int SILVER_ICON_ASSET_ID = 156907;
        public const int COPPER_ICON_ASSET_ID = 156902;
    }
}
