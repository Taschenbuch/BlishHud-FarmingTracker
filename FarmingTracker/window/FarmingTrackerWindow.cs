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
            _summaryTab = new Tab(services.TextureService.SummaryTabIconTexture, () => summaryTabView, Constants.TabTitles.SUMMARY);
            _settingsTab = new Tab(services.TextureService.SettingsTabIconTexture, () => new SettingsTabView(services), Constants.TabTitles.SETTINGS);
            _customStatProfitTab = new Tab(services.TextureService.CustomStatProfitTabIconTexture, () => new CustomStatProfitTabView(model, services), Constants.TabTitles.CUSTOM_STAT_PROFIT);

            Tabs.Add(_summaryTab);
            Tabs.Add(new Tab(services.TextureService.TimelineTabIconTexture, () => new PlaceholderTabView(Constants.TabTitles.TIMELINE), Constants.TabTitles.TIMELINE));
            Tabs.Add(new Tab(services.TextureService.FilterTabIconTexture, () => new FilterTabView(services), Constants.TabTitles.FILTER));
            Tabs.Add(new Tab(services.TextureService.SortTabIconTexture, () => new SortTabView(services), Constants.TabTitles.SORT));
            Tabs.Add(_customStatProfitTab);
            Tabs.Add(new Tab(services.TextureService.IgnoredItemsTabIconTexture, () => new IgnoredItemsTabView(model, services), Constants.TabTitles.IGNORED));
            Tabs.Add(_settingsTab);

            if(DebugMode.VisualStudioRunningInDebugMode)
                Tabs.Add(new Tab(services.TextureService.DebugTabIconTexture, () => new DebugTabView(model, services), Constants.TabTitles.DEBUG));
        }

        protected override void DisposeControl()
        {
            _summaryTabView?.Dispose();
            _profitWindow?.Dispose();
            base.DisposeControl();
        }

        public void SelectWindowTab(WindowTab windowTab, WindowVisibility windowVisibility)
        {
            if(windowVisibility == WindowVisibility.Toggle && Visible)
            {
                Hide();
                return;
            }

            switch (windowTab)
            {
                case WindowTab.Summary:
                    SelectedTab = _summaryTab;
                    break;
                case WindowTab.Settings:
                    SelectedTab = _settingsTab;
                    break;
                case WindowTab.CustomProfit:
                    SelectedTab = _customStatProfitTab;
                    break;
                default:
                    break;
            }

            Show();
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
    }
}
