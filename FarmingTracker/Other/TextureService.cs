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
    }
}
