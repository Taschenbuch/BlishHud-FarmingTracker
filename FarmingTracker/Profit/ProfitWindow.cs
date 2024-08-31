using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;

namespace FarmingTracker
{
    public class ProfitWindow : RelativePositionAndMouseDraggableContainer
    {
        public ProfitWindow(Services services) : base(services.SettingService)
        {
            _services = services;
            HeightSizingMode = SizingMode.AutoSize;
            WidthSizingMode = SizingMode.AutoSize;
            Parent = GameService.Graphics.SpriteScreen;

            ProfitPanels = new ProfitPanels(services, this);
            
            OnProfitWindowBackgroundOpacitySettingChanged();
            OnIsProfitWindowVisibleSettingChanged();
            services.SettingService.ProfitWindowBackgroundOpacitySetting.SettingChanged += OnProfitWindowBackgroundOpacitySettingChanged;
            services.SettingService.IsProfitWindowVisibleSetting.SettingChanged += OnIsProfitWindowVisibleSettingChanged;
        }

        public ProfitPanels ProfitPanels { get; private set; }

        private void OnIsProfitWindowVisibleSettingChanged(object sender = null, ValueChangedEventArgs<bool> e = null)
        {
            Visible = _services.SettingService.IsProfitWindowVisibleSetting.Value;
        }

        private void OnProfitWindowBackgroundOpacitySettingChanged(object sender = null, ValueChangedEventArgs<int> e = null)
        {
            BackgroundColor = Color.Black * (_services.SettingService.ProfitWindowBackgroundOpacitySetting.Value / 255f);
        }

        protected override void DisposeControl()
        {
            _services.SettingService.ProfitWindowBackgroundOpacitySetting.SettingChanged -= OnProfitWindowBackgroundOpacitySettingChanged;
            _services.SettingService.IsProfitWindowVisibleSetting.SettingChanged -= OnIsProfitWindowVisibleSettingChanged;
            base.DisposeControl();
        }

        private readonly Services _services;
    }
}
