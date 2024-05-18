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

        protected override void Unload()
        {
            // todo 
        }

        protected override void Build(Container buildPanel)
        {
            var rootFlowPanel = new FlowPanel
            {
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                CanScroll = true,
                ControlPadding = new Vector2(20, 20),
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.Fill,
                Parent = buildPanel
            };

            ControlFactory.CreateHintLabel(rootFlowPanel, "Items that are hidden by filters are still included in the profit calculation.");
            CreateFilterSettingPanel("Rarity", Constants.ALL_ITEM_RARITIES, _services.SettingService.RarityStatsFilterSetting, _services, rootFlowPanel);
            CreateFilterSettingPanel("Type", Constants.ALL_ITEM_TYPES, _services.SettingService.TypeStatsFilterSetting, _services, rootFlowPanel);
            CreateFilterSettingPanel("Flag", Constants.ALL_ITEM_FLAGS, _services.SettingService.FlagStatsFilterSetting, _services, rootFlowPanel);
        }

        private static void CreateFilterSettingPanel<T>(
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
                BackgroundColor = Microsoft.Xna.Framework.Color.Black * 0.5f,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                OuterControlPadding = new Vector2(10, 10),
                ControlPadding = new Vector2(0, 10),
                Width = 300,
                HeightSizingMode = SizingMode.AutoSize,
                Parent = rootFlowPanel
            };

            var allButton = new StandardButton
            {
                Text = "All",
                Width = 50,
                Parent = filterFlowPanel
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
                    services.UpdateLoop.TriggerUiUpdate();
                };
            }

            allButton.Click += (s, e) =>
            {
                foreach (var filterCheckbox in filterCheckboxes)
                    filterCheckbox.Checked = true;
            };
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
