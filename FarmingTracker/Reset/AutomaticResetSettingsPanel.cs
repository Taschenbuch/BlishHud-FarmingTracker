using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FarmingTracker
{
    public class AutomaticResetSettingsPanel : FlowPanel
    {
        public AutomaticResetSettingsPanel(Container parent, Services services)
        {
            _services = services;
            CreateAutomaticResetDropDown(parent, services.SettingService);
            CreateMinutesDropDown(parent, services.SettingService);
        }

        private static void CreateAutomaticResetDropDown(Container parent, SettingService settingService)
        {
            var automaticResetPanel = new Panel()
            {
                BasicTooltipText = settingService.AutomaticResetSetting.GetDescriptionFunc(),
                HeightSizingMode = SizingMode.AutoSize,
                WidthSizingMode = SizingMode.AutoSize,
                Parent = parent
            };

            var automaticResetLabel = new Label
            {
                Text = settingService.AutomaticResetSetting.GetDisplayNameFunc(),
                BasicTooltipText = settingService.AutomaticResetSetting.GetDescriptionFunc(),
                Location = new Point(5, 4),
                AutoSizeHeight = true,
                AutoSizeWidth = true,
                Parent = automaticResetPanel
            };

            var automaticResetDropDown = new Dropdown()
            {
                BasicTooltipText = settingService.AutomaticResetSetting.GetDescriptionFunc(),
                Location = new Point(automaticResetLabel.Right + 5, 0),
                Width = 370,
                Parent = automaticResetPanel,
            };

            var dropDownTextDict = GetDropDownTextsForAutomaticSessionResetSetting();

            foreach (var dropDownText in dropDownTextDict.Values)
                automaticResetDropDown.Items.Add(dropDownText);

            automaticResetDropDown.SelectedItem = dropDownTextDict[settingService.AutomaticResetSetting.Value];
            automaticResetDropDown.ValueChanged += (s, e) =>
            {
                settingService.AutomaticResetSetting.Value = dropDownTextDict.First(d => d.Value == e.CurrentValue).Key;
            };
        }

        private void CreateMinutesDropDown(Container parent, SettingService settingService)
        {
            _resetMinutesPanel = new Panel
            {
                BasicTooltipText = settingService.MinutesUntilResetAfterModuleShutdownSetting.GetDescriptionFunc(),
                HeightSizingMode = SizingMode.AutoSize,
                WidthSizingMode = SizingMode.AutoSize,
                Parent = parent
            };

            var minutesDropDown = new Dropdown
            {
                BasicTooltipText = settingService.MinutesUntilResetAfterModuleShutdownSetting.GetDescriptionFunc(),
                Location = new Point(5, 0),
                Width = 60,
                Parent = _resetMinutesPanel
            };

            new Label 
            {
                Text = settingService.MinutesUntilResetAfterModuleShutdownSetting.GetDisplayNameFunc(),
                BasicTooltipText = settingService.MinutesUntilResetAfterModuleShutdownSetting.GetDescriptionFunc(),
                Location = new Point(minutesDropDown.Right + 5, 4),
                AutoSizeWidth = true,
                AutoSizeHeight = true,
                Parent = _resetMinutesPanel,
            };

            var dropDownValues = new List<int> { 15, 30, 45, 60, 90, 120, 180, 240, 360, 480, 600, 720, 840, 960, 1080, 1200 }.Select(m => m.ToString());
            foreach (string dropDownValue in dropDownValues)
                minutesDropDown.Items.Add(dropDownValue);

            minutesDropDown.SelectedItem = settingService.MinutesUntilResetAfterModuleShutdownSetting.Value.ToString();
            minutesDropDown.ValueChanged += (s, o) =>
            {
                settingService.MinutesUntilResetAfterModuleShutdownSetting.Value = int.Parse(minutesDropDown.SelectedItem);
            };

            settingService.AutomaticResetSetting.SettingChanged += AutomaticResetSettingChanged;
            AutomaticResetSettingChanged();
        }

        protected override void DisposeControl()
        {
            _services.SettingService.AutomaticResetSetting.SettingChanged -= AutomaticResetSettingChanged;
            base.DisposeControl();
        }

        private void AutomaticResetSettingChanged(object sender = null, ValueChangedEventArgs<AutomaticReset> e = null)
        {
            // opacity because show()/hide() is not reliable. it does not collapse the height in a flowpanel after hding. And sometimes after show() it overlaps with other settings.
            _resetMinutesPanel.Opacity = _services.SettingService.AutomaticResetSetting.Value == AutomaticReset.MinutesAfterModuleShutdown 
                ? 1f 
                : 0f;
        }
        private static Dictionary<AutomaticReset, string> GetDropDownTextsForAutomaticSessionResetSetting()
        {
            return new Dictionary<AutomaticReset, string>
            {
                [AutomaticReset.Never] = "Never (right click menu icon for manual reset)",
                [AutomaticReset.OnModuleStart] = "On module start",
                [AutomaticReset.OnDailyReset] = $"On daily reset ({GetNextDailyResetLocalTime()})",
                [AutomaticReset.OnWeeklyReset] = $"On weekly reset ({GetNextWeeklyResetInLocalTime(AutomaticReset.OnWeeklyReset)})",
                [AutomaticReset.OnWeeklyNaWvwReset] = $"On weekly NA WvW reset ({GetNextWeeklyResetInLocalTime(AutomaticReset.OnWeeklyNaWvwReset)})",
                [AutomaticReset.OnWeeklyEuWvwReset] = $"On weekly EU WvW reset ({GetNextWeeklyResetInLocalTime(AutomaticReset.OnWeeklyEuWvwReset)})",
                [AutomaticReset.OnWeeklyMapBonusRewardsReset] = $"On weekly map bonus rewards reset ({GetNextWeeklyResetInLocalTime(AutomaticReset.OnWeeklyMapBonusRewardsReset)})",
                [AutomaticReset.MinutesAfterModuleShutdown] = $"Minutes after module shutdown (change minutes below)"
            };
        }

        private static string GetNextWeeklyResetInLocalTime(AutomaticReset automaticSessionReset)
        {
            return NextAutomaticResetCalculator.GetNextResetDateTimeUtc(DateTime.UtcNow, automaticSessionReset, DOES_NOT_MATTER)
                .ToLocalTime()
                .ToString("dddd HH:mm");
        }

        private static string GetNextDailyResetLocalTime()
        {
            return NextAutomaticResetCalculator.GetNextResetDateTimeUtc(DateTime.UtcNow, AutomaticReset.OnDailyReset, DOES_NOT_MATTER)
                .ToLocalTime()
                .ToString("HH:mm");
        }

        private const int DOES_NOT_MATTER = 60;
        private readonly Services _services;
        private Panel _resetMinutesPanel;
    }
}
