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
            _searchTabIconTexture = services.ContentsManager.GetTexture(@"searchTabIcon.png");

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

            var updateLoop = new UpdateLoop();
            var farmingSummaryTabView = new FarmingSummaryTabView(this, flowPanelWidth, services);
            _sessionSummaryTab = new Tab(_sessionSummaryTabIconTexture, () => farmingSummaryTabView, "Session summary");
            _farmingSummaryTabView = farmingSummaryTabView;
            
            _settingsTab = new Tab(AsyncTexture2D.FromAssetId(156737), () => new SettingsTabView(services), "Settings");

            _farmingTrackerWindow.Tabs.Add(_sessionSummaryTab);
            _farmingTrackerWindow.Tabs.Add(new Tab(_timelineTabIconTexture, () => new PlaceholderTabView("TIMELINE VIEW"), "Timeline view"));
            _farmingTrackerWindow.Tabs.Add(new Tab(_filterTabIconTexture, () => new FilterTabView(services), "Filter"));
            _farmingTrackerWindow.Tabs.Add(new Tab(_sortTabIconTexture, () => new PlaceholderTabView("CUSTOM SORTING"), "Sort"));
            _farmingTrackerWindow.Tabs.Add(new Tab(_searchTabIconTexture, () => new PlaceholderTabView("SEARCHING"), "Search"));
            _farmingTrackerWindow.Tabs.Add(_settingsTab);
            _farmingTrackerWindow.Tabs.Add(new Tab(_helpTabIconTexture, () => new PlaceholderTabView("Check 'Setup DRF' on settings tab for help.", true), "Help"));
        }

        public void Dispose()
        {
            _windowEmblemTexture?.Dispose();
            _sessionSummaryTabIconTexture?.Dispose();
            _filterTabIconTexture?.Dispose();
            _sortTabIconTexture?.Dispose();
            _helpTabIconTexture?.Dispose();
            _searchTabIconTexture?.Dispose();
            _farmingTrackerWindow?.Dispose();
        }

        public void ShowWindowAndSelectSettingsTab()
        {
            _farmingTrackerWindow.Show();
            _farmingTrackerWindow.SelectedTab = _settingsTab;
        }

        public void ToggleWindowAndSelectSessionSummaryTab()
        {
            if(_farmingTrackerWindow.Visible)
                _farmingTrackerWindow.Hide();
            else
            {
                _farmingTrackerWindow.SelectedTab = _sessionSummaryTab;
                _farmingTrackerWindow.Show();
            }
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
        private readonly Texture2D _searchTabIconTexture;
        private readonly TabbedWindow2 _farmingTrackerWindow;
        private readonly Tab _settingsTab;
        private readonly FarmingSummaryTabView _farmingSummaryTabView;
        private readonly Tab _sessionSummaryTab;
    }
}
