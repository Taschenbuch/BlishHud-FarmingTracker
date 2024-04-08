using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;
using System;

namespace FarmingTracker
{
    public class ModuleSettingsView : View
    {
        public ModuleSettingsView(Services services)
        {
            _services = services;
        }

        protected override void Unload()
        {
            _services.Drf.DrfConnectionStatusChanged -= OnDrfConnectionStatusChanged;
            _drfConnectionStatusValueLabel = null;
        }

        protected override void Build(Container buildPanel)
        {
            var rootFlowPanel = new FlowPanel
            {
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                CanScroll = true,
                OuterControlPadding = new Vector2(10, 20),
                ControlPadding = new Vector2(0, 10),
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.Fill,
                Parent = buildPanel
            };

            var font = _services.FontService.Fonts[ContentService.FontSize.Size18];

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

            var xAlignedPosition = drfConnectionStatusTitleLabel.Right + 10;

            _drfConnectionStatusValueLabel = new Label
            {
                Text = "", // set later
                Font = font,
                StrokeText = true,
                AutoSizeHeight = true,
                AutoSizeWidth = true,
                Location = new Point(xAlignedPosition + 2, 10),
                Parent = drfConnectionStatusPanel,
            };

            _services.Drf.DrfConnectionStatusChanged += OnDrfConnectionStatusChanged;
            OnDrfConnectionStatusChanged();

            var drfTokenPanel = new Panel
            {
                WidthSizingMode = SizingMode.AutoSize,
                HeightSizingMode = SizingMode.AutoSize,
                Parent = rootFlowPanel,
            };

            var drfTokenLabel = new Label
            {
                Text = "DRF Token:",
                Font = font,
                AutoSizeHeight = true,
                AutoSizeWidth = true,
                Location = new Point(0, 10),
                Parent = drfTokenPanel,
            };

            var drfTokenInputFlowPanel = new FlowPanel
            {
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                WidthSizingMode = SizingMode.AutoSize,
                HeightSizingMode = SizingMode.AutoSize,
                Location = new Point(xAlignedPosition, 0),
                Parent = drfTokenPanel,
            };

            // todo tooltip für label und textbox
            // todo verbot + häckchen icon nutzen?
            var drfTokenTextBox = new DrfTokenTextBox(_services.SettingService.DrfToken.Value, font, drfTokenInputFlowPanel);

            var drfTokenHintLabel = new Label
            {
                Text = CreateDrfTokenHintText(_services.SettingService.DrfToken.Value),
                TextColor = Color.Yellow,
                Font = font,
                AutoSizeHeight = true,
                AutoSizeWidth = true,
                Parent = drfTokenInputFlowPanel,
            };

            drfTokenTextBox.SanitizedTextChanged += (s, e) =>
            {
                _services.SettingService.DrfToken.Value = drfTokenTextBox.Text;
                drfTokenHintLabel.Text = CreateDrfTokenHintText(drfTokenTextBox.Text);
            };

            ControlFactory.CreateHintLabel( // todo besseren ort für umfassendere help überlegen
                rootFlowPanel,
                "How to get a DRF Token:\n" +
                "1. Open https://drf.rs in your browser.\n" + // todo clickable link
                "2. Create a drf account and link it with your GW2 Account(s).\n" +
                "3. Open the drf.rs settings page.\n" +
                "4. Click on 'Regenerate Token'.\n" +
                "5. Copy the 'DRF Token'.\n" +
                "6. Paste it with CTRL + V into the text input here.");
        }

        private void OnDrfConnectionStatusChanged(object sender = null, EventArgs e = null)
        {
            var drfConnectionStatus = _services.Drf.DrfConnectionStatus;
            _drfConnectionStatusValueLabel.Text = GetDrfConnectionStatusText(drfConnectionStatus);
            _drfConnectionStatusValueLabel.TextColor = GetDrfConnectionStatusTextColor(drfConnectionStatus);
        }

        private Color GetDrfConnectionStatusTextColor(DrfConnectionStatus drfConnectionStatus)
        {
            switch (drfConnectionStatus)
            {
                case DrfConnectionStatus.Connecting:
                    return Color.Yellow;
                case DrfConnectionStatus.Connected:
                    return Color.LightGreen;
                case DrfConnectionStatus.Disconnected:
                case DrfConnectionStatus.AuthenticationFailed:
                    return RED;
                default:
                    Module.Logger.Error($"Fallback: white. Because switch case missing or should not be be handled here: {nameof(drfConnectionStatus)}.{drfConnectionStatus}.");
                    return RED;
            }
        }

        private string GetDrfConnectionStatusText(DrfConnectionStatus drfConnectionStatus)
        {
            var smileyVerticalSpace = "  ";
            switch (drfConnectionStatus)
            {
                case DrfConnectionStatus.Disconnected:
                    return $"Disconnected{smileyVerticalSpace}:-(";
                case DrfConnectionStatus.Connecting:
                    return "Connecting...";
                case DrfConnectionStatus.Connected:
                    return $"Connected{smileyVerticalSpace}:-)";
                case DrfConnectionStatus.AuthenticationFailed:
                    return $"Authentication failed. Add a valid DRF Token!{smileyVerticalSpace}:-(";
                case DrfConnectionStatus.ModuleError:
                    return $"Module Error.{smileyVerticalSpace}:-( Report bug on Discord: https://discord.com/invite/FYKN3qh";
                default:
                    Module.Logger.Error($"Fallback: Unknown Status. Because switch case missing or should not be be handled here: {nameof(drfConnectionStatus)}.{drfConnectionStatus}.");
                    return $"Unknown Status.{smileyVerticalSpace}:-(";
            }
        }

        private static string CreateDrfTokenHintText(string drfToken)
        {
            var drfTokenFormat = DrfToken.ValidateFormat(drfToken);

            switch (drfTokenFormat)
            {
                case DrfTokenFormat.ValidFormat:
                    return "";
                case DrfTokenFormat.InvalidFormat:
                    return "Incomplete or invalid DRF Token format.\nExpected format:\nxxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx with x = a-f, 0-9";
                case DrfTokenFormat.EmptyToken:
                    return "DRF Token required.\nModule wont work without it.";
                default:
                    Module.Logger.Error($"Fallback: no hint. Because switch case missing or should not be be handled here: {nameof(DrfTokenFormat)}.{drfTokenFormat}.");
                    return "";
            }
        }

        private readonly Services _services;
        private Label _drfConnectionStatusValueLabel;
        private readonly Color RED = new Color(255, 120, 120);
    }
}