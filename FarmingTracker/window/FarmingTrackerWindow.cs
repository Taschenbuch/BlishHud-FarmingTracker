using Blish_HUD.Controls;
using Blish_HUD;
using Microsoft.Xna.Framework;

namespace FarmingTracker
{
    public class FarmingTrackerWindow : TabbedWindow2
    {
        public FarmingTrackerWindow(int windowWidth, int windowHeight, Model model, Services services)
            : base(services.TextureService.WindowBackgroundTexture,
                new Rectangle(20, 26, windowWidth, windowHeight),
                new Rectangle(80, 20, windowWidth - 80, windowHeight - 20))
        {
            Title = "Farming Tracker";
            Emblem = services.TextureService.WindowEmblemTexture;
            SavesPosition = true;
            Id = "Ecksofa.FarmingTracker: FarmingTrackerWindow";
            Location = new Point(300, 300);
            CanResize = true;
            SavesSize = true;
            Width = 630; // default width on first startup. Will be ignored on consecutive module startups after resizing the window.
            Parent = GameService.Graphics.SpriteScreen;

            Resized += (s, e) =>
            {
                ShowOrHideWindowSubtitle(e.CurrentSize.X);
            };

            TabChanged += (s, e) => 
            {
                ShowOrHideWindowSubtitle(Width);
            };

            _profitWindow = new ProfitWindow(services);
            var summaryTabView = new SummaryTabView(_profitWindow, model, services);
            _summaryTabView = summaryTabView;
            _summaryTab = new Tab(services.TextureService.SummaryTabIconTexture, () => summaryTabView, SUMMARY_TAB_TITLE);
            _settingsTab = new Tab(services.TextureService.SettingsTabIconTexture, () => new SettingsTabView(services), SETTINGS_TAB_TITLE);
            _customStatProfitTab = new Tab(services.TextureService.CustomStatProfitTabIconTexture, () => new CustomStatProfitTabView(model, services), CUSTOM_STAT_PROFIT_TAB_TITLE);

            Tabs.Add(_summaryTab);
            Tabs.Add(new Tab(services.TextureService.TimelineTabIconTexture, () => new PlaceholderTabView(TIMELINE_TAB_TITLE), TIMELINE_TAB_TITLE));
            Tabs.Add(new Tab(services.TextureService.FilterTabIconTexture, () => new FilterTabView(services), FILTER_TAB_TITLE));
            Tabs.Add(new Tab(services.TextureService.SortTabIconTexture, () => new SortTabView(services), SORT_TAB_TITLE));
            Tabs.Add(_customStatProfitTab);
            Tabs.Add(new Tab(services.TextureService.IgnoredItemsTabIconTexture, () => new IgnoredItemsTabView(model, services), IGNORED_ITEMS_TAB_TITLE));
            Tabs.Add(_settingsTab);

            if(DebugMode.VisualStudioRunningInDebugMode)
                Tabs.Add(new Tab(services.TextureService.DebugTabIconTexture, () => new DebugTabView(model, services), DEBUG_TAB_TITLE));
        }

        protected override void DisposeControl()
        {
            _summaryTabView?.Dispose();
            _profitWindow?.Dispose();
            base.DisposeControl();
        }

        public void ShowWindowAndSelectSettingsTab()
        {
            Show();
            SelectedTab = _settingsTab;
        }

        public void ShowWindowAndSelectCustomProfitTab()
        {
            Show();
            SelectedTab = _customStatProfitTab;
        }

        public void ToggleWindowAndSelectSummaryTab()
        {
            if(Visible)
                Hide();
            else
            {
                SelectedTab = _summaryTab;
                Show();
            }
        }

        public void Update2(GameTime gameTime) // Update2() because Update() already exists but is not always called.
        {
            _summaryTabView?.Update(gameTime);
        }

        private void ShowOrHideWindowSubtitle(int width)
        {
            Subtitle = width < 500
                ? ""
                : SelectedTab.Name;
        }

        private readonly SummaryTabView _summaryTabView;
        private readonly Tab _summaryTab;
        private readonly Tab _settingsTab;
        private readonly Tab _customStatProfitTab;
        private readonly ProfitWindow _profitWindow;
        public const string SUMMARY_TAB_TITLE = "Summary";
        private const string TIMELINE_TAB_TITLE = "Timeline";
        private const string FILTER_TAB_TITLE = "Filter";
        private const string SORT_TAB_TITLE = "Sort Items";
        public const string IGNORED_ITEMS_TAB_TITLE = "Ignored Items";
        public const string CUSTOM_STAT_PROFIT_TAB_TITLE = "Custom Profit";
        public const string SETTINGS_TAB_TITLE = "Settings";
        private const string DEBUG_TAB_TITLE = "Debug";
    }
}
