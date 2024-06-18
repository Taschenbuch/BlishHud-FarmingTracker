using Blish_HUD.Controls;
using Blish_HUD;
using Microsoft.Xna.Framework;
using System;

namespace FarmingTracker
{
    public class FarmingTrackerWindowService : IDisposable
    {
        public FarmingTrackerWindowService(Model model, Services services)
        {
            var windowWidth = 570;
            var windowHeight = 650;
            var contentWidth = windowWidth - 80;
            var contentHeight = windowHeight - 20;

            _farmingTrackerWindow = new TabbedWindow2(
                services.TextureService.WindowBackgroundTexture,
                new Rectangle(20, 26, windowWidth, windowHeight),
                new Rectangle(80, 20, contentWidth, contentHeight))
            {
                Title = "Farming Tracker",
                Emblem = services.TextureService.WindowEmblemTexture,
                SavesPosition = true,
                Id = "Ecksofa.FarmingTracker: FarmingTrackerWindow",
                Location = new Point(300, 300),
                CanResize = true,
                SavesSize = true,
                Width = 630, // default width on first startup. Will be ignored on consecutive module startups after resizing the window.
                Parent = GameService.Graphics.SpriteScreen,
            };

            _farmingTrackerWindow.Resized += (s, e) =>
            {
                ShowOrHideWindowSubtitle(e.CurrentSize.X);
            };

            _farmingTrackerWindow.TabChanged += (s, e) => 
            {
                ShowOrHideWindowSubtitle(_farmingTrackerWindow.Width);
            };

            var summaryTabView = new SummaryTabView(this, model, services);
            _summaryTabView = summaryTabView;
            _summaryTab = new Tab(services.TextureService.SummaryTabIconTexture, () => summaryTabView, SUMMARY_TAB_TITLE);
            _settingsTab = new Tab(services.TextureService.SettingsTabIconTexture, () => new SettingsTabView(services), SETTINGS_TAB_TITLE);

            _farmingTrackerWindow.Tabs.Add(_summaryTab);
            _farmingTrackerWindow.Tabs.Add(new Tab(services.TextureService.TimelineTabIconTexture, () => new PlaceholderTabView(TIMELINE_TAB_TITLE), TIMELINE_TAB_TITLE));
            _farmingTrackerWindow.Tabs.Add(new Tab(services.TextureService.FilterTabIconTexture, () => new FilterTabView(services), FILTER_TAB_TITLE));
            _farmingTrackerWindow.Tabs.Add(new Tab(services.TextureService.SortTabIconTexture, () => new SortTabView(services), SORT_TAB_TITLE));
            _farmingTrackerWindow.Tabs.Add(new Tab(services.TextureService.IgnoredItemsTabIconTexture, () => new IgnoredItemsTabView(model, services), IGNORED_ITEMS_TAB_TITLE));
            _farmingTrackerWindow.Tabs.Add(_settingsTab);
#if DEBUG
            _farmingTrackerWindow.Tabs.Add(new Tab(services.TextureService.DebugTabIconTexture, () => new DebugTabView(model, services), DEBUG_TAB_TITLE));
#endif
        }

        private void ShowOrHideWindowSubtitle(int width)
        {
            _farmingTrackerWindow.Subtitle = width < 500
                                ? ""
                                : _farmingTrackerWindow.SelectedTab.Name;
        }

        public void Dispose()
        {
            _summaryTabView?.Dispose();
            _farmingTrackerWindow?.Dispose();
        }

        public void ShowWindowAndSelectSettingsTab()
        {
            _farmingTrackerWindow.Show();
            _farmingTrackerWindow.SelectedTab = _settingsTab;
        }

        public void ToggleWindowAndSelectSummaryTab()
        {
            if(_farmingTrackerWindow.Visible)
                _farmingTrackerWindow.Hide();
            else
            {
                _farmingTrackerWindow.SelectedTab = _summaryTab;
                _farmingTrackerWindow.Show();
            }
        }

        public void Update(GameTime gameTime)
        {
            _summaryTabView?.Update(gameTime);
        }

        private readonly TabbedWindow2 _farmingTrackerWindow;
        private readonly SummaryTabView _summaryTabView;
        private readonly Tab _summaryTab;
        private readonly Tab _settingsTab;
        public const string SUMMARY_TAB_TITLE = "Summary";
        private const string TIMELINE_TAB_TITLE = "Timeline";
        private const string FILTER_TAB_TITLE = "Filter";
        private const string SORT_TAB_TITLE = "Sort Items";
        public const string IGNORED_ITEMS_TAB_TITLE = "Ignored Items";
        private const string SETTINGS_TAB_TITLE = "Settings";
        private const string DEBUG_TAB_TITLE = "Debug";
    }
}
