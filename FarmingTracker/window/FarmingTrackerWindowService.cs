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
            var windowWidth = 570;
            var windowHeight = 650;
            var contentWidth = windowWidth - 80;
            var contentHeight = windowHeight - 20;
            var flowPanelWidth = contentWidth - 40;

            var textureService = services.TextureService;

            _farmingTrackerWindow = new TabbedWindow2(
                AsyncTexture2D.FromAssetId(155997),
                new Rectangle(20, 26, windowWidth, windowHeight),
                new Rectangle(80, 20, contentWidth, contentHeight))
            {
                Title = "Farming Tracker",
                Emblem = textureService.WindowEmblemTexture,
                SavesPosition = true,
                Id = "Ecksofa.FarmingTracker: FarmingTrackerWindow",
                Location = new Point(300, 300),
                Parent = GameService.Graphics.SpriteScreen,
            };

            var updateLoop = new UpdateLoop();
            var farmingSummaryTabView = new FarmingSummaryTabView(this, flowPanelWidth, services);
            _sessionSummaryTab = new Tab(services.TextureService.SessionSummaryTabIconTexture, () => farmingSummaryTabView, "Session summary");
            _farmingSummaryTabView = farmingSummaryTabView;
            
            _settingsTab = new Tab(AsyncTexture2D.FromAssetId(156737), () => new SettingsTabView(services), "Settings");

            _farmingTrackerWindow.Tabs.Add(_sessionSummaryTab);
            _farmingTrackerWindow.Tabs.Add(new Tab(textureService.TimelineTabIconTexture, () => new PlaceholderTabView("TIMELINE VIEW"), "Timeline view"));
            _farmingTrackerWindow.Tabs.Add(new Tab(textureService.FilterTabIconTexture, () => new FilterTabView(services), "Filter"));
            _farmingTrackerWindow.Tabs.Add(new Tab(textureService.SortTabIconTexture, () => new PlaceholderTabView("CUSTOM SORTING"), "Sort"));
            _farmingTrackerWindow.Tabs.Add(new Tab(textureService.SearchTabIconTexture, () => new PlaceholderTabView("SEARCHING"), "Search"));
            _farmingTrackerWindow.Tabs.Add(_settingsTab);
            _farmingTrackerWindow.Tabs.Add(new Tab(textureService.HelpTabIconTexture, () => new PlaceholderTabView("Check 'Setup DRF' on settings tab for help.", true), "Help"));
        }

        public void Dispose()
        {
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

        private readonly TabbedWindow2 _farmingTrackerWindow;
        private readonly Tab _settingsTab;
        private readonly FarmingSummaryTabView _farmingSummaryTabView;
        private readonly Tab _sessionSummaryTab;
    }
}
