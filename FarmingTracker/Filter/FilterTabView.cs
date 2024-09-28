using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace FarmingTracker
{
    public class FilterTabView : View
    {
        public FilterTabView(Services services)
        {
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
                ControlPadding = new Vector2(20, 10),
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.Fill,
                Parent = buildPanel
            };

            var collapsibleHelp = new CollapsibleHelp(
                "- Checked = visible.\n" +
                "- Unchecked = hidden by filter.\n" +
                $"- Items hidden by filters are still included in the profit calculation and are still shown in the '{Constants.FAVORITE_ITEMS_PANEL_TITLE}' panel.\n" +
                "- A filter, e.g. rarity filter, will not be applied if all its checkboxes are unchecked. In this case no items will be hidden by the filter.\n" +
                "- filter icon on filter panel header:\n" +
                "TRANSPARENT: filter wont hide stats.\n" +
                "OPAQUE: filter will hide stats.\n" +
                "- expand/collapse panels: for a better overview expand/collapse the filter panels by using the expand/collapse-all-buttons or by clicking on the filter panel headers.",
                buildPanel.ContentRegion.Width - Constants.SCROLLBAR_WIDTH_OFFSET, // buildPanel because other Panels dont have correctly updated width yet.
                _rootFlowPanel);

            buildPanel.ContentResized += (s, e) =>
            {
                collapsibleHelp.UpdateSize(e.CurrentRegion.Width - Constants.SCROLLBAR_WIDTH_OFFSET);
            };

            var buttonFlowPanel = new FlowPanel
            {
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
                ControlPadding = new Vector2(5, 0),
                WidthSizingMode = SizingMode.AutoSize,
                HeightSizingMode = SizingMode.AutoSize,
                Parent = _rootFlowPanel
            };

            var filterPanels = new List<FlowPanel>();
            var expandAllButton = new StandardButton
            {
                Text = "Expand all",
                Width = 90,
                Parent = buttonFlowPanel
            };

            var collapseAllButton = new StandardButton
            {
                Text = "Collapse all",
                Width = 90,
                Parent = buttonFlowPanel
            };

            expandAllButton.Click += (s, e) =>
            {
                foreach (var filterPanel in filterPanels)
                    filterPanel.Expand();
            };

            collapseAllButton.Click += (s, e) =>
            {
                foreach (var filterPanel in filterPanels)
                    filterPanel.Collapse();
            };

            filterPanels.Add(CreateFilterSettingPanel("Count (items & currencies)", Constants.ALL_COUNTS, _services.SettingService.CountFilterSetting, _services, _rootFlowPanel, "Coin will never be hidden."));
            filterPanels.Add(CreateFilterSettingPanel("Sell Methods (items & currencies)", Constants.ALL_SELL_METHODS, _services.SettingService.SellMethodFilterSetting, _services, _rootFlowPanel, $"{MATCH_MULTIPLE_OPTION_HINT}\nRaw gold (coin) will always be visible regardless of which of these filters\nare active or not."));
            filterPanels.Add(CreateFilterSettingPanel("Rarity (items)", Constants.ALL_ITEM_RARITIES, _services.SettingService.RarityStatsFilterSetting, _services, _rootFlowPanel));
            filterPanels.Add(CreateFilterSettingPanel("Type (items)", Constants.ALL_ITEM_TYPES, _services.SettingService.TypeStatsFilterSetting, _services, _rootFlowPanel));
            filterPanels.Add(CreateFilterSettingPanel("Flag (items)", Constants.ALL_ITEM_FLAGS, _services.SettingService.FlagStatsFilterSetting, _services, _rootFlowPanel, MATCH_MULTIPLE_OPTION_HINT));
            filterPanels.Add(CreateFilterSettingPanel("Currencies", Constants.ALL_CURRENCIES, _services.SettingService.CurrencyFilterSetting, _services, _rootFlowPanel));
            filterPanels.Add(CreateFilterSettingPanel("GW2 API (items & currencies)", Constants.ALL_KNOWN_BY_API, _services.SettingService.KnownByApiFilterSetting, _services, _rootFlowPanel, "Coin will never be hidden. Some items like the lvl-80-boost or\ncertain reknown heart items are not known by the GW2 API."));
        }

        private static FlowPanel CreateFilterSettingPanel<T>(
            string panelTitel,
            T[] allPossibleFilterElements, 
            SettingEntry<List<T>> filterSettingEntry,
            Services services, 
            Container parent,
            string hintText = "") where T : notnull
        {
            var filterIconPanel = new Panel
            {
                WidthSizingMode = SizingMode.AutoSize,
                HeightSizingMode = SizingMode.AutoSize,
                Parent = parent
            };

            var filterFlowPanel = new FlowPanel
            {
                Title = panelTitel,
                ShowBorder = true, // hack: adds some padding at the bottom. otherwise last checkbox touches flowpanel bottom border.
                CanCollapse = true,
                BackgroundColor = Color.Black * 0.5f,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                OuterControlPadding = new Vector2(10, 10),
                ControlPadding = new Vector2(0, 10),
                Width = Constants.PANEL_WIDTH,
                HeightSizingMode = SizingMode.AutoSize,
                Parent = filterIconPanel
            };

            var filterIcon = new ClickThroughImage(services.TextureService.FilterTabIconTexture, new Point(380, 3), filterIconPanel);

            if (!string.IsNullOrWhiteSpace(hintText))
                new HintLabel(filterFlowPanel, hintText);

            var buttonFlowPanel = new FlowPanel
            {
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
                ControlPadding = new Vector2(10, 0),
                WidthSizingMode = SizingMode.AutoSize,
                HeightSizingMode = SizingMode.AutoSize,
                Parent = filterFlowPanel
            };

            var showAllButton = new StandardButton
            {
                Text = "Show all",
                Width = 80,
                Parent = buttonFlowPanel
            };

            var hideAllButton = new StandardButton
            {
                Text = "Hide all",
                Width = 80,
                Parent = buttonFlowPanel
            };

            var filterCheckboxes = new List<Checkbox>();
            var selectedFilterElements = filterSettingEntry.Value.ToList();
            UpdateOpacity(filterIcon, selectedFilterElements, allPossibleFilterElements);

            foreach (var filterElement in allPossibleFilterElements)
            {
                var filterCheckbox = new Checkbox()
                {
                    Text = Helper.ConvertEnumValueToTextWithBlanks(filterElement.ToString()),
                    Checked = selectedFilterElements.Contains(filterElement),
                    Parent = filterFlowPanel,
                };

                filterCheckboxes.Add(filterCheckbox);

                filterCheckbox.CheckedChanged += (s, e) =>
                {
                    if (filterCheckbox.Checked)
                    {
                        if (!selectedFilterElements.Contains(filterElement))
                            selectedFilterElements.Add(filterElement);
                    }
                    else
                    {
                        if (selectedFilterElements.Contains(filterElement))
                            selectedFilterElements.Remove(filterElement);
                    }

                    UpdateOpacity(filterIcon, selectedFilterElements, allPossibleFilterElements);
                    filterSettingEntry.Value = selectedFilterElements;
                    services.UpdateLoop.TriggerUpdateUi();
                };
            }

            hideAllButton.Click += (s, e) =>
            {
                foreach (var filterCheckbox in filterCheckboxes)
                    filterCheckbox.Checked = false;
            };

            showAllButton.Click += (s, e) =>
            {
                foreach (var filterCheckbox in filterCheckboxes)
                    filterCheckbox.Checked = true;
            };

            return filterFlowPanel;
        }

        private static void UpdateOpacity<T>(ClickThroughImage filterIcon, List<T> selectedFilterElements, T[] allPossibleFilterElements)
        {
            var noneSelected = !selectedFilterElements.Any();
            var allSelected = selectedFilterElements.Count == allPossibleFilterElements.Count();
            var filterIsInactive = noneSelected || allSelected;
            filterIcon.SetOpacity(filterIsInactive);
        }

        private readonly Services _services;
        private FlowPanel? _rootFlowPanel;
        private const string MATCH_MULTIPLE_OPTION_HINT = "Some items match several of these options. These items are only hidden\nif all matching options are unselected.";
    }
}
