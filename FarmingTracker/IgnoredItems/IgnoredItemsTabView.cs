using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;
using System.Linq;

namespace FarmingTracker
{
    public class IgnoredItemsTabView : View
    {
        public IgnoredItemsTabView(Model model, Services services)
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
                $"IGNORE ITEM:\n" +
                $"In the '{Constants.TabTitles.SUMMARY}' tab right click on an item icon in the 'Items' panel to ignore it.\n" +
                $"\n" +
                $"UNIGNORE ITEM:\n" +
                $"left click on an item here to unignore it.\n" +
                $"\n" +
                $"WHY IGNORE?\n" +
                $"An ignored item will appear here. It is hidden in the '{Constants.TabTitles.SUMMARY}' tab" +
                $" and does not contribute to profit calculations." +
                $" That can be usefull to prevent that none-legendary equipment that you swap manually is tracked accidently.",
                buildPanel.ContentRegion.Width - Constants.SCROLLBAR_WIDTH_OFFSET, // buildPanel because other Panels dont have correctly updated width yet.
                _rootFlowPanel);

            buildPanel.ContentResized += (s, e) =>
            {
                collapsibleHelp.UpdateSize(e.CurrentRegion.Width - Constants.SCROLLBAR_WIDTH_OFFSET);
            };

            var flowPanelWithButtonContainer = new AutoSizeContainer(_rootFlowPanel);

            var ignoredItemsWrapperFlowPanel = new FlowPanel
            {
                Title = IGNORED_ITEMS_PANEL_TITLE,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                Width = buildPanel.ContentRegion.Width - Constants.SCROLLBAR_WIDTH_OFFSET,
                HeightSizingMode = SizingMode.AutoSize,
                Parent = flowPanelWithButtonContainer
            };

            var unignoreAllButton = new StandardButton
            {
                Text = "Unignore all items",
                Enabled = false,
                Width = 150,
                Top = 5,
                Right = buildPanel.ContentRegion.Width - Constants.SCROLLBAR_WIDTH_OFFSET,
                Parent = flowPanelWithButtonContainer
            };

            var hintLabel = new HintLabel(ignoredItemsWrapperFlowPanel, Constants.ZERO_HEIGHT_EMPTY_LABEL);

            var ignoredItemsFlowPanel = new FlowPanel
            {
                FlowDirection = ControlFlowDirection.LeftToRight,
                HeightSizingMode = SizingMode.AutoSize,
                WidthSizingMode = SizingMode.Fill,
                Parent = ignoredItemsWrapperFlowPanel
            };

            buildPanel.ContentResized += (s, e) =>
            {
                ignoredItemsWrapperFlowPanel.Width = e.CurrentRegion.Width - Constants.SCROLLBAR_WIDTH_OFFSET;
                unignoreAllButton.Right = e.CurrentRegion.Width - Constants.SCROLLBAR_WIDTH_OFFSET;
            };

            var ignoredItems = _model.IgnoredItemApiIds.ToListSafe().Select(i => _model.Stats.StatsSnapshot.ItemById[i]).ToList();
            var noItemsAreIgnored = ignoredItems.IsEmpty();
            if (noItemsAreIgnored)
            {
                ShowNoItemsAreIgnoredHintIfNecessary(hintLabel, _model);
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
                ShowIgnoredItem(ignoredItem, _model, _services, hintLabel, ignoredItemsFlowPanel);

            unignoreAllButton.Enabled = true;
            unignoreAllButton.Click += (sender, args) =>
            {
                foreach (var statContainer in ignoredItemsFlowPanel.Children.ToList())
                    statContainer.Dispose(); // this removes it from flowPanel, too.
                
                _model.IgnoredItemApiIds.ClearSafe();
                _services.UpdateLoop.TriggerUpdateUi();
                _services.UpdateLoop.TriggerSaveModel();
                
                ShowNoItemsAreIgnoredHintIfNecessary(hintLabel, _model);
            };
        }

        private static void ShowIgnoredItem(Stat ignoredItem, Model model, Services services, HintLabel hintLabel, Container parent)
        {
            var statContainer = new StatContainer(ignoredItem, PanelType.IgnoredItems, model.IgnoredItemApiIds, model.FavoriteItemApiIds, model.CustomStatProfits, services)
            {
                Parent = parent
            };

            statContainer.Click += (sender, args) =>
            {
                UnignoreItem(ignoredItem, model, services);
                statContainer.Dispose();
                ShowNoItemsAreIgnoredHintIfNecessary(hintLabel, model);
            };
        }

        private static void UnignoreItem(Stat item, Model model, Services services)
        {
            model.IgnoredItemApiIds.RemoveSafe(item.ApiId);
            services.UpdateLoop.TriggerUpdateUi();
            services.UpdateLoop.TriggerSaveModel();
        }

        private static void ShowNoItemsAreIgnoredHintIfNecessary(HintLabel hintLabel, Model model)
        {
            if (model.IgnoredItemApiIds.AnySafe())
                return;

            hintLabel.Text = 
                $"{Constants.HINT_IN_PANEL_PADDING}No items are ignored.\n" +
                $"{Constants.HINT_IN_PANEL_PADDING}You can ignore an item by right clicking it in the '{Constants.TabTitles.SUMMARY}' tab.";
        }

        private static void ShowLoadingHint(HintLabel hintLabel)
        {
            hintLabel.Text =
                $"{Constants.HINT_IN_PANEL_PADDING}This tab will not refresh automatically.\n" +
                $"{Constants.HINT_IN_PANEL_PADDING}Go to '{Constants.TabTitles.SUMMARY}' tab and " +
                $"wait until the '{Constants.UPDATING_HINT_TEXT}' hint disappears.\n" +
                $"{Constants.HINT_IN_PANEL_PADDING}Then come back here and your ignored items will be displayed.";
        }

        private readonly Model _model;
        private readonly Services _services;
        private FlowPanel? _rootFlowPanel;
        private const string IGNORED_ITEMS_PANEL_TITLE = "Ignored Items";
    }
}
