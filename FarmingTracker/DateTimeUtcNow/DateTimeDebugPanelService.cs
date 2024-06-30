using Blish_HUD.Controls;
using Blish_HUD.Settings;
using System.Collections.Generic;
using System;
using System.Linq;
using Microsoft.Xna.Framework;

namespace FarmingTracker
{
    public class DateTimeDebugPanelService
    {
        public static void CreateDateTimeDebugPanel(Container parent, SettingEntry<bool> debugDateTimeEnabledSetting, SettingEntry<DateTime> debugDateTimeValueSetting)
        {
            var debugDateTimeFlowPanel = new FlowPanel
            {
                Title = "DateTime Debug",
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                OuterControlPadding = new Vector2(5, 5),
                ShowBorder = true,
                BackgroundColor = Color.Black * 0.3f,
                WidthSizingMode = SizingMode.AutoSize,
                HeightSizingMode = SizingMode.AutoSize,
                Parent = parent
            };

            new SettingControl(debugDateTimeFlowPanel, debugDateTimeEnabledSetting);

            var dateTimeTextBox = new TextBox()
            {
                Text = debugDateTimeValueSetting.Value.ToString(),
                BasicTooltipText = "new debug UTC for date time mocking. mainly for reset",
                Width = 200,
                Parent = debugDateTimeFlowPanel
            };

            var setDateTimeButton = new StandardButton
            {
                BasicTooltipText = "click to apply as new debug UTC",
                Width = 210,
                Parent = debugDateTimeFlowPanel
            };

            var setToRealDateTimeButton = new StandardButton
            {
                Text = "Update textbox with real UTC",
                BasicTooltipText = "This just replaces the text in the textbox with the real current UTC. it does NOT start mocking with this time. press other button for that",
                Width = 210,
                Parent = debugDateTimeFlowPanel
            };

            setToRealDateTimeButton.Click += (s, e) =>
            {
                dateTimeTextBox.Text = DateTime.UtcNow.ToString();
            };

            setDateTimeButton.Click += (s, e) =>
            {
                DateTimeService.UtcNow = debugDateTimeValueSetting.Value;
            };

            debugDateTimeEnabledSetting.SettingChanged += (s, o) => setDateTimeButton.Enabled = debugDateTimeEnabledSetting.Value;
            setDateTimeButton.Enabled = debugDateTimeEnabledSetting.Value;

            var resetDateTimelabelDict = new Dictionary<AutomaticReset, Label>();
            var dateTimeBasedAutomaticResets = Enum.GetValues(typeof(AutomaticReset))
                .Cast<AutomaticReset>()
                .Where(a => a != AutomaticReset.Never && a != AutomaticReset.OnModuleStart)
                .ToList();

            foreach (var automaticReset in dateTimeBasedAutomaticResets)
                resetDateTimelabelDict[automaticReset] = new Label
                {
                    Text = automaticReset.ToString(),
                    AutoSizeHeight = true,
                    AutoSizeWidth = true,
                    Parent = debugDateTimeFlowPanel
                };

            dateTimeTextBox.TextChanged += (s, o) =>
            {
                if(DateTime.TryParse(dateTimeTextBox.Text, out DateTime dateTimeText))
                    debugDateTimeValueSetting.Value = dateTimeText;
                
                UpdateDateTimeButtonText(setDateTimeButton, dateTimeTextBox.Text); // parameter to prevent that method is moved above label creation -> crash
                UpdateResetDateTimeLabels(resetDateTimelabelDict, dateTimeBasedAutomaticResets, dateTimeTextBox.Text);
            };

            UpdateDateTimeButtonText(setDateTimeButton, dateTimeTextBox.Text);
            UpdateResetDateTimeLabels(resetDateTimelabelDict, dateTimeBasedAutomaticResets, dateTimeTextBox.Text);
        }

        private static void UpdateResetDateTimeLabels(
            Dictionary<AutomaticReset, Label> resetDateTimelabelDict,
            List<AutomaticReset> dateTimeBasedAutomaticResets,
            string dateTimeTextBoxText)
        {
            if (!DateTime.TryParse(dateTimeTextBoxText, out var dateTimeUtc))
                return;

            foreach (var automaticReset in dateTimeBasedAutomaticResets)
            {
                var dateTimeText = NextAutomaticResetCalculator.GetNextResetDateTimeUtc(dateTimeUtc, automaticReset, DOES_NOT_MATTER).ToString(DEBUG_DATE_FORMAT);
                resetDateTimelabelDict[automaticReset].Text = $"{dateTimeText} {automaticReset}";
            }
        }

        private static void UpdateDateTimeButtonText(StandardButton button, string dateTimeText)
        {
            if (DateTime.TryParse(dateTimeText, out DateTime mockedDateTimeUtc))
                button.Text = $"SET {mockedDateTimeUtc.ToString(DEBUG_DATE_FORMAT)}";
        }

        private const string DEBUG_DATE_FORMAT = "ddd dd:MM:yyyy HH:mm:ss";
        private const int DOES_NOT_MATTER = 60;
    }
}
