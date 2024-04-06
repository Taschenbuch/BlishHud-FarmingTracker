using Blish_HUD;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FarmingTracker
{
    public class Drf : IDisposable
    {
        public Drf(Services services)
        {
            _services = services;
            InitializeEventHandlers();
            services.SettingService.DrfToken.SettingChanged += OnDrfTokenSettingChanged;
            FireAndForgetConnectToDrf();
        }

        public DrfConnectionStatus DrfConnectionStatus { get; private set; } = DrfConnectionStatus.Disconnected;
        public event EventHandler DrfConnectionStatusChanged;

        public void SetDrfConnectionStatus(DrfConnectionStatus status)
        {
            DrfConnectionStatus = status;
            DrfConnectionStatusChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            _services.SettingService.DrfToken.SettingChanged -= OnDrfTokenSettingChanged;
            _drfWebSocketClient.Dispose();
            // todo events unsubscriben nötig!
        }

        public List<DrfMessage> GetDrfMessages() => _drfWebSocketClient.GetDrfMessages();
        public bool HasNewDrfMessages() => _drfWebSocketClient.HasNewDrfMessages();

        // sometimes drf.dll fails to get the wallet snapshot after a map change.
        // This results in a drf message with all currencies currently in the wallet instead of just the difference
        public static List<DrfMessage> RemoveInvalidMessages(List<DrfMessage> drfMessages)
        {
            return drfMessages.Where(m => m.Payload.Drop.Currencies.Count <= 10).ToList();
        }

        private async void OnDrfTokenSettingChanged(object sender = null, ValueChangedEventArgs<string> e = null)
        {
            //_drfWebSocketClient.WebSocketUrl = "ws://localhost:8080"; // todo debug
            var drfToken = _services.SettingService.DrfToken.Value;

            if (DrfToken.HasValidFormat(drfToken)) // prevents that Connect() is spammed while user is typing in the drf token.
                await _drfWebSocketClient.Connect(drfToken);
        }

        // To trigger at least one connect on startup without drf token validation. This prevents that the module starts in "Disconnected" state.
        private async void FireAndForgetConnectToDrf()
        {
            //_drfWebSocketClient.WebSocketUrl = "ws://localhost:8080"; // todo debug
            await _drfWebSocketClient.Connect(_services.SettingService.DrfToken.Value);
        }

        private void InitializeEventHandlers()
        {
            _drfWebSocketClient.Connecting += (s, e) =>
            {
                Module.Logger.Debug("Connecting");
                SetDrfConnectionStatus(DrfConnectionStatus.Connecting);
            };

            _drfWebSocketClient.Connected += (s, e) =>
            {
                Module.Logger.Debug("Connected");
                SetDrfConnectionStatus(DrfConnectionStatus.Connected);
            };

            _drfWebSocketClient.ConnectFailed += (s, e) =>
            {
                Module.Logger.Warn($"ConnectFailed: {e.Data}");
                SetDrfConnectionStatus(DrfConnectionStatus.Disconnected);
            };

            _drfWebSocketClient.SendAuthenticationFailed += (s, e) =>
            {
                Module.Logger.Warn($"SendAuthenticationFailed: {e.Data}");
                SetDrfConnectionStatus(DrfConnectionStatus.Disconnected);
            };

            _drfWebSocketClient.AuthenticationFailed += (s, e) =>
            {
                Module.Logger.Warn("AuthenticationFailed");
                SetDrfConnectionStatus(DrfConnectionStatus.AuthenticationFailed);
            };

            _drfWebSocketClient.UnexpectedNotOpenWhileReceiving += (s, e) =>
            {
                // todo reconnect nötig, aber keine info an user oder? 
                // todo wird das durch einen reconnect ggf. getriggert? wäre blöd wenn reconnect selbst reconnect triggert.
                Module.Logger.Warn("ReceivedUnexpectedNotOpen");
            };

            _drfWebSocketClient.ReceivedUnexpectedBinaryMessage += (s, e) =>
            {
                // no need to inform user because message will be ignored
                Module.Logger.Warn("ReceivedUnexpectedBinaryMessage");
            };

            _drfWebSocketClient.ConnectCrashed += (s, e) =>
            {
                Module.Logger.Error($"ConnectCrashed: {e.Data}"); // todo test ob wirklich full exception strack trace lockt. wichtig wenn released
                SetDrfConnectionStatus(DrfConnectionStatus.ModuleError);
            };

            _drfWebSocketClient.ReceiveCrashed += (s, e) =>
            {
                Module.Logger.Error($"ReceiveCrashed: {e.Data}");
                SetDrfConnectionStatus(DrfConnectionStatus.ModuleError);
            };
        }

        private readonly Services _services;
        private readonly DrfWebSocketClient _drfWebSocketClient = new DrfWebSocketClient();
    }
}
