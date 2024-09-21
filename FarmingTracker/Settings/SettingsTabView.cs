using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;
using System;
using System.Threading.Tasks;
using MonoGame.Extended.BitmapFonts;

namespace FarmingTracker
{
    public class SettingsTabView : View
    {
        public SettingsTabView(Services services)
        {
            _services = services;
        }

        protected override void Unload()
        {
            _rootFlowPanel?.Dispose();
            _rootFlowPanel = null;
            _services.Drf.DrfConnectionStatusChanged -= OnDrfConnectionStatusChanged;
            _services.SettingService.CountBackgroundOpacitySetting.SettingChanged -= OnSettingChanged;
            _services.SettingService.CountBackgroundColorSetting.SettingChanged -= OnSettingChanged;
            _services.SettingService.PositiveCountTextColorSetting.SettingChanged -= OnSettingChanged;
            _services.SettingService.NegativeCountTextColorSetting.SettingChanged -= OnSettingChanged;
            _services.SettingService.CountFontSizeSetting.SettingChanged -= OnSettingChanged;
            _services.SettingService.CountHoritzontalAlignmentSetting.SettingChanged -= OnSettingChanged;
            _services.SettingService.StatIconSizeSetting.SettingChanged -= OnSettingChanged;
            _services.SettingService.NegativeCountIconOpacitySetting.SettingChanged -= OnSettingChanged;
            _services.SettingService.RarityIconBorderIsVisibleSetting.SettingChanged -= OnSettingChanged;
            _drfConnectionStatusValueLabel = null;
        }

        protected override async void Build(Container buildPanel)
        {
            _rootFlowPanel = new FlowPanel
            {
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                CanScroll = true,
                ControlPadding = new Vector2(0, 20),
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.Fill,
                Parent = buildPanel
            };

            var font = _services.FontService.Fonts[ContentService.FontSize.Size16];
            CreateDrfConnectionStatusLabel(font, _rootFlowPanel);
            await Task.Delay(1); // hack: this prevents that the collapsed drf token panel is permanently invisible after switching tabs back and forth
            CreateSetupDrfTokenPanel(font, _rootFlowPanel);

            var miscSettingsFlowPanel = new SettingsFlowPanel(_rootFlowPanel, "Misc");
            new SettingControl(miscSettingsFlowPanel, _services.SettingService.WindowVisibilityKeyBindingSetting);
            var automaticResetSettingsPanel = new AutomaticResetSettingsPanel(miscSettingsFlowPanel, _services);
            
            var countSettingsFlowPanel = new SettingsFlowPanel(_rootFlowPanel, "Count");
            new SettingControl(countSettingsFlowPanel, _services.SettingService.CountBackgroundOpacitySetting);
            new SettingControl(countSettingsFlowPanel, _services.SettingService.CountBackgroundColorSetting);
            new SettingControl(countSettingsFlowPanel, _services.SettingService.PositiveCountTextColorSetting);
            new SettingControl(countSettingsFlowPanel, _services.SettingService.NegativeCountTextColorSetting);
            new SettingControl(countSettingsFlowPanel, _services.SettingService.CountFontSizeSetting);
            new SettingControl(countSettingsFlowPanel, _services.SettingService.CountHoritzontalAlignmentSetting);
            
            var iconSettingsFlowPanel = new SettingsFlowPanel(_rootFlowPanel, "Icon");
            CreateIconSizeDropdown(iconSettingsFlowPanel, _services);
            new SettingControl(iconSettingsFlowPanel, _services.SettingService.NegativeCountIconOpacitySetting);
            new SettingControl(iconSettingsFlowPanel, _services.SettingService.RarityIconBorderIsVisibleSetting);

            var profitWindowSettingsFlowPanel = new SettingsFlowPanel(_rootFlowPanel, "Profit window");
            new FixedWidthHintLabel(
                profitWindowSettingsFlowPanel,
                Constants.LABEL_WIDTH, // -20 as a buffer because wrapping sometimes cut off text.
                "A small window which shows the profit. It is permanently visible even when the main farming tracker window is not visible.");
            new SettingControl(profitWindowSettingsFlowPanel, _services.SettingService.IsProfitWindowVisibleSetting);
            new SettingControl(profitWindowSettingsFlowPanel, _services.SettingService.DragProfitWindowWithMouseIsEnabledSetting);
            new SettingControl(profitWindowSettingsFlowPanel, _services.SettingService.ProfitWindowCanBeClickedThroughSetting);
            new SettingControl(profitWindowSettingsFlowPanel, _services.SettingService.WindowAnchorSetting);
            new SettingControl(profitWindowSettingsFlowPanel, _services.SettingService.ProfitWindowBackgroundOpacitySetting);
            new SettingControl(profitWindowSettingsFlowPanel, _services.SettingService.ProfitWindowDisplayModeSetting);
            new SettingControl(profitWindowSettingsFlowPanel, _services.SettingService.ProfitLabelTextSetting);
            new SettingControl(profitWindowSettingsFlowPanel, _services.SettingService.ProfitPerHourLabelTextSetting);

            _services.SettingService.CountBackgroundOpacitySetting.SettingChanged += OnSettingChanged;
            _services.SettingService.CountBackgroundColorSetting.SettingChanged += OnSettingChanged;
            _services.SettingService.PositiveCountTextColorSetting.SettingChanged += OnSettingChanged;
            _services.SettingService.NegativeCountTextColorSetting.SettingChanged += OnSettingChanged;
            _services.SettingService.CountFontSizeSetting.SettingChanged += OnSettingChanged;
            _services.SettingService.StatIconSizeSetting.SettingChanged += OnSettingChanged;
            _services.SettingService.CountHoritzontalAlignmentSetting.SettingChanged += OnSettingChanged;
            _services.SettingService.NegativeCountIconOpacitySetting.SettingChanged += OnSettingChanged;
            _services.SettingService.RarityIconBorderIsVisibleSetting.SettingChanged += OnSettingChanged;
        }

        private void OnSettingChanged<T>(object sender, ValueChangedEventArgs<T> e)
        {
            _services.UpdateLoop.TriggerUpdateUi();
        }

        private void CreateIconSizeDropdown(Container parent, Services services)
        {
            var settingTooltipText = services.SettingService.StatIconSizeSetting.GetDescriptionFunc();

            var iconSizePanel = new Panel
            {
                BasicTooltipText = settingTooltipText,
                HeightSizingMode = SizingMode.AutoSize,
                WidthSizingMode = SizingMode.AutoSize,
                Parent = parent
            };

            var iconSizeLabel = new Label
            {
                Text = services.SettingService.StatIconSizeSetting.GetDisplayNameFunc(),
                BasicTooltipText = settingTooltipText,
                Top = 4,
                Left = 5,
                AutoSizeWidth = true,
                AutoSizeHeight = true,
                Parent = iconSizePanel,
            };

            var iconSizeDropDown = new Dropdown
            {
                BasicTooltipText = settingTooltipText,
                Left = iconSizeLabel.Right + 5,
                Width = 60,
                Parent = iconSizePanel
            };

            foreach (string dropDownValue in Enum.GetNames(typeof(StatIconSize)))
                iconSizeDropDown.Items.Add(dropDownValue);

            iconSizeDropDown.SelectedItem = services.SettingService.StatIconSizeSetting.Value.ToString();
            iconSizeDropDown.ValueChanged += (s, o) => services.UpdateLoop.TriggerUpdateUi();
            iconSizeDropDown.ValueChanged += (s, o) =>
            {
                services.SettingService.StatIconSizeSetting.Value = (StatIconSize)Enum.Parse(typeof(StatIconSize), iconSizeDropDown.SelectedItem);
            };
        }

        private void CreateDrfConnectionStatusLabel(BitmapFont font, FlowPanel rootFlowPanel)
        {
            var drfConnectionStatusPanel = new Panel
            {
                WidthSizingMode = SizingMode.AutoSize,
                HeightSizingMode = SizingMode.AutoSize,
                Parent = rootFlowPanel,
            };

            var drfConnectionStatusTitleLabel = new Label
            {
                Text = $"{Constants.DRF_CONNECTION_LABEL_TEXT}:",
                Font = font,
                AutoSizeHeight = true,
                AutoSizeWidth = true,
                Location = new Point(0, 10),
                Parent = drfConnectionStatusPanel,
            };

            _drfConnectionStatusValueLabel = new Label
            {
                Text = "", // set later
                Font = font,
                StrokeText = true,
                AutoSizeHeight = true,
                AutoSizeWidth = true,
                Location = new Point(drfConnectionStatusTitleLabel.Right + 5, 10),
                Parent = drfConnectionStatusPanel,
            };

            _services.Drf.DrfConnectionStatusChanged += OnDrfConnectionStatusChanged;
            OnDrfConnectionStatusChanged();
        }

        private void CreateSetupDrfTokenPanel(BitmapFont font, Container parent)
        {
            var setupDrfWrapperContainer = new AutoSizeContainer(parent);

            var addDrfTokenFlowPanel = new FlowPanel
            {
                Title = Constants.FULL_HEIGHT_EMPTY_LABEL,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                BackgroundColor = Color.Black * 0.5f,
                CanCollapse = true,
                Collapsed = true,
                OuterControlPadding = new Vector2(5, 5),
                ControlPadding = new Vector2(0, 10),
                Width = Constants.PANEL_WIDTH,
                HeightSizingMode = SizingMode.AutoSize,
                Parent = setupDrfWrapperContainer,
            };

            // yellow panel title label
            new ClickThroughLabel()
            {
                Text = "Setup DRF (click)",
                TextColor = Color.Yellow,
                Font = _services.FontService.Fonts[ContentService.FontSize.Size20],
                AutoSizeHeight = true,
                AutoSizeWidth = true,
                Top = 6,
                Left = 10,
                Parent = setupDrfWrapperContainer,
            };

            var drfTokenInputPanel = new Panel
            {
                WidthSizingMode = SizingMode.AutoSize,
                HeightSizingMode = SizingMode.AutoSize,
                Parent = addDrfTokenFlowPanel,
            };

            var drfTokenTooltip = "Add DRF token from DRF website here. How generate this token is described below.";

            var drfTokenLabel = new Label
            {
                Text = "DRF Token:",
                BasicTooltipText = drfTokenTooltip,
                Font = font,
                AutoSizeHeight = true,
                AutoSizeWidth = true,
                Location = new Point(0, 10),
                Parent = drfTokenInputPanel,
            };

            var drfTokenTextBoxFlowPanel = new FlowPanel
            {
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                WidthSizingMode = SizingMode.AutoSize,
                HeightSizingMode = SizingMode.AutoSize,
                Location = new Point(drfTokenLabel.Right + 10, 0),
                Parent = drfTokenInputPanel,
            };

            var drfTokenTextBox = new DrfTokenTextBox(_services.SettingService.DrfTokenSetting.Value, drfTokenTooltip, font, drfTokenTextBoxFlowPanel);

            var drfTokenValidationLabel = new Label
            {
                Text = DrfToken.CreateDrfTokenHintText(_services.SettingService.DrfTokenSetting.Value),
                TextColor = Color.Yellow,
                Font = font,
                AutoSizeHeight = true,
                AutoSizeWidth = true,
                Parent = drfTokenTextBoxFlowPanel,
            };

            drfTokenTextBox.SanitizedTextChanged += (s, e) =>
            {
                _services.SettingService.DrfTokenSetting.Value = drfTokenTextBox.Text;
                drfTokenValidationLabel.Text = DrfToken.CreateDrfTokenHintText(drfTokenTextBox.Text);
            };
            
            SetupInstructions.CreateSetupInstructions(font, addDrfTokenFlowPanel, _services);
        }

        private void OnDrfConnectionStatusChanged(object? sender = null, EventArgs? e = null)
        {
            if(_drfConnectionStatusValueLabel == null)
            {
                Module.Logger.Error("DRF status label missing.");
                return;
            }

            var drfConnectionStatus = _services.Drf.DrfConnectionStatus;            
            _drfConnectionStatusValueLabel.TextColor = DrfConnectionStatusService.GetDrfConnectionStatusTextColor(drfConnectionStatus);
            _drfConnectionStatusValueLabel.Text = DrfConnectionStatusService.GetSettingTabDrfConnectionStatusText(
                drfConnectionStatus,
                _services.Drf.ReconnectTriesCounter,
                _services.Drf.ReconnectDelaySeconds);
        }

        private readonly Services _services;
        private Label? _drfConnectionStatusValueLabel;
        private FlowPanel? _rootFlowPanel;
    }
}