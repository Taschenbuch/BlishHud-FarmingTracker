using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;
using System.Linq;

namespace FarmingTracker
{
    public class CustomStatProfitTabView : View
    {
        public CustomStatProfitTabView(Model model, Services services)
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
                $"xxxx", // todo x
                buildPanel.ContentRegion.Width - Constants.SCROLLBAR_WIDTH_OFFSET, // buildPanel because other Panels dont have correctly updated width yet.
                _rootFlowPanel);

            buildPanel.ContentResized += (s, e) =>
            {
                collapsibleHelp.UpdateSize(e.CurrentRegion.Width - Constants.SCROLLBAR_WIDTH_OFFSET);
            };

            var hintLabel = new HintLabel(_rootFlowPanel, Constants.ZERO_HEIGHT_EMPTY_LABEL);

            var customStatProfits = _model.CustomStatProfits.ToListSafe();
            var noCustomSatProfitsExist = customStatProfits.IsEmpty();
            if (noCustomSatProfitsExist)
            {
                ShowNoCustomStatProfitsExistHintIfNecessary(hintLabel, _model);
                return;
            }

            var statsSnapshot = _model.Stats.StatsSnapshot;
            var currencies = statsSnapshot.CurrencyById.Values.Where(c => !c.IsCoin).ToList();
            var items = statsSnapshot.ItemById.Values;

            var currenciesApiDataMissing = currencies.Any(i => i.Details.State == ApiStatDetailsState.MissingBecauseApiNotCalledYet);
            var itemApiDataMissing = items.Any(i => i.Details.State == ApiStatDetailsState.MissingBecauseApiNotCalledYet);
            if (currenciesApiDataMissing || itemApiDataMissing)
            {
                ShowLoadingHint(hintLabel);
                return;
            }

            var statsFlowPanel = CreateStatsFlowPanel(buildPanel, _rootFlowPanel);

            foreach (var customStatProfit in customStatProfits)
            {
                var stats = customStatProfit.StatType == StatType.Item
                    ? items
                    : currencies;

                var stat = stats.SingleOrDefault(s => s.ApiId == customStatProfit.ApiId && s.StatType == customStatProfit.StatType);

                if (stat == null)
                {
                    Module.Logger.Error($"Missing stat in model for customStatprofit id: {customStatProfit.ApiId}");
                    continue;
                }

                new CustomStatProfitRowPanel(customStatProfit, stat, hintLabel, _model, _services, statsFlowPanel);
            }
        }

        private static FlowPanel CreateStatsFlowPanel(Container buildPanel, Container parent)
        {
            return new FlowPanel
            {
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ControlPadding = new Vector2(0, 5),
                OuterControlPadding = new Vector2(0, 5),
                Width = buildPanel.ContentRegion.Width - Constants.SCROLLBAR_WIDTH_OFFSET,
                HeightSizingMode = SizingMode.AutoSize,
                Parent = parent
            };
        }

        public static void ShowNoCustomStatProfitsExistHintIfNecessary(HintLabel hintLabel, Model model)
        {
            if (model.CustomStatProfits.AnySafe())
                return;

            hintLabel.Text =
                $"{Constants.HINT_IN_PANEL_PADDING}No custom profits are set.\n" +
                $"{Constants.HINT_IN_PANEL_PADDING}You can set custom profits by right clicking an item/currency in the '{FarmingTrackerWindow.SUMMARY_TAB_TITLE}' tab.";
        }

        private static void ShowLoadingHint(HintLabel hintLabel)
        {
            hintLabel.Text =
                $"{Constants.HINT_IN_PANEL_PADDING}This tab will not refresh automatically.\n" +
                $"{Constants.HINT_IN_PANEL_PADDING}Go to '{FarmingTrackerWindow.SUMMARY_TAB_TITLE}' tab and " +
                $"wait until the '{Constants.UPDATING_HINT_TEXT}' hint disappears.\n" +
                $"{Constants.HINT_IN_PANEL_PADDING}Then come back here.";
        }

        private readonly Model _model;
        private readonly Services _services;
        private FlowPanel? _rootFlowPanel;
    }
}
