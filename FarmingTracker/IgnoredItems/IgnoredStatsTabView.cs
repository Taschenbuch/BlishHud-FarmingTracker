using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;
using System.Linq;

namespace FarmingTracker
{
    public class IgnoredStatsTabView : View
    {
        public IgnoredStatsTabView(Model model, Services services)
        {
            _model = model;
            _services = services;
        }

        protected override void Unload()
        {
            _rootFlowPanel?.Dispose();
            _rootFlowPanel = null;
            base.Unload();
        }

        protected override void Build(Container buildPanel)
        {
            _rootFlowPanel = new FlowPanel
            {
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                CanScroll = true,
                ControlPadding = new Vector2(0, 10),
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.Fill,
                Parent = buildPanel
            };

            var collapsibleHelp = new CollapsibleHelp(
                $"IGNORE ITEM / CURRENCY:\n" +
                $"In the '{Constants.TabTitles.SUMMARY}' tab right click on an item / currency icon to ignore it.\n" +
                $"\n" +
                $"UNIGNORE ITEM / CURRENCY:\n" +
                $"left click on an item or currency here to unignore it.\n" +
                $"\n" +
                $"WHY IGNORE?\n" +
                $"An ignored item / currency will appear here. It is hidden in the '{Constants.TabTitles.SUMMARY}' tab" +
                $" and does not contribute to profit calculations." +
                $" That can be usefull to prevent that none-legendary equipment that you swap manually is tracked accidently.",
                buildPanel.ContentRegion.Width - Constants.SCROLLBAR_WIDTH_OFFSET, // buildPanel because other Panels dont have correctly updated width yet.
                _rootFlowPanel);

            buildPanel.ContentResized += (s, e) => collapsibleHelp.UpdateSize(e.CurrentRegion.Width - Constants.SCROLLBAR_WIDTH_OFFSET);

            var flowPanelWithButtonContainer = new AutoSizeContainer(_rootFlowPanel);

            var ignoredStatsWrapperFlowPanel = new FlowPanel
            {
                Title = IGNORED_STATS_PANEL_TITLE,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                Icon = _services.TextureService.IgnoredStatsPanelIconTexture,
                Width = buildPanel.ContentRegion.Width - Constants.SCROLLBAR_WIDTH_OFFSET,
                HeightSizingMode = SizingMode.AutoSize,
                Parent = flowPanelWithButtonContainer
            };

            var unignoreAllButton = new StandardButton
            {
                Text = "Unignore all",
                Enabled = false,
                Width = 150,
                Top = 5,
                Right = buildPanel.ContentRegion.Width - Constants.SCROLLBAR_WIDTH_OFFSET,
                Parent = flowPanelWithButtonContainer
            };

            var hintLabel = new HintLabel(ignoredStatsWrapperFlowPanel, Constants.ZERO_HEIGHT_EMPTY_LABEL);

            var ignoredStatsFlowPanel = new FlowPanel
            {
                FlowDirection = ControlFlowDirection.LeftToRight,
                HeightSizingMode = SizingMode.AutoSize,
                WidthSizingMode = SizingMode.Fill,
                Parent = ignoredStatsWrapperFlowPanel
            };

            buildPanel.ContentResized += (s, e) =>
            {
                ignoredStatsWrapperFlowPanel.Width = e.CurrentRegion.Width - Constants.SCROLLBAR_WIDTH_OFFSET;
                unignoreAllButton.Right = e.CurrentRegion.Width - Constants.SCROLLBAR_WIDTH_OFFSET;
            };

            var ignoredStats = _model.Stats.ItemById.Values // todo x lock?
                .Concat(_model.Stats.CurrencyById.Values)
                .Where(s => s.StatVisibility == StatVisibility.Ignored);

            var noStatsAreIgnored = ignoredStats.IsEmpty();
            if (noStatsAreIgnored)
            {
                ShowNoStatsAreIgnoredHintIfNecessary(hintLabel, _model);
                return;
            }

            var ignoredStatsApiDataMissing = ignoredStats.Any(i => i.Details.State == ApiStatDetailsState.MissingBecauseApiNotCalledYet);
            if (ignoredStatsApiDataMissing)
            {
                ShowLoadingHint(hintLabel);
                return;
            }

            hintLabel.Text = $"Left click an item or currency to unignore it.";

            foreach (var ignoredStat in ignoredStats)
                ShowIgnoredStat(ignoredStat, _model, _services, hintLabel, ignoredStatsFlowPanel);

            unignoreAllButton.Enabled = true;
            unignoreAllButton.Click += (sender, args) =>
            {
                foreach (var statContainer in ignoredStatsFlowPanel.Children.ToList())
                    statContainer.Dispose(); // this removes it from flowPanel, too.

                foreach (var stat in ignoredStats) // todo x lock?
                    if(stat.StatVisibility == StatVisibility.Ignored)
                        stat.StatVisibility = StatVisibility.Regular;

                _services.UpdateLoop.TriggerUpdateUi();
                _services.UpdateLoop.TriggerSaveModel();

                ShowNoStatsAreIgnoredHintIfNecessary(hintLabel, _model);
            };
        }

        private static void ShowIgnoredStat(Stat ignoredStat, Model model, Services services, HintLabel hintLabel, Container parent)
        {
            var statContainer = new StatContainer(ignoredStat, PanelType.IgnoredStats, model, services)
            {
                Parent = parent
            };

            statContainer.Click += (sender, args) =>
            {
                UnignoreStat(ignoredStat, model, services);
                statContainer.Dispose();
                ShowNoStatsAreIgnoredHintIfNecessary(hintLabel, model);
            };
        }

        private static void UnignoreStat(Stat stat, Model model, Services services)
        {
            var ignoredStats = model.Stats.ItemById.Values // todo x lock?
                .Concat(model.Stats.CurrencyById.Values)
                .Where(s => s.StatVisibility == StatVisibility.Ignored);

            var matchingFavoriteStat = ignoredStats.FirstOrDefault(i => i.StatType == stat.StatType && i.ApiId == stat.ApiId);
            if(matchingFavoriteStat == null)
            {
                Module.Logger.Error("Failed to remove ignored stat because ignored stat did not exist. That should not be possible.");
                return;
            }

            matchingFavoriteStat.StatVisibility = StatVisibility.Regular;
            services.UpdateLoop.TriggerUpdateUi();
            services.UpdateLoop.TriggerSaveModel();
        }

        private static void ShowNoStatsAreIgnoredHintIfNecessary(HintLabel hintLabel, Model model)
        {
            var ignoredStats = model.Stats.ItemById.Values // todo x lock?
                .Concat(model.Stats.CurrencyById.Values)
                .Where(s => s.StatVisibility == StatVisibility.Ignored);

            if (ignoredStats.Any()) // todo x lock?
                return;

            hintLabel.Text = 
                $"Nothing is ignored.\n" +
                $"You can ignore an item or currency by right clicking it in the '{Constants.TabTitles.SUMMARY}' tab.";
        }

        private static void ShowLoadingHint(HintLabel hintLabel)
        {
            hintLabel.Text =
                $"This tab will not refresh automatically.\n" +
                $"Go to '{Constants.TabTitles.SUMMARY}' tab and " +
                $"wait until the '{Constants.UPDATING_HINT_TEXT}' hint disappears.\n" +
                $"Then come back here and your ignored items and currencies will be displayed.";
        }

        private readonly Model _model;
        private readonly Services _services;
        private FlowPanel? _rootFlowPanel;
        private const string IGNORED_STATS_PANEL_TITLE = "Ignored items and currencies";
    }
}
