using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

// Lit:
// - microsoft online documentation is almost none existant
// - best to read source of class System.Net.Websockets.ManagedWebSocket found in System.Net.WebSockets.sln.
// That class seems to be used by System.Net.WebSockets.ClientWebSocket internally
// - third party tools that can be used for reference: // todo update comments here
// ClientWebSocket sends pong responses, when pinged. Does not send ping https://github.com/dotnet/runtime/issues/48729
// windows tcp keep-alive timeout für idle tcp verbindungen: ca 2h
namespace FarmingTracker
{
    public class DrfWebSocketClient: IDisposable
    {
        public DrfWebSocketClient()
        {
            try
            {
                _clientWebSocket = new ClientWebSocket();
            }
            catch (PlatformNotSupportedException e)
            {
                WindowsVersionIsTooLowToSupportWebSockets = true;
                
                Module.Logger.Warn(
                    $"Failed to initialize the DRF WebSocket client. This is typically caused by not using at least Windows 8. " +
                    $"WebSockets are not supported in older Windows versions. The module will not work. " +
                    $"PlatformNotSupportedException message: {e.Message}");
            }
        }

        /// <summary> To change websocket server url for debugging </summary>
        public string WebSocketUrl { get; set; } = "wss://drf.rs/ws";
        public bool WindowsVersionIsTooLowToSupportWebSockets { get; set; }
        public event EventHandler? Connecting;
        public event EventHandler? ConnectedAndAuthenticationRequestSent;
        /// <summary> 
        /// e.g. "Unable to connect to the remote server" when websocket server is not running
        /// </summary>
        public event EventHandler<GenericEventArgs<Exception>>? ConnectFailed;
        public event EventHandler<GenericEventArgs<Exception>>? ConnectCrashed;
        public event EventHandler<GenericEventArgs<Exception>>? SendAuthenticationFailed;
        public event EventHandler<GenericEventArgs<string>>? AuthenticationFailed;
        public event EventHandler<GenericEventArgs<string>>? UnexpectedNotOpenWhileReceiving;
        public event EventHandler<GenericEventArgs<string>>? ReceivedMessage;
        public event EventHandler? ReceivedUnexpectedBinaryMessage;
        public event EventHandler<GenericEventArgs<Exception>>? ReceiveCrashed;
        public event EventHandler<GenericEventArgs<Exception>>? ReceiveFailed;

        public List<DrfMessage> GetDrfMessages()
        {
            var newEmptyList = new List<DrfMessage>();
            List<DrfMessage> receivedMessages;

            lock (_drfMessagesLock)
            {
                receivedMessages = _drfMessages;
                _drfMessages = newEmptyList;
            }

            return RemoveInvalidMessages(receivedMessages);
        }

        // clientWebSocket.Abort() is handled by Connect() and Receive(). So it should not be handled here
        // _disposed bool should not be necessary, but it happened multiple times
        // that after module.unload() and this Dispose() this class still received messages or didnt stop doing reconnect tries
        public void Dispose()
        {
            if(WindowsVersionIsTooLowToSupportWebSockets) 
                return;

            lock (_disposeLock)
            {
                // remove eventHandlers to reduce risk of triggering unwanted reconnects. Should not be necessary but better safe than sorry.
                ConnectFailed = null;
                ConnectCrashed = null;
                SendAuthenticationFailed = null;
                AuthenticationFailed = null;
                UnexpectedNotOpenWhileReceiving = null;
                ReceivedMessage = null;
                ReceivedUnexpectedBinaryMessage = null;
                ReceiveCrashed = null;
                ReceiveFailed = null;

                _disposed = true;
                _disposeCts.Cancel(); // do not dispose cancelationTokenSource because then tasks may not cancel correctely anymore
            }
        }

        // todo alles auch in Task.Run von receive ausführen? warum sollte nur connect teil nicht auf anderem thread laufen?
        // NIEMALS per Task.Run starten! Dadurch kann es passieren, dass Connect(old) nach Connect(new) ausgeführt wird, wenn
        // viele Connects() schnell hintereinander kommen bzw. Close() sehr lange dauert.
        // kann ich gar nicht verhindern... weil ich ja in events, die auf beliebigen threads laufen, einen reconnect triggere...
        // kann ich irgendwie auf bestimmten thread dafür wechseln? z.b. Ui thread? Wobei selbst das bringt nichts...
        // wahrscheinlich einzige lösung: user token eingabe überarbeiten:
        // User clickt "Add key" -> warten bis aktive verbindung getrennt -> user token kann geändert werden
        // -> user clicked "use" -> Verbindung wird gestartet
        public async Task Connect(string drfToken)
        {
            if (WindowsVersionIsTooLowToSupportWebSockets)
                return;

            // only allow latest Connect() call to wait at semaphore. Latest has the most up to date drfToken. No need to run older Connect()s
            var letOnlyLatestWaitCts = new CancellationTokenSource();

            lock (_letOnlyLatestWaitLock)
            {
                _letOnlyLatestWaitCts.Cancel();
                _letOnlyLatestWaitCts = letOnlyLatestWaitCts;
            }            

            ClientWebSocket? clientWebSocket = null;

            try
            {
                // do not cancel subsequent Connect()s, because they will use a newer drfToken.
                try
                {
                    await _closeSemaphoreSlim.WaitAsync(letOnlyLatestWaitCts.Token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    return;
                }                

                CancellationTokenSource disposeCts;
                try
                {
                    await Close().ConfigureAwait(false);
                    disposeCts = new CancellationTokenSource();

                    lock (_disposeLock)
                    {
                        if (_disposed)
                            return;

                        _disposeCts.Cancel(); // do not dispose cancelationTokenSource because then tasks may not cancel correctely anymore
                        _disposeCts = disposeCts;
                    }

                    clientWebSocket = new ClientWebSocket();
                    _clientWebSocket = clientWebSocket;
                }
                finally
                {
                    _closeSemaphoreSlim.Release();
                }

                Connecting?.Invoke(this, EventArgs.Empty);

                try
                {
                    await clientWebSocket.ConnectAsync(new Uri(WebSocketUrl), disposeCts.Token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    // this may never get catched. Instead a WebSocketException will be thrown.
                    throw;
                }
                catch (Exception e)
                {
                    // no internet (comes immediately):
                    // WebSocketException: Unable to connect to the remote server
                    // -> WebException: The remote name could not be resolved: 'drf.rs'
                    //
                    // internet, but webSocket server is not running (comes immediately):
                    // WebSocketException: Unable to connect to the remote server
                    // -> WebException: Unable to connect to the remote server
                    // -> SocketException: No connection could be made because the target machine actively refused it 127.0.0.1:8080
                    //
                    // instead of OperationCanceledException when cts.Cancel(). Ist bug: https://github.com/dotnet/runtime/issues/29763
                    // WebSocketException: Unable to connect to the remote server
                    // -> WebException: The request was aborted: The request was canceled.

                    clientWebSocket.Abort(); // calls .Dispose() internally
                    if (disposeCts.IsCancellationRequested) // because ClientWebSocket does not throw OperationCanceledException
                        return;

                    ConnectFailed?.Invoke(this, new GenericEventArgs<Exception>(e));
                    return;
                }

                var authenticationSendBuffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes($"Bearer {drfToken}"));

                try
                {
                    // todo kann ich direkt nach connect schon close erhalten haben?
                    await clientWebSocket.SendAsync(authenticationSendBuffer, WebSocketMessageType.Text, true, disposeCts.Token).ConfigureAwait(false); 
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    clientWebSocket.Abort(); // calls .Dispose() internally
                    if (disposeCts.IsCancellationRequested) // because ClientWebSocket does not throw OperationCanceledException
                        return;

                    SendAuthenticationFailed?.Invoke(this, new GenericEventArgs<Exception>(e));
                    return;
                }

                // must not await, otherwise Connect() never returns because of the infinite receive loop in the happy path
                ConnectedAndAuthenticationRequestSent?.Invoke(this, EventArgs.Empty);
                _ = Task.Run(() => ReceiveContinuously(clientWebSocket, disposeCts.Token), disposeCts.Token);
            }
            catch (OperationCanceledException)
            {
                clientWebSocket?.Abort(); // calls .Dispose() internally
            }
            catch (Exception e)
            {
                clientWebSocket?.Abort(); // calls .Dispose() internally
                ConnectCrashed?.Invoke(this, new GenericEventArgs<Exception>(e));
            }
        }

        private async void ReceiveContinuously(ClientWebSocket clientWebSocket, CancellationToken ctsToken)
        {
            WebSocketReceiveResult receiveResult;

            try
            {
                while (!_disposed)
                {
                    ctsToken.ThrowIfCancellationRequested();

                    if (clientWebSocket.State != WebSocketState.Open)
                    {
                        if (clientWebSocket.CloseStatusDescription == CLOSED_BY_CLIENT_DESCRIPTION)
                            return;

                        UnexpectedNotOpenWhileReceiving?.Invoke(this, new GenericEventArgs<string>($"receive loop start {CreateStatusMessage(clientWebSocket)}"));
                        return;
                    }

                    var offsetInReceiveBuffer = 0;
                    do
                    {
                        var partialReceiveBuffer = new ArraySegment<byte>(_receiveBuffer, offsetInReceiveBuffer, PARTIAL_RECEIVE_BUFFER_SIZE);
                        receiveResult = await clientWebSocket.ReceiveAsync(partialReceiveBuffer, ctsToken).ConfigureAwait(false);
                        offsetInReceiveBuffer += receiveResult.Count;
                        // about every 100-400th DRF message is split into 2. Though that might be caused by ClientWebSocket and not DRF websocket
                        // message will automatically plit when the partial receiveBuffer is not big enough either.
                    } while (!receiveResult.EndOfMessage);

                    switch (receiveResult.MessageType)
                    {
                        case WebSocketMessageType.Binary:
                        {
                            ReceivedUnexpectedBinaryMessage?.Invoke(this, default);
                            break; // ignore binary data. no need to close connection.
                        }
                        case WebSocketMessageType.Text:
                        {
                            // e.g. map entered {"kind": "session_update","payload":{"character": "Impfzertifikat","level": 0,"map": 28,"start": "2024-03-21T20:34:39.032650329Z","end": null}}
                            // e.g. map left    {"kind": "session_update","payload":{"character": "Impfzertifikat","level": 0,"map": 28,"start": "2024-03-21T20:34:39.032650329Z","end": "2024-03-21T20:35:48.626118887Z"}}
                            // e.g. drop        {"kind": "data",          "payload":{"character": "Impfzertifikat","drop":{"items":{"70047":-1},"curr":{"23":3},"mf":249,"timestamp":"2024-03-18T22:43:02.524471732Z"}}}
                            var receivedJson = Encoding.UTF8.GetString(_receiveBuffer, 0, offsetInReceiveBuffer);

                            var mapOrCharacterSelectHasBeenEnteredOrLeft = receivedJson[9] == FIRST_LETTER_OF_KIND_SESSION_UPDATE; // {"kind":"s <- index 9
                            if (mapOrCharacterSelectHasBeenEnteredOrLeft)
                                continue;

                            var drfMessage = JsonConvert.DeserializeObject<DrfMessage>(receivedJson);

                            if (drfMessage == null)
                            {
                                Module.Logger.Error("Failed to create drfMessage from json.");
                                break;
                            }

                            lock (_drfMessagesLock)
                                _drfMessages.Add(drfMessage);

                            ReceivedMessage?.Invoke(this, new GenericEventArgs<string>($"({offsetInReceiveBuffer} bytes): {receivedJson}\n"));
                            break;
                        }
                        case WebSocketMessageType.Close:
                        {
                            if (clientWebSocket.CloseStatusDescription == CLOSED_BY_CLIENT_DESCRIPTION)
                                return;

                            if (clientWebSocket.CloseStatusDescription == CLOSED_BY_SERVER_BECAUSE_AUTHENTICATION_FAILED_DESCRIPTION)
                            {
                                AuthenticationFailed?.Invoke(this, new GenericEventArgs<string>(CreateStatusMessage(clientWebSocket)));
                                return;
                            }

                            // https://stackoverflow.com/a/76682535
                            // - CloseOutputAsync() does not wait, while CloseAsync() may wait infinite for an answer of the server
                            // - Close(Output)Async() has to be called on server AND client side. It is close initialiser and close response.
                            // use CloseOutputAsync() instead of CloseAsync() because of bug in .net <3.0 websocket: https://mcguirev10.com/2019/08/17/how-to-close-websocket-correctly.html
                            await clientWebSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, receiveResult.CloseStatusDescription, ctsToken).ConfigureAwait(false);
                            UnexpectedNotOpenWhileReceiving?.Invoke(this, new GenericEventArgs<string>($"close message {CreateStatusMessage(clientWebSocket)}"));
                            return;
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // NOOP
            }
            catch (WebSocketException e)
            {
                // no internet (comes after 60s):
                // WebSocketException: An internal WebSocket error occurred. Please see the innerException, if present, for more details.
                // -> IOException: Unable to read data from the transport connection: A connection attempt failed because the connected party did not properly respond after a period of time, or established connection failed because connected host has failed to respond.
                // -> SocketException: A connection attempt failed because the connected party did not properly respond after a period of time, or established connection failed because connected host has failed to respond
                //
                // internet, but webSocket server stopped running while receiving messages (comes immediately):
                // WebSocketException: An internal WebSocket error occurred. Please see the innerException, if present, for more details.
                // -> SocketException: An existing connection was forcibly closed by the remote host

                if (ctsToken.IsCancellationRequested) // because ClientWebSocket does not throw OperationCanceledException
                    return;

                ReceiveFailed?.Invoke(this, new GenericEventArgs<Exception>(e));
            }
            catch (Exception e)
            {
                if (ctsToken.IsCancellationRequested) // because ClientWebSocket does not throw OperationCanceledException
                    return;

                // InvalidOperationException: ReceiveAsync(): The ClientWebSocket is not connected
                ReceiveCrashed?.Invoke(this, new GenericEventArgs<Exception>(e));
            }
            finally
            {
                clientWebSocket.Abort(); // calls .Dispose() internally
            }
        }

        // Not public anymore and semaphore was removed because Close() and Connect() can otherwise run into all kind of racing conditions.
        // those were triggered occasionally so they were hard to track down.
        private async Task Close()
        {
            try
            {
                if(_clientWebSocket == null)
                {
                    Module.Logger.Error("Close() expects clientWebSocket to be set.");
                    return;
                }

                var canBeClosed =
                    _clientWebSocket.State == WebSocketState.Open
                    || _clientWebSocket.State == WebSocketState.CloseReceived
                    || _clientWebSocket.State == WebSocketState.Connecting;

                if (canBeClosed) // CloseOutputAsync because see comment for other call of it
                    await _clientWebSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, CLOSED_BY_CLIENT_DESCRIPTION, default).ConfigureAwait(false);

                // do not call _clientWebSocket.Abort() here. The method using "new clientWebSocket()" has to handle the Abort() = Dispose() itself.
            }
            catch (Exception)
            {
                // NOOP because CloseOutputAsync has a high chance of throwing an exception
            }
        }

        private static string CreateStatusMessage(ClientWebSocket clientWebSocket)
        {
            return $"State.{clientWebSocket.State} CloseStatus.{clientWebSocket.CloseStatus} CloseStatusDescription: {clientWebSocket.CloseStatusDescription}";
        }

        // sometimes drf.dll fails to get the wallet snapshot after a map change.
        // This results in a drf message with all currencies currently in the wallet instead of just the difference
        private static List<DrfMessage> RemoveInvalidMessages(List<DrfMessage> drfMessages)
        {
            return drfMessages.Where(m => m.Payload.Drop.Currencies.Count <= MAX_CURRENCIES_IN_A_SINGLE_DROP).ToList();
        }

        private readonly SemaphoreSlim _closeSemaphoreSlim = new SemaphoreSlim(1);
        private static readonly object _drfMessagesLock = new object();
        private static readonly object _letOnlyLatestWaitLock = new object();
        private static readonly object _disposeLock = new object();
        private List<DrfMessage> _drfMessages = new List<DrfMessage>();
        private ClientWebSocket? _clientWebSocket; // first time set by ctor to check if at least windows 8 
        private const char FIRST_LETTER_OF_KIND_SESSION_UPDATE = 's';
        private const string CLOSED_BY_CLIENT_DESCRIPTION = "closed by blish farming tracker module";
        private const string CLOSED_BY_SERVER_BECAUSE_AUTHENTICATION_FAILED_DESCRIPTION = "no valid session provided";
        private const int PARTIAL_RECEIVE_BUFFER_SIZE = 4000;
        private const int RECEIVE_BUFFER_SIZE = 10 * PARTIAL_RECEIVE_BUFFER_SIZE;
        public const int MAX_CURRENCIES_IN_A_SINGLE_DROP = 10;
        private readonly byte[] _receiveBuffer = new byte[RECEIVE_BUFFER_SIZE];
        private CancellationTokenSource _disposeCts = new CancellationTokenSource();
        private CancellationTokenSource _letOnlyLatestWaitCts = new CancellationTokenSource();
        private bool _disposed;
    }
}
