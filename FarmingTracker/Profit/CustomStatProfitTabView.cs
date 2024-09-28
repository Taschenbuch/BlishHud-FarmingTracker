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
                $"ADD CUSTOM PROFIT:\n" +
                $"In the '{Constants.TabTitles.SUMMARY}' tab right click on an item or currency icon to set its custom profit to 0. " +
                $"After that you can edit the custom profit in this tab.\n" +
                $"\n" +
                $"REMOVE CUSTOM PROFIT:\n" +
                $"In this tab click on the 'x' button of a custom profit row.\n" +
                $"\n" +
                $"HOW DOES IT WORK?\n" +
                $"- If a custom profit is set, the custom profit will be used to calculate the total profit instead of the trading post or vendor sell price. " +
                $"Even when the custom profit is lower or 0.\n" +
                $"- Trading post taxes are not deducted from the custom profit.\n" +
                $"- The custom profit applies to a single item/currency. Because of that you can not set the custom profit of 10 karma to 1 copper for example " +
                $"(0.1 copper/Karma).\n" +
                $"\n" +
                $"WHY CUSTOM PROFIT?\n" +
                $"- Currencies like volatile magic cannot be sold to the vendor or on the trading post. " +
                $"But there are other ways to make profit with them. Use the custom profit to take that into account depending on the method you use.\n" +
                $"- Set an item with an unrealistically high/low trading post price to a more realistic custom profit.",
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

                var stat = stats.SingleOrDefault(s => customStatProfit.BelongsToStat(s));

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
                $"{Constants.HINT_IN_PANEL_PADDING}You can set custom profits by right clicking an item/currency in the '{Constants.TabTitles.SUMMARY}' tab.";
        }

        private static void ShowLoadingHint(HintLabel hintLabel)
        {
            hintLabel.Text =
                $"{Constants.HINT_IN_PANEL_PADDING}This tab will not refresh automatically.\n" +
                $"{Constants.HINT_IN_PANEL_PADDING}Go to '{Constants.TabTitles.SUMMARY}' tab and " +
                $"wait until the '{Constants.UPDATING_HINT_TEXT}' hint disappears.\n" +
                $"{Constants.HINT_IN_PANEL_PADDING}Then come back here.";
        }

        private readonly Model _model;
        private readonly Services _services;
        private FlowPanel? _rootFlowPanel;
    }
}
