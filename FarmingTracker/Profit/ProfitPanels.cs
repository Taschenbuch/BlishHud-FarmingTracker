﻿using Blish_HUD;
using Blish_HUD.Controls;
using MonoGame.Extended.BitmapFonts;
using static Blish_HUD.ContentService;

namespace FarmingTracker
{
    public class ProfitPanels : FlowPanel
    {
        public ProfitPanels(Services services, bool isProfitWindow, Container parent)
        {
            _settingService = services.SettingService;
            _isProfitWindow = isProfitWindow;
            _profitTooltip = new ProfitTooltip(services);

            Tooltip = _profitTooltip;
            FlowDirection = ControlFlowDirection.SingleTopToBottom;
            WidthSizingMode = SizingMode.AutoSize;
            HeightSizingMode = SizingMode.AutoSize;
            Parent = parent;

            var font = services.FontService.Fonts[FontSize.Size16];
            
            _profitPanel = new CoinsPanel(_profitTooltip, font, services.TextureService, this);
            _profitPerHourPanel = new CoinsPanel(_profitTooltip, font, services.TextureService, this);

            _profitLabel = CreateProfitLabel(_profitTooltip, font, _profitPanel);
            _profitPerHourLabel = CreateProfitLabel(_profitTooltip, font, _profitPerHourPanel);

            ShowProfits(0, 0);

            services.SettingService.ProfitPerHourLabelTextSetting.SettingChanged += OnProfitLabelTextSettingChanged;
            services.SettingService.ProfitLabelTextSetting.SettingChanged += OnProfitLabelTextSettingChanged;
            services.SettingService.ProfitWindowDisplayModeSetting.SettingChanged += OnProfitWindowDisplayModeSettingChanged;
            OnProfitLabelTextSettingChanged();
            OnProfitWindowDisplayModeSettingChanged();
        }

        protected override void DisposeControl()
        {
            _settingService.ProfitPerHourLabelTextSetting.SettingChanged -= OnProfitLabelTextSettingChanged;
            _settingService.ProfitLabelTextSetting.SettingChanged -= OnProfitLabelTextSettingChanged;
            _settingService.ProfitWindowDisplayModeSetting.SettingChanged -= OnProfitWindowDisplayModeSettingChanged;
            _profitTooltip?.Dispose();
            base.DisposeControl();
        }

        public void ShowProfits(long signed_profitInCopper, long signed_profitPerHourInCopper)
        {
            _profitPanel.SetCoins(signed_profitInCopper);
            _profitPerHourPanel.SetCoins(signed_profitPerHourInCopper);
            _profitTooltip.ProfitPerHourPanel.SetCoins(signed_profitPerHourInCopper);
        }

        private Label CreateProfitLabel(ProfitTooltip profitTooltip, BitmapFont font, CoinsPanel parent)
        {
            return new Label
            {
                Font = font,
                Tooltip = profitTooltip,
                AutoSizeHeight = true,
                AutoSizeWidth = true,
                Parent = parent,
            };
        }

        private void OnProfitLabelTextSettingChanged(object? sender = null, ValueChangedEventArgs<string>? e = null)
        {
            if(_isProfitWindow)
            {
                _profitLabel.Text = $" {_settingService.ProfitLabelTextSetting.Value}"; // blank as padding because sign label should get no control padding from flowPanel.
                _profitPerHourLabel.Text = $" {_settingService.ProfitPerHourLabelTextSetting.Value}";
            }
            else
            {
                // do not modify profit labels in farming tracker main window.
                _profitLabel.Text = $" Profit"; // blank as padding because sign label should get no control padding from flowPanel.
                _profitPerHourLabel.Text = $" Profit per hour";
            }
        }

        private void OnProfitWindowDisplayModeSettingChanged(object? sender = null, ValueChangedEventArgs<ProfitWindowDisplayMode>? e = null)
        {
            if(!_isProfitWindow) // do not modify what is displayed in farming tracker main window.
                return;

            _profitPanel.Parent = null;
            _profitPerHourPanel.Parent = null;

            switch (_settingService.ProfitWindowDisplayModeSetting.Value)
            {
                case ProfitWindowDisplayMode.ProfitAndProfitPerHour:
                    _profitPanel.Parent = this;
                    _profitPerHourPanel.Parent = this;
                    break;
                case ProfitWindowDisplayMode.Profit:
                    _profitPanel.Parent = this;
                    break;
                case ProfitWindowDisplayMode.ProfitPerHour:
                    _profitPerHourPanel.Parent = this;
                    break;
            }
        }

        private readonly ProfitTooltip _profitTooltip;
        private readonly CoinsPanel _profitPanel;
        private readonly CoinsPanel _profitPerHourPanel;
        private readonly Label _profitLabel;
        private readonly Label _profitPerHourLabel;
        private readonly SettingService _settingService;
        private readonly bool _isProfitWindow;
    }
}
