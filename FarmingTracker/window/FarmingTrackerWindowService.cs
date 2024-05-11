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
            _helpTabIconTexture = services.ContentsManager.GetTexture(@"helpTabIcon.png");
            _filterTabIconTexture = services.ContentsManager.GetTexture(@"filterTabIcon.png");
            _sortTabIconTexture = services.ContentsManager.GetTexture(@"sortTabIcon.png");
            _timelineTabIconTexture = services.ContentsManager.GetTexture(@"timelineTabIcon.png");
            _sessionSummaryTabIconTexture = services.ContentsManager.GetTexture(@"sessionSummaryTabIcon.png");

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

            _settingsTab = new Tab(AsyncTexture2D.FromAssetId(156737), () => new SettingsTabView(services), "Settings");
            _farmingSummaryTabView = new FarmingSummaryTabView(flowPanelWidth, services);
            _farmingTrackerWindow.Tabs.Add(new Tab(_sessionSummaryTabIconTexture, () => _farmingSummaryTabView, "Session summary"));
            _farmingTrackerWindow.Tabs.Add(new Tab(_timelineTabIconTexture, () => new PlaceholderTabView(), "Timeline view"));
            _farmingTrackerWindow.Tabs.Add(new Tab(_filterTabIconTexture, () => new PlaceholderTabView(), "Filter"));
            _farmingTrackerWindow.Tabs.Add(new Tab(_sortTabIconTexture, () => new PlaceholderTabView(), "Sort"));
            _farmingTrackerWindow.Tabs.Add(_settingsTab);
            _farmingTrackerWindow.Tabs.Add(new Tab(_helpTabIconTexture, () => new PlaceholderTabView(), "Help"));
        }

        public void Dispose()
        {
            _windowEmblemTexture?.Dispose();
            _sessionSummaryTabIconTexture?.Dispose();
            _filterTabIconTexture?.Dispose();
            _sortTabIconTexture?.Dispose();
            _helpTabIconTexture?.Dispose();
            _farmingTrackerWindow?.Dispose();
        }

        public void ShowWindowAndSelectSettingsTab()
        {
            _farmingTrackerWindow.Show();
            _farmingTrackerWindow.SelectedTab = _settingsTab;
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
        private readonly Texture2D _helpTabIconTexture;
        private readonly Texture2D _filterTabIconTexture;
        private readonly Texture2D _sortTabIconTexture;
        private readonly Texture2D _timelineTabIconTexture;
        private readonly Texture2D _sessionSummaryTabIconTexture;
        private readonly TabbedWindow2 _farmingTrackerWindow;
        private readonly Tab _settingsTab;
        private readonly FarmingSummaryTabView _farmingSummaryTabView;
    }
}
