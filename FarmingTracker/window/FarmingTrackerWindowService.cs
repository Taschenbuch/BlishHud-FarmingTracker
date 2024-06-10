using Blish_HUD.Controls;
using Blish_HUD;
using Microsoft.Xna.Framework;
using System;
using Blish_HUD.Graphics.UI;

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
                Parent = GameService.Graphics.SpriteScreen,
            };

            var summaryTabView = new SummaryTabView(this, services);

            IView SummaryViewFunc()
            {
                _farmingTrackerWindow.Subtitle = SUMMARY_TAB_TITLE;
                return summaryTabView;
            }

            _summaryTabView = summaryTabView;

            IView TimelineViewFunc()
            {
                _farmingTrackerWindow.Subtitle = TIMELINE_TAB_TITLE;
                return new PlaceholderTabView(TIMELINE_TAB_TITLE);
            }

            IView FilterViewFunc()
            {
                _farmingTrackerWindow.Subtitle = FILTER_TAB_TITLE;
                return new FilterTabView(services);
            }

            IView SortViewFunc()
            {
                _farmingTrackerWindow.Subtitle = SORT_TAB_TITLE;
                return new SortTabView(services);
            }

            IView IgnoredItemsViewFunc()
            {
                _farmingTrackerWindow.Subtitle = IGNORED_ITEMS_TAB_TITLE;
                return new IgnoredItemsTabView(services);
            }

            IView SettingViewFunc()
            {
                _farmingTrackerWindow.Subtitle = SETTINGS_TAB_TITLE;
                return new SettingsTabView(services);
            }

            IView DebugViewFunc()
            {
                _farmingTrackerWindow.Subtitle = DEBUG_TAB_TITLE;
                return new DebugTabView(services);
            }

            _summaryTab = new Tab(services.TextureService.SummaryTabIconTexture, SummaryViewFunc, SUMMARY_TAB_TITLE);
            _settingsTab = new Tab(services.TextureService.SettingsTabIconTexture, SettingViewFunc, SETTINGS_TAB_TITLE);

            _farmingTrackerWindow.Tabs.Add(_summaryTab);
            _farmingTrackerWindow.Tabs.Add(new Tab(services.TextureService.TimelineTabIconTexture, TimelineViewFunc, TIMELINE_TAB_TITLE));
            _farmingTrackerWindow.Tabs.Add(new Tab(services.TextureService.FilterTabIconTexture, FilterViewFunc, FILTER_TAB_TITLE));
            _farmingTrackerWindow.Tabs.Add(new Tab(services.TextureService.SortTabIconTexture, SortViewFunc, SORT_TAB_TITLE));
            _farmingTrackerWindow.Tabs.Add(new Tab(services.TextureService.IgnoredItemsTabIconTexture, IgnoredItemsViewFunc, IGNORED_ITEMS_TAB_TITLE));
            _farmingTrackerWindow.Tabs.Add(_settingsTab);
#if DEBUG
            _farmingTrackerWindow.Tabs.Add(new Tab(services.TextureService.DebugTabIconTexture, DebugViewFunc, DEBUG_TAB_TITLE));
#endif
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
        private readonly Tab _settingsTab;
        private readonly SummaryTabView _summaryTabView;
        private readonly Tab _summaryTab;
        public const string SUMMARY_TAB_TITLE = "Farming Summary";
        private const string TIMELINE_TAB_TITLE = "Farming Timeline";
        private const string FILTER_TAB_TITLE = "Filter";
        private const string SORT_TAB_TITLE = "Sort (items)";
        public const string IGNORED_ITEMS_TAB_TITLE = "Ignored Items";
        private const string SETTINGS_TAB_TITLE = "Settings";
        private const string DEBUG_TAB_TITLE = "Debug";
    }
}
