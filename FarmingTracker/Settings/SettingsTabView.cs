using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;
using System;
using System.Threading.Tasks;
using System.Diagnostics;
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
            _services.Drf.DrfConnectionStatusChanged -= OnDrfConnectionStatusChanged;
            _services.SettingService.CountBackgroundOpacitySetting.SettingChanged -= OnSettingChanged;
            _services.SettingService.CountBackgroundColorSetting.SettingChanged -= OnSettingChanged;
            _services.SettingService.CountTextColorSetting.SettingChanged -= OnSettingChanged;
            _services.SettingService.CountFontSizeSetting.SettingChanged -= OnSettingChanged;
            _services.SettingService.CountHoritzontalAlignmentSetting.SettingChanged -= OnSettingChanged;
            _services.SettingService.StatIconSizeSetting.SettingChanged -= OnSettingChanged;
            _drfConnectionStatusValueLabel = null;
        }

        protected override async void Build(Container buildPanel)
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

            var font = _services.FontService.Fonts[ContentService.FontSize.Size16];
            CreateDrfConnectionStatusLabel(font, rootFlowPanel);
            await Task.Delay(1); // hack: this prevents that the collapsed drf token panel is permanently invisible after switching tabs back and forth
            CreateSetupDrfTokenPanel(font, rootFlowPanel);
            SettingControls.CreateSetting(rootFlowPanel, _services.SettingService.AutomaticResetSetting);
            SettingControls.CreateSetting(rootFlowPanel, _services.SettingService.WindowVisibilityKeyBindingSetting);
            SettingControls.CreateSetting(rootFlowPanel, _services.SettingService.CountBackgroundOpacitySetting);
            SettingControls.CreateSetting(rootFlowPanel, _services.SettingService.CountBackgroundColorSetting);
            SettingControls.CreateSetting(rootFlowPanel, _services.SettingService.CountTextColorSetting);
            SettingControls.CreateSetting(rootFlowPanel, _services.SettingService.CountFontSizeSetting);
            SettingControls.CreateSetting(rootFlowPanel, _services.SettingService.CountHoritzontalAlignmentSetting);
            CreateIconSizeDropdown(rootFlowPanel, _services);
            SettingControls.CreateSetting(rootFlowPanel, _services.SettingService.RarityIconBorderIsVisibleSetting);

            _services.SettingService.CountBackgroundOpacitySetting.SettingChanged += OnSettingChanged;
            _services.SettingService.CountBackgroundColorSetting.SettingChanged += OnSettingChanged;
            _services.SettingService.CountTextColorSetting.SettingChanged += OnSettingChanged;
            _services.SettingService.CountFontSizeSetting.SettingChanged += OnSettingChanged;
            _services.SettingService.StatIconSizeSetting.SettingChanged += OnSettingChanged;
            _services.SettingService.CountHoritzontalAlignmentSetting.SettingChanged += OnSettingChanged;
        }

        private void OnSettingChanged<T>(object sender, ValueChangedEventArgs<T> e)
        {
            _services.UpdateLoop.TriggerUpdateStatPanels();
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
            iconSizeDropDown.ValueChanged += (s, o) => services.UpdateLoop.TriggerUpdateStatPanels();
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
                Text = $"{DRF_CONNECTION_LABEL_TEXT}:",
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
                Location = new Point(drfConnectionStatusTitleLabel.Right + 20, 10),
                Parent = drfConnectionStatusPanel,
            };

            _services.Drf.DrfConnectionStatusChanged += OnDrfConnectionStatusChanged;
            OnDrfConnectionStatusChanged();
        }

        private void CreateSetupDrfTokenPanel(BitmapFont font, FlowPanel rootFlowPanel)
        {
            var addDrfTokenFlowPanel = new FlowPanel
            {
                Title = "Setup DRF (click)",
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                BackgroundColor = Color.Black * 0.5f,
                CanCollapse = true,
                Collapsed = true,
                OuterControlPadding = new Vector2(5, 5),
                ControlPadding = new Vector2(0, 10),
                Width = Constants.PANEL_WIDTH,
                HeightSizingMode = SizingMode.AutoSize,
                Parent = rootFlowPanel,
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

            var buttonTooltip = "Open DRF website in your default web browser.";
            AddVerticalSpacing(_services, addDrfTokenFlowPanel);
            new HeaderLabel(addDrfTokenFlowPanel, "Prerequisite:", font);
            new HintLabel(
                addDrfTokenFlowPanel,
                "- Windows 8 or newer because DRF requires websocket technolgy.");

            AddVerticalSpacing(_services, addDrfTokenFlowPanel);
            new HeaderLabel(addDrfTokenFlowPanel, "Setup DRF DLL and DRF account:", font);
            new HintLabel(
                addDrfTokenFlowPanel,
                "1. Click the button below and follow the instructions to setup the drf.dll.\n" +
                "2. Create a drf account on the website and link it with\nyour GW2 Account(s).");

            CreateButtonToOpenUrlInDefaultBrowser("https://drf.rs/getting-started", "Open drf.dll setup instructions", buttonTooltip, addDrfTokenFlowPanel);

            AddVerticalSpacing(_services, addDrfTokenFlowPanel);
            var testDrfHeader = "Test DRF";
            new HeaderLabel(addDrfTokenFlowPanel, $"{testDrfHeader}:", font);
            new HintLabel(
                addDrfTokenFlowPanel,
                "1. Click the button below to open the DRF web live tracker.\n" +
                "2. Use this web live tracker to check if the tracking is working.\n" +
                "e.g. by opening an unidentified gear.\n" +
                "The items should appear almost instantly in the web live tracker.");

            CreateButtonToOpenUrlInDefaultBrowser("https://drf.rs/dashboard/livetracker", "Open DRF web live tracker", buttonTooltip, addDrfTokenFlowPanel);

            AddVerticalSpacing(_services, addDrfTokenFlowPanel);
            new HeaderLabel(addDrfTokenFlowPanel, "Does NOT work? :-( Try this:", font);
            new HintLabel(
                addDrfTokenFlowPanel,
                "- After a GW2 patch, you will have to wait until a fixed arcdps version\nis released if you use arcdps to load the drf.dll.\n" +
                "- If you installed drf.dll a while ago, check the drf website whether an\nupdated version of drf.dll is available.\n" +
                "- DRF Discord can help:");

            CreateButtonToOpenUrlInDefaultBrowser("https://discord.gg/VSgehyHkrD", "Open DRF Discord", "Open DRF discord in your default web browser.", addDrfTokenFlowPanel);

            AddVerticalSpacing(_services, addDrfTokenFlowPanel);
            new HeaderLabel(addDrfTokenFlowPanel, "Is working? :-) Get the DRF Token:", font);
            new HintLabel(
                addDrfTokenFlowPanel,
                "1. Click the button below to open the drf.rs settings page.\n" +
                "2. Click on 'Regenerate Token'.\n" +
                "3. Copy the 'DRF Token' by clicking on the copy icon.\n" +
                "4. Paste the DRF Token with CTRL + V into the DRF token input above.\n" +
                "5. Done! Open the first tab again to see the tracked items/currencies :-)");

            CreateButtonToOpenUrlInDefaultBrowser("https://drf.rs/dashboard/user/settings", "Open DRF web settings", buttonTooltip, addDrfTokenFlowPanel);

            AddVerticalSpacing(_services, addDrfTokenFlowPanel);
            new HeaderLabel(addDrfTokenFlowPanel, $"'{DRF_CONNECTION_LABEL_TEXT}' is 'Authentication failed':", font);
            new HintLabel(
                addDrfTokenFlowPanel,
                "- Make sure you copied the DRF token into the module with the\ncopy button and CTRL+V as explained above.\n" +
                "Otherwise you may accidentally copy only part of the token.\n" +
                "In this case the DRF token input above will show you\nthat the format is incomplete/invalid.\n" +
                "- After you have clicked on 'Regenerate Token' on the DRF website, any\nold DRF token you may have used previously will become invalid.\n" +
                "You must add the new token to the module.");

            AddVerticalSpacing(_services, addDrfTokenFlowPanel);
            new HeaderLabel(addDrfTokenFlowPanel, $"'{DRF_CONNECTION_LABEL_TEXT}' is 'Connected' but does not track changes", font);
            new HintLabel(
                addDrfTokenFlowPanel, 
                $"- Currencies and items changes will be shown after the '{Constants.UPDATING_HINT_TEXT}'\nor '{Constants.RESETTING_HINT_TEXT}' hint disappears.\n" +
                $"While those hints are shown the module normally waits for the\nGW2 API.\n" +
                $"If the GW2 API is slow or has a timeout, this can unfortunately\ntake a while.\n" +
                $"- If this is not the case follow the steps from '{testDrfHeader}'");

            AddVerticalSpacing(_services, addDrfTokenFlowPanel);
            new HeaderLabel(addDrfTokenFlowPanel, $"Why is the GW2 API needed?", font);
            new HintLabel(
                addDrfTokenFlowPanel,
                $"- DRF offers only raw data. To get further details like item/currency\nname, description, icon and profits the GW2 API is still needed.\n" +
                $"- The GW2 API is the reason why the module cannot display changes\nto your account immediately but somtimes takes several second\n" +
                $"because it has to wait for the GW2 API responses.");

            AddVerticalSpacing(_services, addDrfTokenFlowPanel);
            new HeaderLabel(addDrfTokenFlowPanel, $"Red bug images appear", font);
            new HintLabel(
                addDrfTokenFlowPanel,
                $"- When the bug image is used for an item/currency:\n" +
                $"hover with the mouse over the bug icon to read the tooltip.\n" +
                $"In most cases the tooltip should mention that those are items missing\nin the GW2 API.\nE.g. lvl-80-boost item or some reknown heart items.\n" +
                $"- If the bug images appears somewhere else in the module's UI or the\nitem tooltip is not mentioning an missing item:\n" +
                $"Reason 1: The item is new and BlishHUD's texture cache does not\nknow the icon yet.\n" +
                $"OR\n" +
                $"Reason 2: You ran BlishHUD as admin at one point and later stopped\nrunning BlishHUD as admin. This causes file permission issues for software\nlike BlishHUD that has to create cache or config data.\n" +
                $"You can try to fix 'Reason 2' by closing BlishHUD and then deleting\nthe 'Blish HUD' folder at 'C:\\ProgramData\\Blish HUD'.");
        }

        private static void AddVerticalSpacing(Services services, FlowPanel addDrfTokenFlowPanel)
        {
            new HeaderLabel(addDrfTokenFlowPanel, "", services.FontService.Fonts[ContentService.FontSize.Size8]);
        }

        private void OnDrfConnectionStatusChanged(object sender = null, EventArgs e = null)
        {
            var drfConnectionStatus = _services.Drf.DrfConnectionStatus;            
            _drfConnectionStatusValueLabel.TextColor = DrfConnectionStatusService.GetDrfConnectionStatusTextColor(drfConnectionStatus);
            _drfConnectionStatusValueLabel.Text = DrfConnectionStatusService.GetSettingTabDrfConnectionStatusText(
                drfConnectionStatus,
                _services.Drf.ReconnectTriesCounter,
                _services.Drf.ReconnectDelaySeconds);
        }

        private static void CreateButtonToOpenUrlInDefaultBrowser(string url, string buttonText, string buttonTooltip, Container parent)
        {
            var patchNotesButton = new StandardButton
            {
                Text = buttonText,
                BasicTooltipText = buttonTooltip,
                Width = 300,
                Parent = parent
            };

            patchNotesButton.Click += (s, e) =>
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            };
        }


        private readonly Services _services;
        private Label _drfConnectionStatusValueLabel;
        private const string DRF_CONNECTION_LABEL_TEXT = "DRF Connection";
    }
}