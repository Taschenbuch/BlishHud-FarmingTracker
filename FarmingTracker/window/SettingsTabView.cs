using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Settings.UI.Views;
using Blish_HUD.Settings;
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
            _drfConnectionStatusValueLabel = null;
        }

        protected override async void Build(Container buildPanel)
        {
            var rootFlowPanel = new FlowPanel
            {
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                CanScroll = true,
                ControlPadding = new Vector2(0, 20),
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.Fill,
                Parent = buildPanel
            };

            var font = _services.FontService.Fonts[ContentService.FontSize.Size16];
            CreateDrfStatusLabel(font, rootFlowPanel);
            await Task.Delay(1); // hack: this prevents that the collapsed flowpanel is permanently invisible after switching tabs back and forth
            CreateAddDrfTokenPanel(font, rootFlowPanel);
            CreateSetting(rootFlowPanel, _services.SettingService.WindowVisibilityKeyBindingSetting);
        }

        private void CreateDrfStatusLabel(BitmapFont font, FlowPanel rootFlowPanel)
        {
            var drfConnectionStatusPanel = new Panel
            {
                WidthSizingMode = SizingMode.AutoSize,
                HeightSizingMode = SizingMode.AutoSize,
                Parent = rootFlowPanel,
            };

            var drfConnectionStatusTitleLabel = new Label
            {
                Text = "DRF Connection:",
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

        private void CreateAddDrfTokenPanel(BitmapFont font, FlowPanel rootFlowPanel)
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
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                Parent = rootFlowPanel,
            };

            var drfTokenInputPanel = new Panel
            {
                WidthSizingMode = SizingMode.AutoSize,
                HeightSizingMode = SizingMode.AutoSize,
                Parent = addDrfTokenFlowPanel,
            };

            var drfTokenLabel = new Label
            {
                Text = "DRF Token:",
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

            // todo tooltip für label und textbox
            // todo verbot + häckchen icon nutzen?            
            var drfTokenTextBox = new DrfTokenTextBox(_services.SettingService.DrfTokenSetting.Value, font, drfTokenTextBoxFlowPanel);

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
            ControlFactory.CreateHintLabel(addDrfTokenFlowPanel, "\nSetup DRF DLL and DRF account:", font);
            ControlFactory.CreateHintLabel(
                addDrfTokenFlowPanel,
                "1. Click the button below and follow the instructions to setup the drf.dll.\n" +
                "2. Create a drf account on the website and link it with your GW2 Account(s).");

            CreateButtonToOpenUrlInDefaultBrowser("https://drf.rs/getting-started", "Show drf.dll setup instructions", buttonTooltip, addDrfTokenFlowPanel);
            ControlFactory.CreateHintLabel(addDrfTokenFlowPanel, "Test DRF:", font);
            ControlFactory.CreateHintLabel(
                addDrfTokenFlowPanel,
                "1. Click the button below to open the DRF web live tracker.\n" +
                "2. Use this web live tracker to check if the tracking is working.\n" +
                "e.g. by opening an unidentified gear.\n" +
                "The items should appear almost instantly in the web live tracker.");

            CreateButtonToOpenUrlInDefaultBrowser("https://drf.rs/dashboard/livetracker", "Open DRF web live tracker", buttonTooltip, addDrfTokenFlowPanel);
            
            ControlFactory.CreateHintLabel(addDrfTokenFlowPanel, "Does NOT work? :-( DRF Discord can help:", font);
            CreateButtonToOpenUrlInDefaultBrowser("https://discord.gg/VSgehyHkrD", "Open DRF Discord", "Open DRF discord in your default web browser.", addDrfTokenFlowPanel);
            
            ControlFactory.CreateHintLabel(addDrfTokenFlowPanel, "Is working? :-) Get the DRF Token:", font);
            ControlFactory.CreateHintLabel(
                addDrfTokenFlowPanel,
                "1. Click the button below to open the drf.rs settings page.\n" +
                "2. Click on 'Regenerate Token'.\n" +
                "3. Copy the 'DRF Token'.\n" +
                "4. Paste the DRF Token with CTRL + V into the DRF token input above.\n" +
                "5. Done! Open the first tab again to see the tracked items/currencies :-)");

            CreateButtonToOpenUrlInDefaultBrowser("https://drf.rs/dashboard/user/settings", "Open DRF web settings", buttonTooltip, addDrfTokenFlowPanel);

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

        private static ViewContainer CreateSetting(Container parent, SettingEntry settingEntry)
        {
            var viewContainer = new ViewContainer { Parent = parent };
            viewContainer.Show(SettingView.FromType(settingEntry, parent.Width));
            return viewContainer;
        }

        private readonly Services _services;
        private Label _drfConnectionStatusValueLabel;
    }
}