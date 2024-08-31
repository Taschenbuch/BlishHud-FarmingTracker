using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;

namespace FarmingTracker
{
    public class RelativePositionAndMouseDraggableContainer : Container
    {
        public RelativePositionAndMouseDraggableContainer(SettingService settingService)
        {
            _settingService = settingService;
            SetLocationFromWindowAnchorLocationSettings();
            GameService.Input.Mouse.LeftMouseButtonReleased   += OnLeftMouseButtonReleased;
            GameService.Graphics.SpriteScreen.Resized         += OnSpriteScreenResized;
            settingService.WindowAnchorSetting.SettingChanged += WindowAnchorSettingChanged;
        }

        protected override void DisposeControl()
        {
            GameService.Input.Mouse.LeftMouseButtonReleased    -= OnLeftMouseButtonReleased;
            GameService.Graphics.SpriteScreen.Resized          -= OnSpriteScreenResized;
            _settingService.WindowAnchorSetting.SettingChanged -= WindowAnchorSettingChanged;
            base.DisposeControl();
        }

        // Size changes not only when stats become visible/hidden, but also on blish startup when stats window is constructed.
        // DO NOT force stat window to be within gw2 screen borders here. This will shift the stats window to left/right/top/bottom screen border depending on windowAnchor setting because
        // SpriteScreen.Size will resize multiple times, starting at 40x20, during blish startup.
        public override void RecalculateLayout()
        {
            base.RecalculateLayout(); // not required because Control and Container dot not implement them. But maybe in a future blish version they do.
            SetLocationFromWindowAnchorLocationSettings();
        }

        public override void UpdateContainer(GameTime gameTime)
        {
            if (_settingService.DragProfitWindowWithMouseIsEnabledSetting.Value && _containerIsDraggedByMouse)
            {
                var newLocation = Input.Mouse.Position - _mousePressedLocationInsideContainer;
                AdjustAndSetLocationToKeepWindowAnchorInsideScreenBoundaries(newLocation);
            }
        }

        protected override void OnLeftMouseButtonPressed(MouseEventArgs e)
        {
            if (_settingService.DragProfitWindowWithMouseIsEnabledSetting.Value)
            {
                _containerIsDraggedByMouse           = true;
                _mousePressedLocationInsideContainer = Input.Mouse.Position - Location;
            }

            base.OnLeftMouseButtonPressed(e);
        }

        // not using the override on purpose because it does not register the release when clicking fast (workaround suggested by freesnow)
        private void OnLeftMouseButtonReleased(object sender, MouseEventArgs e)
        {
            _containerIsDraggedByMouse = false;

            if (_settingService.DragProfitWindowWithMouseIsEnabledSetting.Value)
                SaveWindowAnchorLocationInSettings(Location);
        }

        private void WindowAnchorSettingChanged(object sender, ValueChangedEventArgs<WindowAnchor> e)
        {
            AdjustAndSetLocationToKeepWindowAnchorInsideScreenBoundaries(Location);
        }

        private void OnSpriteScreenResized(object sender, ResizedEventArgs resizedEventArgs)
        {
            SetLocationFromWindowAnchorLocationSettings();
        }

        private void AdjustAndSetLocationToKeepWindowAnchorInsideScreenBoundaries(Point location)
        {
            var windowAnchorLocation
                = WindowAnchorService.ConvertBetweenControlAndWindowAnchorLocation(location, Size, ConvertLocation.ToWindowAnchorLocation, _settingService.WindowAnchorSetting.Value);

            var adjustedWindowAnchorLocation = ScreenBoundariesService.AdjustLocationToKeepControlInsideScreenBoundaries(
                windowAnchorLocation, Size, GameService.Graphics.SpriteScreen.Size, _settingService.WindowAnchorSetting.Value);

            var adjustedLocation = WindowAnchorService.ConvertBetweenControlAndWindowAnchorLocation(
                adjustedWindowAnchorLocation, Size, ConvertLocation.ToControlLocation, _settingService.WindowAnchorSetting.Value);

            SaveWindowAnchorLocationInSettings(adjustedLocation);
            Location = adjustedLocation;
        }

        // do not use AdjustCoordinates in this method,
        // because it is called by OnSpriteScreenResized -> would cause unwanted adjusting because of multiple SpriteScreen resizing on module start up (was a user reported bug)
        private void SetLocationFromWindowAnchorLocationSettings()
        {
            var windowAnchorLocation 
                = ConvertCoordinatesService.ConvertRelativeToAbsoluteCoordinates(_settingService.ProfitWindowRelativeWindowAnchorLocationSetting.Value, GameService.Graphics.SpriteScreen.Size);
            Location 
                = WindowAnchorService.ConvertBetweenControlAndWindowAnchorLocation(windowAnchorLocation, Size, ConvertLocation.ToControlLocation, _settingService.WindowAnchorSetting.Value);
        }

        private void SaveWindowAnchorLocationInSettings(Point location)
        {
            var windowAnchorLocation 
                = WindowAnchorService.ConvertBetweenControlAndWindowAnchorLocation(location, Size, ConvertLocation.ToWindowAnchorLocation, _settingService.WindowAnchorSetting.Value);
            
            var newLocation = ConvertCoordinatesService.ConvertAbsoluteToRelativeCoordinates(windowAnchorLocation, GameService.Graphics.SpriteScreen.Size);
            var oldLocation = _settingService.ProfitWindowRelativeWindowAnchorLocationSetting.Value;
            // this if prevents that every click in a window causes a settings update.
            // No idea why this code is even triggered for StandardWindows that dont use this custom Container class.
            var windowLocationChanged = newLocation.X != oldLocation.X || newLocation.Y != oldLocation.Y;
            if (windowLocationChanged)
                _settingService.ProfitWindowRelativeWindowAnchorLocationSetting.Value = newLocation;
        }

        private Point _mousePressedLocationInsideContainer = Point.Zero;
        private bool _containerIsDraggedByMouse;
        private readonly SettingService _settingService;
    }
}