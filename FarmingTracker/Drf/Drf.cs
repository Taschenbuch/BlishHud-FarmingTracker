using Blish_HUD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FarmingTracker
{
    public class Drf : IDisposable
    {
        public Drf(SettingService settingsService)
        {
            _settingService = settingsService;
            settingsService.DrfToken.SettingChanged += OnDrfTokenSettingChanged;
            InitializeEventHandlers();
            FireAndForgetConnectToDrf();
        }

        public DrfConnectionStatus DrfConnectionStatus { get; private set; } = DrfConnectionStatus.Disconnected;
        public int ReconnectDelaySeconds { get; private set; }
        public int ReconnectTriesCounter => _reconnectTriesCounter;

        public event EventHandler DrfConnectionStatusChanged;

        public void SetDrfConnectionStatus(DrfConnectionStatus status)
        {
            DrfConnectionStatus = status;
            DrfConnectionStatusChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            _settingService.DrfToken.SettingChanged -= OnDrfTokenSettingChanged;
            _drfWebSocketClient.Dispose();
            // todo events unsubscriben nötig!
        }

        public List<DrfMessage> GetDrfMessages() => _drfWebSocketClient.GetDrfMessages();

        private void InitializeEventHandlers()
        {
            _drfWebSocketClient.Connecting += (s, e) =>
            {
                Module.Logger.Debug("Connecting");
                SetDrfConnectionStatus(DrfConnectionStatus.Connecting);
            };

            _drfWebSocketClient.ConnectFailed += async (s, e) =>
            {
                Module.Logger.Warn($"ConnectFailed: {ExceptionService.GetExceptionSummary(e.Data)}");
                await Reconnect();
            };

            _drfWebSocketClient.SendAuthenticationFailed += async (s, e) =>
            {
                Module.Logger.Warn($"SendAuthenticationFailed: {ExceptionService.GetExceptionSummary(e.Data)}");
                await Reconnect();
            };

            _drfWebSocketClient.ConnectCrashed += (s, e) =>
            {
                Module.Logger.Error($"ConnectCrashed: {e.Data}");
                SetDrfConnectionStatus(DrfConnectionStatus.ModuleError);
            };

            _drfWebSocketClient.ConnectedAndAuthenticationRequestSent += (s, e) =>
            {
                Module.Logger.Debug("ConnectedAndAuthenticationRequestSent");
                _reconnectTriesCounter = 0;
                SetDrfConnectionStatus(DrfConnectionStatus.Connected);
            };

            _drfWebSocketClient.UnexpectedNotOpenWhileReceiving += async (s, e) =>
            {
                Module.Logger.Warn("ReceivedUnexpectedNotOpen");
                await Reconnect();
            };

            _drfWebSocketClient.ReceiveFailed += async (s, e) =>
            {
                Module.Logger.Warn($"ReceiveFailed: {ExceptionService.GetExceptionSummary(e.Data)}");
                await Reconnect();
            };

            _drfWebSocketClient.AuthenticationFailed += (s, e) =>
            {
                Module.Logger.Warn("AuthenticationFailed");
                SetDrfConnectionStatus(DrfConnectionStatus.AuthenticationFailed);
            };

            _drfWebSocketClient.ReceivedUnexpectedBinaryMessage += (s, e) =>
            {
                Module.Logger.Warn("ReceivedUnexpectedBinaryMessage");
                // no need to inform user because binary messages are ignored. But it is still interesting to log those.
            };

            _drfWebSocketClient.ReceiveCrashed += (s, e) =>
            {
                Module.Logger.Error($"ReceiveCrashed: {e.Data}");
                SetDrfConnectionStatus(DrfConnectionStatus.ModuleError);
            };
        }

        private async Task Reconnect()
        {
            lock (_reconnectLock)
            {
                if (DrfConnectionStatus == DrfConnectionStatus.TryReconnect) // todo überflüssig? 
                {
                    Module.Logger.Debug("Do not trigger a new reconnect because a reconnect is already in progress.");
                    return;
                }

                // dont use SetDrfConnectionStatus() because eventHandler execution would increase time inside lock
                DrfConnectionStatus = DrfConnectionStatus.TryReconnect; 
            }

            var reconnectTriesCounter = Interlocked.Increment(ref _reconnectTriesCounter);
            var reconnectDelayMs = DetermineReconnectDelayMs(reconnectTriesCounter);
            var reconnectDelaySeconds = reconnectDelayMs / 1000;
            ReconnectDelaySeconds = reconnectDelaySeconds;
            DrfConnectionStatusChanged?.Invoke(this, EventArgs.Empty);

            await Task.Delay(reconnectDelayMs); // todo reconnect canceln nötig? oder reicht der if danach?
            
            if (DrfConnectionStatus != DrfConnectionStatus.TryReconnect)
            {
                Module.Logger.Debug($"Cancel reconnect because {nameof(DrfConnectionStatus)} changed during reconnect delay.");
                return;
            }

            if (reconnectTriesCounter <= 6 || reconnectTriesCounter % 20 == 0) // todo wieder aktivieren
                Module.Logger.Warn(
                    $"{reconnectTriesCounter}. try to reconnect to DRF after {reconnectDelaySeconds}s delay. " +
                    $"This message will only appear for the first 6 tries and every 20 tries.");

            // To trigger at least one connect on startup without drf token validation. This prevents that the module starts in "Disconnected" state.
            FireAndForgetConnectToDrf();
        }

        // Delay must not be too long because otherwise it would take too much time
        // after an internet connection interruption to connect to drf again.
        private static int DetermineReconnectDelayMs(int reconnectTriesCounter)
        {
            return reconnectTriesCounter switch
            {
                1 => 0, // first reconnect try should is instant
                2 => 2_000,
                3 => 5_000,
                4 => 10_000,
                5 => 20_000,
                _ => 30_000 
            };
        }

        private async void OnDrfTokenSettingChanged(object sender = null, ValueChangedEventArgs<string> e = null)
        {
            //_drfWebSocketClient.WebSocketUrl = "ws://localhost:8080"; // todo debug
            var drfToken = _settingService.DrfToken.Value;

            if (DrfToken.HasValidFormat(drfToken)) // prevents that Connect() is spammed while user is typing in the drf token.
                await _drfWebSocketClient.Connect(drfToken);
        }

        private async void FireAndForgetConnectToDrf()
        {
            //_drfWebSocketClient.WebSocketUrl = "ws://localhost:8080"; // todo debug
            await _drfWebSocketClient.Connect(_settingService.DrfToken.Value);
        }

        private readonly SettingService _settingService;
        private readonly DrfWebSocketClient _drfWebSocketClient = new DrfWebSocketClient();
        private int _reconnectTriesCounter;
        private static readonly object _reconnectLock = new object();
    }
}
