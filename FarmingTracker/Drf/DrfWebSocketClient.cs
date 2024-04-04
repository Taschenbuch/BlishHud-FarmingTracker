using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
        public event EventHandler<DrfWebSocketEventArgs> ConnectFailed;
        public event EventHandler<DrfWebSocketEventArgs> ConnectCrashed;
        public event EventHandler<DrfWebSocketEventArgs> SendAuthenticationFailed;
        public event EventHandler<DrfWebSocketEventArgs> AuthenticationFailed;
        public event EventHandler ReceivedUnexpectedBinaryMessage;
        public event EventHandler<DrfWebSocketEventArgs> UnexpectedNotOpenWhileReceiving;
        public event EventHandler<DrfWebSocketEventArgs> ReceivedMessage;
        public event EventHandler<DrfWebSocketEventArgs> ReceiveCrashed;

        /// <summary> To change websocket server url for debugging </summary>
        public string WebSocketUrl { get; set; } = "wss://drf.rs/ws";

        public bool HasNewDrfMessages()
        {
            lock (_drfMessagesLock)
                return _drfMessages.Count != 0;
        }

        public List<DrfMessage> GetDrfMessages()
        {
            var newEmptyList = new List<DrfMessage>();
            lock (_drfMessagesLock)
            {
                var receivedMessages = _drfMessages;
                _drfMessages = newEmptyList;
                return receivedMessages;
            }
        }               

        public void Dispose()
        {
            _cancelationTokenSource.Cancel(); // do not dispose cancelationTokenSource because then tasks may not cancel correctely anymore
            _clientWebSocket.Abort(); // calls ClientWebSocket.Dispose() internally
        }

        public async Task Connect(string drfApiToken)
        {
            // todo alles auch in Task.Run von receive ausführen? warum sollte nur connect teil nicht auf anderem thread laufen?
            try
            {
                // do not cancel subsequent Connect()s, because they may use a different drfApiToken.
                await _closeSemaphoreSlim.WaitAsync(0).ConfigureAwait(false); 
                CancellationTokenSource cancelationTokenSource;
                ClientWebSocket clientWebSocket;

                try
                {
                    await Close().ConfigureAwait(false);
                    _cancelationTokenSource = new CancellationTokenSource();
                    _clientWebSocket = new ClientWebSocket();
                    // local variables instead of field because of potential reentrancy now or after future refactoring
                    cancelationTokenSource = _cancelationTokenSource; 
                    clientWebSocket = _clientWebSocket;
                }
                finally
                {
                    _closeSemaphoreSlim.Release();
                }

                try
                {
                    await clientWebSocket.ConnectAsync(new Uri(WebSocketUrl), cancelationTokenSource.Token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (Exception e)
                {
                    ConnectFailed?.Invoke(this, new DrfWebSocketEventArgs(e.Message));
                    return;
                }
                
                var authenticationSendBuffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes($"Bearer {drfApiToken}"));

                try
                {
                    // todo kann ich direkt nach connect schon close erhalten haben?
                    await clientWebSocket.SendAsync(authenticationSendBuffer, WebSocketMessageType.Text, true, cancelationTokenSource.Token).ConfigureAwait(false); 
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (Exception e)
                {
                    SendAuthenticationFailed?.Invoke(this, new DrfWebSocketEventArgs(e.Message));
                    return;
                }

                var fireAndForgetTask = Task.Run(() => ReceiveContinuously(clientWebSocket, cancelationTokenSource.Token), cancelationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception e)
            {
                ConnectCrashed?.Invoke(this, new DrfWebSocketEventArgs(e.Message));
                return;
            }
        }

        private async void ReceiveContinuously(ClientWebSocket clientWebSocket, CancellationToken cancellationToken)
        {
            var messageNumber = 1; // todo weg
            WebSocketReceiveResult receiveResult;

            try
            {
                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (clientWebSocket.State != WebSocketState.Open)
                    {
                        if (clientWebSocket.CloseStatusDescription == CLOSED_BY_CLIENT_DESCRIPTION)
                            return;

                        UnexpectedNotOpenWhileReceiving?.Invoke(this, new DrfWebSocketEventArgs(CreateStatusMessage(clientWebSocket)));
                        return;
                    }

                    var offsetInReceiveBuffer = 0;
                    do
                    {
                        var partialReceiveBuffer = new ArraySegment<byte>(_receiveBuffer, offsetInReceiveBuffer, PARTIAL_RECEIVE_BUFFER_SIZE);
                        receiveResult = await clientWebSocket.ReceiveAsync(partialReceiveBuffer, cancellationToken).ConfigureAwait(false);
                        offsetInReceiveBuffer += receiveResult.Count;
                        // about every 100-400th DRF message is split into 2. Though that might be caused by ClientWebSocket and not DRF websocket
                        // message will automatically plit when the partial receiveBuffer is not big enough either.
                    } while (!receiveResult.EndOfMessage);

                    //Console.WriteLine(CreateStatusMessage(clientWebSocket)); // todo weg

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

                            lock (_drfMessagesLock)
                                _drfMessages.Add(drfMessage);

                            ReceivedMessage?.Invoke(this, new DrfWebSocketEventArgs($"{messageNumber} ({offsetInReceiveBuffer} bytes): {receivedJson}\n"));
                            messageNumber++;
                            break;
                        }
                        case WebSocketMessageType.Close:
                        {
                            if (clientWebSocket.CloseStatusDescription == CLOSED_BY_CLIENT_DESCRIPTION)
                                return;

                            if (clientWebSocket.CloseStatusDescription == CLOSED_BY_SERVER_BECAUSE_AUTHENTICATION_FAILED_DESCRIPTION)
                            {
                                AuthenticationFailed?.Invoke(this, new DrfWebSocketEventArgs(CreateStatusMessage(clientWebSocket)));
                                return;
                            }

                            // https://stackoverflow.com/a/76682535
                            // - CloseOutputAsync() does not wait, while CloseAsync() may wait endless for an answer of the server
                            // - Close(Output)Async() has to be called on server AND client side. It is close initialiser and close response.
                            // use CloseOutputAsync() instead of CloseAsync() because of bug in .net <3.0 websocket: https://mcguirev10.com/2019/08/17/how-to-close-websocket-correctly.html
                            await clientWebSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, receiveResult.CloseStatusDescription, cancellationToken).ConfigureAwait(false);
                            UnexpectedNotOpenWhileReceiving?.Invoke(this, new DrfWebSocketEventArgs(CreateStatusMessage(clientWebSocket)));
                            return;
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch(InvalidOperationException e) // e.g. ReceiveAsync(): The ClientWebSocket is not connected.
            {
                ReceiveCrashed?.Invoke(this, new DrfWebSocketEventArgs($"InvalidOperationException: {e.Message}"));
                return;
            }
            catch (WebSocketException e)
            {
                Console.WriteLine($"WebSocketException {e}");
                // probably affected by localisation...
                if (e.Message == "The 'System.Net.WebSockets.InternalClientWebSocket' instance cannot be used for communication because it has been transitioned into the 'Aborted' state.")
                    return;

                ReceiveCrashed?.Invoke(this, new DrfWebSocketEventArgs($"WebSocketException: {e.Message}"));
                return;
            }
            catch (Exception e)
            {
                ReceiveCrashed?.Invoke(this, new DrfWebSocketEventArgs($"Exception: {e.Message}"));
                return;
            }
        }

        // Not public anymore and semaphore was removed because Close() and Connect() can otherwise run into all kind of racing conditions.
        // those were triggered occasionally so they were hard to track down.
        private async Task Close()
        {
            try
            {
                var canBeClosed =
                    _clientWebSocket.State == WebSocketState.Open
                    || _clientWebSocket.State == WebSocketState.CloseReceived
                    || _clientWebSocket.State == WebSocketState.Connecting;

                Console.WriteLine("canBeClosed: " + canBeClosed); // todo weg

                if (canBeClosed) // CloseOutputAsync because see comment for other call of it
                    await _clientWebSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, CLOSED_BY_CLIENT_DESCRIPTION, default).ConfigureAwait(false);

                Dispose();
            }
            catch (Exception)
            {
                Console.WriteLine("Close: " + CreateStatusMessage(_clientWebSocket)); // todo weg
                /* NOOP because CloseOutputAsync has a high chance of throwing an exception*/
            }
        }

        private static string CreateStatusMessage(ClientWebSocket clientWebSocket)
        {
            return $"State.{clientWebSocket.State} CloseStatus.{clientWebSocket.CloseStatus} CloseStatusDescription: {clientWebSocket.CloseStatusDescription}";
        }

        private readonly SemaphoreSlim _closeSemaphoreSlim = new SemaphoreSlim(1);
        private static readonly object _drfMessagesLock = new Object();
        private List<DrfMessage> _drfMessages = new List<DrfMessage>();
        private ClientWebSocket _clientWebSocket = new ClientWebSocket();
        private const char FIRST_LETTER_OF_KIND_SESSION_UPDATE = 's';
        private const string CLOSED_BY_CLIENT_DESCRIPTION = "closed by blish farming tracker module";
        private const string CLOSED_BY_SERVER_BECAUSE_AUTHENTICATION_FAILED_DESCRIPTION = "no valid session provided";
        private const int PARTIAL_RECEIVE_BUFFER_SIZE = 4000;
        private const int RECEIVE_BUFFER_SIZE = 10 * PARTIAL_RECEIVE_BUFFER_SIZE;
        private readonly byte[] _receiveBuffer = new byte[RECEIVE_BUFFER_SIZE];
        private CancellationTokenSource _cancelationTokenSource = new CancellationTokenSource();
    }
}
