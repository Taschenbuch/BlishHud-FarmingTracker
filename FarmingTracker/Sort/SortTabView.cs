﻿using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace FarmingTracker
{
    public class SortTabView : View
    {
        public SortTabView(Services services)
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
                "- only items are sorted, not currencies.\n" +
                "- custom profit is not taken into account in the profit sorting for now. This may change in the future.\n" +
                "- multiple sorts can be combined. Example:\n" +
                "'sort by' positive/negative count, 'then by' rarity, 'then by' name.\n" +
                "This will first split the items into gained items (positive count) and lost items (negative count). " +
                "Then it will sort the gained items by rarity. " +
                "After that items of same rarity are sorted by name. " +
                "This process is repeated for the lost items.\n" +
                "- sort by API ID is similiar to sort by ITEM TYPE. It can sometimes help grouping similiar items (e.g. material storage items).\n" +
                "- combining multiple sorts can sometimes not produce the expected result. " +
                "e.g. do not sort by API ID or NAME first. Every item has an unique API ID and an unique NAME. " +
                "Because of that it will create groups where each group consists of only 1 item. " +
                "Single item groups cannot be further sorted. Because of that every 'then by' sort after an API ID / NAME sort, will have no effect.",
                buildPanel.ContentRegion.Width - Constants.SCROLLBAR_WIDTH_OFFSET, // buildPanel because other Panels dont have correctly updated width yet.
                _rootFlowPanel);

            buildPanel.ContentResized += (s, e) =>
            {
                collapsibleHelp.UpdateSize(e.CurrentRegion.Width - Constants.SCROLLBAR_WIDTH_OFFSET);
            };

            var allSortsFlowPanel = new FlowPanel
            {
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ControlPadding = new Vector2(0, 5),
                Width = Constants.PANEL_WIDTH,
                WidthSizingMode = SizingMode.AutoSize,
                HeightSizingMode = SizingMode.AutoSize,
                Parent = _rootFlowPanel
            };

            var addNewSortButton = new StandardButton
            {
                Text = "+ Add sort",
                Width = 90,
                Parent = _rootFlowPanel
            };

            var sortPanels = new List<SortPanel>();

            addNewSortButton.Click += (s, e) =>
            {
                var sortByWithDirection = SortByWithDirection.Name_Ascending;
                AddSingleSortPanel(allSortsFlowPanel, sortPanels, sortByWithDirection);
                var sortByList = _services.SettingService.SortByWithDirectionListSetting.Value.ToList();
                sortByList.Add(sortByWithDirection);
                _services.SettingService.SortByWithDirectionListSetting.Value = sortByList;
                _services.UpdateLoop.TriggerUpdateUi();
            };

            foreach (var sortByWithDirection in _services.SettingService.SortByWithDirectionListSetting.Value.ToList())
                AddSingleSortPanel(allSortsFlowPanel, sortPanels, sortByWithDirection);
        }

        private void AddSingleSortPanel(FlowPanel allSortsFlowPanel, List<SortPanel> sortPanels, SortByWithDirection sortByWithDirection)
        {
            var singleSortPanel = new SortPanel(allSortsFlowPanel, sortByWithDirection);
            sortPanels.Add(singleSortPanel);
            SetThenByOrSortByLabels(sortPanels);

            singleSortPanel.Dropdown.ValueChanged += (s, e) =>
            {
                var sortPanelIndex = sortPanels.IndexOf(singleSortPanel);
                if (sortPanelIndex == -1)
                {
                    Module.Logger.Error("Removing sort panel failed. Could not find sort panel.");
                    return;
                }

                var sortByList = _services.SettingService.SortByWithDirectionListSetting.Value.ToList();
                sortByList.RemoveAt(sortPanelIndex);
                sortByList.Insert(sortPanelIndex, singleSortPanel.GetSelectedSortBy());
                _services.SettingService.SortByWithDirectionListSetting.Value = sortByList;
                _services.UpdateLoop.TriggerUpdateUi();
            };

            singleSortPanel.RemoveSortButton.Click += (s, e) =>
            {
                var sortPanelIndex = sortPanels.IndexOf(singleSortPanel);
                if (sortPanelIndex == -1)
                {
                    Module.Logger.Error("Removing sort panel failed. Could not find sort panel.");
                    return;
                }

                var sortByList = _services.SettingService.SortByWithDirectionListSetting.Value.ToList();
                sortByList.RemoveAt(sortPanelIndex);
                _services.SettingService.SortByWithDirectionListSetting.Value = sortByList;
                
                singleSortPanel.Parent = null;
                sortPanels.Remove(singleSortPanel);
                SetThenByOrSortByLabels(sortPanels);
                _services.UpdateLoop.TriggerUpdateUi();
            };
        }

        private static void SetThenByOrSortByLabels(List<SortPanel> sortPanels)
        {
            if (!sortPanels.Any())
                return;

            foreach (var sortPanel in sortPanels)
                sortPanel.SetLabelText("Then by");

            sortPanels[0].SetLabelText("Sort by");
        }


        private readonly Services _services;
        private FlowPanel? _rootFlowPanel;
    }
}
