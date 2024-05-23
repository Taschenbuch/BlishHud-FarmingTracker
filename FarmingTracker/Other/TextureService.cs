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
            GoldCoinTexture = contentsManager.GetTexture("coin-gold.png");
            SilverCoinTexture = contentsManager.GetTexture("coin-silver.png");
            CopperCoinTexture = contentsManager.GetTexture("coin-copper.png");
            FallbackTexture = contentsManager.GetTexture("fallback_157084.png");

            // no dispose necessary:
            SettingsTabIconTexture = GetTextureFromAssetCacheOrFallback(156737);
            WindowBackgroundTexture = GetTextureFromAssetCacheOrFallback(155997);
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
            GoldCoinTexture?.Dispose();
            SilverCoinTexture?.Dispose();
            CopperCoinTexture?.Dispose();
            FallbackTexture?.Dispose();
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
        public Texture2D HelpTabIconTexture { get; }
        public Texture2D FilterTabIconTexture { get; }
        public Texture2D SortTabIconTexture { get; }
        public Texture2D TimelineTabIconTexture { get; }
        public Texture2D SummaryTabIconTexture { get; }
        public Texture2D SearchTabIconTexture { get; }
        public Texture2D CornerIconTexture { get; }
        public Texture2D CornerIconHoverTexture { get; }
        public Texture2D FallbackTexture { get; }
        public Texture2D GoldCoinTexture { get; }
        public Texture2D SilverCoinTexture { get; }
        public Texture2D CopperCoinTexture { get; }
        public AsyncTexture2D SettingsTabIconTexture { get; }
        public AsyncTexture2D WindowBackgroundTexture { get; }
        public const int MISSING_ASSET_ID = 0;
    }
}
