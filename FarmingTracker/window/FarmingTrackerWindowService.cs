using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;

namespace FarmingTracker
{
    public class FarmingTrackerWindowService : IDisposable
    {
        public FarmingTrackerWindowService(Services services)
        {
            var windowWidth = 560;
            var windowHeight = 640;
            var flowPanelWidth = windowWidth - 47;

            _windowEmblemTexture = services.ContentsManager.GetTexture(@"windowEmblem.png");

            _farmingTrackerWindow = new TabbedWindow2(
                AsyncTexture2D.FromAssetId(155997),
                new Rectangle(-25, 26, windowWidth + 60, windowHeight),
                new Rectangle(40, 20, windowWidth - 40, windowHeight - 50))
            {
                Title = "Farming Tracker",
                Emblem = _windowEmblemTexture,
                SavesPosition = true,
                Id = "Ecksofa.FarmingTracker: FarmingTrackerWindow",
                Location = new Point(300, 300),
                Parent = GameService.Graphics.SpriteScreen,
            };

            _farmingSummaryTabView = new FarmingSummaryTabView(flowPanelWidth, services);
            _farmingTrackerWindow.Tabs.Add(new Tab(AsyncTexture2D.FromAssetId(440022), () => _farmingSummaryTabView, "Session summary"));
            _farmingTrackerWindow.Tabs.Add(new Tab(AsyncTexture2D.FromAssetId(733360), () => new PlaceholderTabView(), "Timeline view")); // alternative 841721
            _farmingTrackerWindow.Tabs.Add(new Tab(AsyncTexture2D.FromAssetId(155945), () => new PlaceholderTabView(), "Filter")); // todo ersetzen mit custom
            _farmingTrackerWindow.Tabs.Add(new Tab(AsyncTexture2D.FromAssetId(156756), () => new PlaceholderTabView(), "Sort")); // todo ersetzen mit custom
            _farmingTrackerWindow.Tabs.Add(new Tab(AsyncTexture2D.FromAssetId(157110), () => new PlaceholderTabView(), "Settings"));
            _farmingTrackerWindow.Tabs.Add(new Tab(AsyncTexture2D.FromAssetId(759447), () => new PlaceholderTabView(), "Help")); // todo ersetzen mit custom
        }

        public void Dispose()
        {
            _windowEmblemTexture?.Dispose();
            _farmingTrackerWindow?.Dispose();
        }

        public void ToggleWindow()
        {
            _farmingTrackerWindow.ToggleWindow();
        }

        public void Update(GameTime gameTime)
        {
            _farmingSummaryTabView?.Update(gameTime);
        }

        private readonly Texture2D _windowEmblemTexture;
        private readonly TabbedWindow2 _farmingTrackerWindow;
        private readonly FarmingSummaryTabView _farmingSummaryTabView;
    }
}
