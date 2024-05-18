using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace FarmingTracker
{
    public class FilterTabView : View
    {
        public FilterTabView(Services services)
        {
            _services = services;
        }

        protected override void Build(Container buildPanel)
        {
            var rootFlowPanel = new FlowPanel
            {
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                CanScroll = true,
                ControlPadding = new Vector2(20, 10),
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.Fill,
                Parent = buildPanel
            };

            new CollapsibleHelp(
                "- Checked = visible.\n" +
                "- Unchecked = hidden by filter.\n" +
                "- A filter, e.g. rarity filter, will not be applied if all its checkboxes are unchecked. In this case no items are hidden by the filter.\n" +
                "- Items hidden by filters are still included in the profit calculation.",
                450, 
                rootFlowPanel);

            var buttonFlowPanel = new FlowPanel
            {
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
                ControlPadding = new Vector2(5, 0),
                WidthSizingMode = SizingMode.AutoSize,
                HeightSizingMode = SizingMode.AutoSize,
                Parent = rootFlowPanel
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

            filterPanels.Add(CreateFilterSettingPanel("Count (items & currencies)", Constants.ALL_COUNTS, _services.SettingService.CountFilterSetting, _services, rootFlowPanel));
            filterPanels.Add(CreateFilterSettingPanel("Sell Methods (items)", Constants.ALL_SELL_METHODS, _services.SettingService.SellMethodFilterSetting, _services, rootFlowPanel));
            filterPanels.Add(CreateFilterSettingPanel("Rarity (items)", Constants.ALL_ITEM_RARITIES, _services.SettingService.RarityStatsFilterSetting, _services, rootFlowPanel));
            filterPanels.Add(CreateFilterSettingPanel("Type (items)", Constants.ALL_ITEM_TYPES, _services.SettingService.TypeStatsFilterSetting, _services, rootFlowPanel));
            filterPanels.Add(CreateFilterSettingPanel("Flag (items)", Constants.ALL_ITEM_FLAGS, _services.SettingService.FlagStatsFilterSetting, _services, rootFlowPanel));
            filterPanels.Add(CreateFilterSettingPanel("Currencies", Constants.ALL_CURRENCIES, _services.SettingService.CurrencyFilterSetting, _services, rootFlowPanel));
        }

        private static FlowPanel CreateFilterSettingPanel<T>(
            string panelTitel, 
            T[] allPossibleFilterElements, 
            SettingEntry<List<T>> filterSettingEntry, 
            Services services, 
            FlowPanel rootFlowPanel)
        {
            var filterFlowPanel = new FlowPanel
            {
                Title = panelTitel,
                ShowBorder = true,
                CanCollapse = true,
                BackgroundColor = Color.Black * 0.5f,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                OuterControlPadding = new Vector2(10, 10),
                ControlPadding = new Vector2(0, 10),
                Width = 300,
                HeightSizingMode = SizingMode.AutoSize,
                Parent = rootFlowPanel
            };

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
            foreach (var filterElement in allPossibleFilterElements)
            {
                var filter = filterSettingEntry.Value;

                var filterCheckbox = new Checkbox()
                {
                    Text = AddBlanksBetweenUpperCasedWords(filterElement.ToString()),
                    Checked = filter.Contains(filterElement),
                    Parent = filterFlowPanel,
                };

                filterCheckboxes.Add(filterCheckbox);

                filterCheckbox.CheckedChanged += (s, e) =>
                {
                    if (filterCheckbox.Checked)
                    {
                        if (!filter.Contains(filterElement))
                            filter.Add(filterElement);
                    }
                    else
                    {
                        if (filter.Contains(filterElement))
                            filter.Remove(filterElement);
                    }

                    filterSettingEntry.Value = filter;
                    services.UpdateLoop.TriggerUpdateStatPanels();
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

        private static string AddBlanksBetweenUpperCasedWords(string textWithUpperCasedWords)
        {
            var textWithBlanks = "";

            foreach (var character in textWithUpperCasedWords)
                textWithBlanks += char.IsUpper(character)
                    ? $" {character}" // blank
                    : $"{character}"; // no blank

            return textWithBlanks;
        }

        private readonly Services _services;
    }
}
