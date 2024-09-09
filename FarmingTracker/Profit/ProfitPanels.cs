using Blish_HUD;
using Blish_HUD.Controls;
using MonoGame.Extended.BitmapFonts;
using System;
using System.Diagnostics;
using System.Linq;
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

            _stopwatch.Restart();
            SetTotalAndPerHourProfit(0, 0);

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

        public void UpdateProfitLabels(StatsSnapshot snapshot, SafeList<int> ignoredItemApiIds, TimeSpan elapsedFarmingTime)
        {
            var totalProfitInCopper = CalculateTotalProfitInCopper(snapshot, ignoredItemApiIds);
            var profitPerHourInCopper = CalculateProfitPerHourInCopper(totalProfitInCopper, elapsedFarmingTime);
            SetTotalAndPerHourProfit(totalProfitInCopper, profitPerHourInCopper);
        }

        public void UpdateProfitPerHourEveryFiveSeconds(TimeSpan elapsedFarmingTime)
        {
            var time = _stopwatch.Elapsed;
            var fiveSecondsHavePassed = time >= _oldTime + TimeSpan.FromSeconds(Constants.PROFIT_PER_HOUR_UPDATE_INTERVAL_IN_SECONDS);
            if (fiveSecondsHavePassed)
            {
                var profitPerHourInCopper = CalculateProfitPerHourInCopper(_profitInCopper, elapsedFarmingTime);
                _profitPerHourPanel.SetProfit(profitPerHourInCopper);
                _profitTooltip.ProfitPerHourPanel.SetProfit(profitPerHourInCopper);
                _oldTime = time;
            }
        }

        private void SetTotalAndPerHourProfit(long profitInCopper, long profitPerHourInCopper)
        {
            _profitPanel.SetProfit(profitInCopper);
            _profitPerHourPanel.SetProfit(profitPerHourInCopper);
            _profitTooltip.ProfitPerHourPanel.SetProfit(profitPerHourInCopper);
            _profitInCopper = profitInCopper;
        }

        private static long CalculateTotalProfitInCopper(StatsSnapshot snapshot, SafeList<int> ignoredItemApiIds)
        {
            var coinsInCopper = snapshot.CurrencyById.Values.SingleOrDefault(s => s.IsCoin)?.Count ?? 0;
            
            var ignoredItemApiIdsCopy = ignoredItemApiIds.ToListSafe();
            var itemsSellProfitInCopper = snapshot.ItemById.Values
                .Where(s => !ignoredItemApiIdsCopy.Contains(s.ApiId))
                .Sum(s => s.CountSign * s.Profits.All.MaxProfitInCopper);

            var totalProfit = coinsInCopper + itemsSellProfitInCopper;

            if(Module.DebugEnabled)
                Module.Logger.Debug(
                    $"totalProfit {totalProfit} = coinsInCopper {coinsInCopper} + itemsSellProfitInCopper {itemsSellProfitInCopper} | " +
                    $"maxProfitsPerItem {string.Join(" ", snapshot.ItemById.Values.Select(s => s.CountSign * s.Profits.All.MaxProfitInCopper))}");

            return totalProfit;
        }

        private static long CalculateProfitPerHourInCopper(long totalProfitInCopper, TimeSpan elapsedFarmingTime)
        {
            if (totalProfitInCopper == 0)
                return 0;

            var sessionJustStarted = elapsedFarmingTime.TotalSeconds < 1;
            if (sessionJustStarted) // otherwise value per hour would be inflated
                return 0;

            var profitPerHourInCopper = totalProfitInCopper / elapsedFarmingTime.TotalHours;

            if (profitPerHourInCopper > long.MaxValue)
                return long.MaxValue;

            if (profitPerHourInCopper <= long.MinValue)
                return long.MinValue + 1; // hack: +1 to prevent that Math.Abs() crashes, because (-1 * long.MinValue) is bigger than long.MaxValue.

            return (long)profitPerHourInCopper;
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

        private void OnProfitLabelTextSettingChanged(object sender = null, ValueChangedEventArgs<string> e = null)
        {
            if(_isProfitWindow)
            {
                _profitLabel.Text = $" {_settingService.ProfitLabelTextSetting.Value}"; // blank as padding because sign label should get no control padding from flowPanel.
                _profitPerHourLabel.Text = $" {_settingService.ProfitPerHourLabelTextSetting.Value}";
            }
            else
            {
                // do not modify profit labels in 6farming tracker main window.
                _profitLabel.Text = $" Profit"; // blank as padding because sign label should get no control padding from flowPanel.
                _profitPerHourLabel.Text = $" Profit per hour";
            }
        }

        private void OnProfitWindowDisplayModeSettingChanged(object sender = null, ValueChangedEventArgs<ProfitWindowDisplayMode> e = null)
        {
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
        private readonly Stopwatch _stopwatch = new Stopwatch();  // do not use elapsedFarmingTime, because it can be resetted and maybe other stuff in the future.
        private readonly SettingService _settingService;
        private readonly bool _isProfitWindow;
        private TimeSpan _oldTime = TimeSpan.Zero;
        private long _profitInCopper;
    }
}
