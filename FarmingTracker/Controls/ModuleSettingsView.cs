using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;
using System;
using System.Threading.Tasks;

namespace FarmingTracker
{
    public class ModuleSettingsView : View, IDisposable
    {
        public ModuleSettingsView(Services services)
        {
            _services = services;
        }

        // workaround until Unload-is-not-called-bug is fixed in blish core.
        public void Dispose() 
        {
            Unload();
        }

        protected override void Unload() // bug: that is currently not called in blish 1.1.1 due to a bug
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
                OuterControlPadding = new Vector2(10, 5),
                ControlPadding = new Vector2(0, 20),
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

            var xAlignLabelPadding = 5;

            var drfConnectionStatusTitleLabel = new Label
            {
                Text = "DRF Connection:",
                Font = font,
                AutoSizeHeight = true,
                AutoSizeWidth = true,
                Location = new Point(xAlignLabelPadding, 10),
                Parent = drfConnectionStatusPanel,
            };

            var xAlignValueLocation = drfConnectionStatusTitleLabel.Right + 10;

            _drfConnectionStatusValueLabel = new Label
            {
                Text = "", // set later
                Font = font,
                StrokeText = true,
                AutoSizeHeight = true,
                AutoSizeWidth = true,
                Location = new Point(xAlignValueLocation + 10, 10),
                Parent = drfConnectionStatusPanel,
            };

            _services.Drf.DrfConnectionStatusChanged -= OnDrfConnectionStatusChanged; // hack: until bug is fixed that SettingsView.Unload() is not called.
            _services.Drf.DrfConnectionStatusChanged += OnDrfConnectionStatusChanged;
            OnDrfConnectionStatusChanged();

            await Task.Delay(1); // hack: this prevents that the collapsed flowpanel is permanently invisible after switching tabs back and forth

            var drfTokenInputFlowPanel = new FlowPanel
            {
                Title = "Add DRF Token (click)",
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                CanCollapse = true,
                Collapsed = true,
                OuterControlPadding = new Vector2(xAlignLabelPadding, 5),
                ControlPadding = new Vector2(0, 10),
                WidthSizingMode = SizingMode.AutoSize,
                HeightSizingMode = SizingMode.AutoSize,
                Parent = rootFlowPanel,
            };

            var drfTokenInputPanel = new Panel
            {
                WidthSizingMode = SizingMode.AutoSize,
                HeightSizingMode = SizingMode.AutoSize,
                Parent = drfTokenInputFlowPanel,
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
                Location = new Point(xAlignValueLocation, 0),
                Parent = drfTokenInputPanel,
            };

            // todo tooltip für label und textbox
            // todo verbot + häckchen icon nutzen?            
            var drfTokenTextBox = new DrfTokenTextBox(_services.SettingService.DrfToken.Value, font, drfTokenTextBoxFlowPanel);

            var drfTokenValidationLabel = new Label
            {
                Text = DrfToken.CreateDrfTokenHintText(_services.SettingService.DrfToken.Value),
                TextColor = Color.Yellow,
                Font = font,
                AutoSizeHeight = true,
                AutoSizeWidth = true,
                Parent = drfTokenTextBoxFlowPanel,
            };

            drfTokenTextBox.SanitizedTextChanged += (s, e) =>
            {
                _services.SettingService.DrfToken.Value = drfTokenTextBox.Text;
                drfTokenValidationLabel.Text = DrfToken.CreateDrfTokenHintText(drfTokenTextBox.Text);
            };

            var drfTokenHelpLabel = new Label // todo besseren ort für umfassendere help überlegen
            {
                Text = "How to get a DRF Token:\n" +
                "1. Open https://drf.rs in your browser.\n" + // todo clickable link
                "2. Create a drf account and link it with your GW2 Account(s).\n" +
                "3. Open the drf.rs settings page.\n" +
                "4. Click on 'Regenerate Token'.\n" +
                "5. Copy the 'DRF Token'.\n" +
                "6. Paste it with CTRL + V into the text input here.",
                AutoSizeHeight = true,
                AutoSizeWidth = true,
                Parent = drfTokenInputFlowPanel,
            };
        }

        private void OnDrfConnectionStatusChanged(object sender = null, EventArgs e = null)
        {
            var drfConnectionStatus = _services.Drf.DrfConnectionStatus;
            _drfConnectionStatusValueLabel.Text = DrfConnectionStatusService.GetDrfConnectionStatusText(drfConnectionStatus);
            _drfConnectionStatusValueLabel.TextColor = DrfConnectionStatusService.GetDrfConnectionStatusTextColor(drfConnectionStatus);
        }

        private readonly Services _services;
        private Label _drfConnectionStatusValueLabel;
    }
}