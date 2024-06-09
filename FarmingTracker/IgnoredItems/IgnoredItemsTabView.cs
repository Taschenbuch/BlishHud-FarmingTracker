using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;
using System.Linq;

namespace FarmingTracker
{
    public class IgnoredItemsTabView : View
    {
        public IgnoredItemsTabView(Services services)
        {
            _services = services;
        }

        protected override void Unload()
        {
            _ignoredItemsFlowPanel?.Dispose();
            _ignoredItemsFlowPanel = null;
            base.Unload();
        }

        protected override void Build(Container buildPanel)
        {
            var rootFlowPanel = new FlowPanel
            {
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                CanScroll = true,
                ControlPadding = new Vector2(0, 10),
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.Fill,
                Parent = buildPanel
            };

            new CollapsibleHelp(
                $"- ignore item:\n" +
                $"In the '{FarmingTrackerWindowService.SUMMARY_TAB_TITLE}' tab right click on an item icon in the 'Items' panel to ignore it.\n" +
                $"\n" +
                $"- unignore item:\n" +
                $"left click on an item here to unignore it.\n" +
                $"\n" +
                $"- Why ignore?\n" +
                $"An ignored item will appear here. It is hidden in the '{FarmingTrackerWindowService.SUMMARY_TAB_TITLE}' tab" +
                $" and does not contribute to profit calculations." +
                $" That can be usefull to prevent that none-legendary equipment that you swap manually is tracked accidently.",
                Constants.PANEL_WIDTH,
                rootFlowPanel);

            var unignoreAllButton = new StandardButton
            {
                Text = "Unignore all items",
                Enabled = false,
                Width = 150,
                Parent = rootFlowPanel
            };

            var ignoredItemsWrapperFlowPanel = new FlowPanel
            {
                Title = IGNORED_ITEMS_PANEL_TITLE,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                Width = Constants.PANEL_WIDTH,
                HeightSizingMode = SizingMode.AutoSize,
                Parent = rootFlowPanel
            };

            var hintLabel = new HintLabel(ignoredItemsWrapperFlowPanel, Constants.ZERO_HEIGHT_EMPTY_LABEL);

            _ignoredItemsFlowPanel = new FlowPanel
            {
                FlowDirection = ControlFlowDirection.LeftToRight,
                Width = Constants.PANEL_WIDTH,
                HeightSizingMode = SizingMode.AutoSize,
                Parent = ignoredItemsWrapperFlowPanel
            };

            var ignoredItems = _services.Model.IgnoredItemApiIds.Select(i => _services.Model.ItemById[i]).ToList();
            var noItemsAreIgnored = _services.Model.IgnoredItemApiIds.IsEmpty();
            if (noItemsAreIgnored)
            {
                ShowNoItemsAreIgnoredHintIfNecessary(hintLabel, _services);
                return;
            }

            var ignoredItemsApiDataMissing = ignoredItems.Any(i => i.Details.State == ApiStatDetailsState.MissingBecauseApiNotCalledYet);
            if (ignoredItemsApiDataMissing)
            {
                ShowLoadingHint(hintLabel);
                return;
            }

            hintLabel.Text = $"{Constants.HINT_IN_PANEL_PADDING}Left click an item to unignore it.";

            foreach (var ignoredItem in ignoredItems)
                ShowIgnoredItem(ignoredItem, _services, hintLabel, _ignoredItemsFlowPanel);

            unignoreAllButton.Enabled = true;
            unignoreAllButton.Click += (sender, args) =>
            {
                foreach (var statContainer in _ignoredItemsFlowPanel.Children.ToList())
                    statContainer.Dispose(); // this removes it from flowPanel, too.
                
                _services.Model.IgnoredItemApiIds.Clear();
                _services.UpdateLoop.TriggerUpdateUi();
                _services.UpdateLoop.TriggerSaveModel();
                
                ShowNoItemsAreIgnoredHintIfNecessary(hintLabel, _services);
            };
        }

        private static void ShowIgnoredItem(Stat ignoredItem, Services services, HintLabel hintLabel, Container parent)
        {
            var statContainer = new StatContainer(ignoredItem, PanelType.IgnoredItems, services)
            {
                Parent = parent
            };

            statContainer.Click += (sender, args) =>
            {
                UnignoreItem(ignoredItem, services);
                statContainer.Dispose();
                ShowNoItemsAreIgnoredHintIfNecessary(hintLabel, services);
            };
        }

        private static void UnignoreItem(Stat item, Services services)
        {
            services.Model.IgnoredItemApiIds.Remove(item.ApiId);
            services.UpdateLoop.TriggerUpdateUi();
            services.UpdateLoop.TriggerSaveModel();
        }

        private static void ShowNoItemsAreIgnoredHintIfNecessary(HintLabel hintLabel, Services services)
        {
            if (services.Model.IgnoredItemApiIds.Any())
                return;

            hintLabel.Text = 
                $"{Constants.HINT_IN_PANEL_PADDING}No items are ignored.\n" +
                $"{Constants.HINT_IN_PANEL_PADDING}You can ignore items by right clicking them\n" +
                $"{Constants.HINT_IN_PANEL_PADDING}in the '{FarmingTrackerWindowService.SUMMARY_TAB_TITLE}' tab.";
        }

        private static void ShowLoadingHint(HintLabel hintLabel)
        {
            hintLabel.Text =
                $"{Constants.HINT_IN_PANEL_PADDING}This tab will not refresh automatically.\n" +
                $"{Constants.HINT_IN_PANEL_PADDING}Go to '{FarmingTrackerWindowService.SUMMARY_TAB_TITLE}' tab and " +
                $"wait until the '{Constants.UPDATING_HINT_TEXT}' hint disappears.\n" +
                $"{Constants.HINT_IN_PANEL_PADDING}Then come back here and your ignored items will be displayed.";
        }

        private readonly Services _services;
        private FlowPanel _ignoredItemsFlowPanel;
        private const string IGNORED_ITEMS_PANEL_TITLE = "Ignored Items";
    }
}
