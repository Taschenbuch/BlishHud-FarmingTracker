using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

// ClientWebSocket sends pong responses, when pinged. Does not send ping https://github.com/dotnet/runtime/issues/48729
// windows tcp keep-alive timeout für idle tcp verbindungen: ca 2h
namespace FarmingTracker
{
    public class DrfWebSocketClient: IDisposable
    {
        public event EventHandler ConnectFailed;
        public event EventHandler ConnectCrashed;
        public event EventHandler AuthenticationFailed;
        public event EventHandler ReceivedUnexpectedBinaryMessage;
        public event EventHandler ReceivedUnexpectedNotOpen;
        public event EventHandler ReceivedCrashed;

        public bool HasNewDrfMessages()
        {
            lock (_messagesLock)
                return _drfMessages.Count != 0;
        }

        public List<DrfMessage> GetDrfMessages()
        {
            var newEmptyList = new List<DrfMessage>();
            lock (_messagesLock)
            {
                var receivedMessages = _drfMessages;
                _drfMessages = newEmptyList;
                return receivedMessages;
            }
        }

        // muss der websocket überhaupt disposed/Abort werden? einfach am leben lassen ginge doch auch, oder? oder hängt er dann irgendwie fest in nem komischen WebSocketState?
        public async Task Close()
        {
            // semaphore to prevent that Connect() will continue while Close() is running, when Connect() and Close() are called in parallel from different threads
            await _closeSemaphoreSlim.WaitAsync(0).ConfigureAwait(false); 

            if (_isClosed)
                return;

            _isClosed = true;

            try
            {
                if (_clientWebSocket != null 
                    && (_clientWebSocket.State == WebSocketState.Open 
                    || _clientWebSocket.State == WebSocketState.CloseReceived 
                    || _clientWebSocket.State == WebSocketState.Connecting))
                {
                    // CloseOutputAsync instead of CloseAsync, because the latter has issues.
                    await _clientWebSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, string.Empty, default).ConfigureAwait(false);
                }

                Dispose();
            }
            catch (Exception) 
            { 
                /* NOOP because CloseOutputAsync has a high chance of throwing an exception*/ 
            }
            finally
            {
                _closeSemaphoreSlim.Release();
            }
        }

        public void Dispose()
        {
            _cancelationTokenSource.Cancel();
            _clientWebSocket?.Abort(); // calls ClientWebSocket.Dispose() internally
            _clientWebSocket = null;
        }

        public async Task Connect(string drfApiToken, string webSocketUrl)
        {
            try
            {
                await Close().ConfigureAwait(false);
                _cancelationTokenSource = new CancellationTokenSource();
                var cancelationTokenSource = _cancelationTokenSource; // use local because of reentrancy. Should not happen, but may when i refactor something in the future
                _clientWebSocket = new ClientWebSocket();
                _isClosed = false;

                try
                {
                    await _clientWebSocket.ConnectAsync(new Uri(webSocketUrl), cancelationTokenSource.Token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (Exception e)
                {
                    ConnectFailed?.Invoke(this, default); // todo exception message oder ganzen stacktrace als string mitgeben (e.ToString()) oder wie das hieß.
                    return;
                }
                
                var authenticationSendBuffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes($"Bearer {drfApiToken}"));

                try
                {
                    await _clientWebSocket.SendAsync(authenticationSendBuffer, WebSocketMessageType.Text, true, cancelationTokenSource.Token).ConfigureAwait(false); 
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (Exception)
                {
                    // todo halb falsch. muss ZUSÄTZLICH auf Open prüfen. muss ich ggf. sogar 1x "receive" callen um close message zu lesen? quatsch oder?
                    AuthenticationFailed?.Invoke(this, default); 
                    return;
                }

                await Task.Run(() => Receive(cancelationTokenSource.Token), cancelationTokenSource.Token); // still fire and forget because of "async void Receive".
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception)
            {
                ConnectCrashed?.Invoke(this, default);
                return;
            }
        }

        private async void Receive(CancellationToken cancellationToken)
        {
            var messageNumber = 1; // todo weg
            var receiveBuffer = new byte[RECEIVE_BUFFER_SIZE];
            WebSocketReceiveResult receiveResult;

            try
            {
                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (_clientWebSocket.State != WebSocketState.Open)
                    {
                        ReceivedUnexpectedNotOpen?.Invoke(this, default);
                        Console.WriteLine("clientWebSocket.State " + _clientWebSocket.State.ToString() + " " + _clientWebSocket.CloseStatus + " " + _clientWebSocket.CloseStatusDescription);
                        return;
                    }

                    var offsetInReceiveBuffer = 0;
                    do
                    {
                        var partialReceiveBuffer = new ArraySegment<byte>(receiveBuffer, offsetInReceiveBuffer, PARTIAL_RECEIVE_BUFFER_SIZE);
                        receiveResult = await _clientWebSocket.ReceiveAsync(partialReceiveBuffer, cancellationToken).ConfigureAwait(false);
                        offsetInReceiveBuffer += receiveResult.Count;
                        // about every 100-400th DRF message is split into 2. Though that might be caused by ClientWebSocket and not DRF websocket
                        // message will automatically plit when the partial receiveBuffer is not big enough either.
                    } while (!receiveResult.EndOfMessage);

                    if (receiveResult.MessageType == WebSocketMessageType.Close)
                    {
                        // CloseOutputAsync wartet nicht, CloseAsync wartet unendlich lange
                        // use CloseOutputAsync() instead of CloseAsync() because of bug in .net <3.0 websocket: https://mcguirev10.com/2019/08/17/how-to-close-websocket-correctly.html
                        await _clientWebSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, string.Empty, cancellationToken).ConfigureAwait(false);
                    }
                    else if (receiveResult.MessageType == WebSocketMessageType.Text)
                    {
                        var receivedJson = Encoding.UTF8.GetString(receiveBuffer, 0, offsetInReceiveBuffer);

                        var mapOrCharacterSelectHasBeenEnteredOrLeft = receivedJson[9] == 's';
                        if (mapOrCharacterSelectHasBeenEnteredOrLeft)
                            continue;

                        var drfMessage = JsonConvert.DeserializeObject<DrfMessage>(receivedJson);

                        lock (_messagesLock)
                            _drfMessages.Add(drfMessage);
                        
                        Console.WriteLine($"{messageNumber} ({offsetInReceiveBuffer} bytes): {receivedJson}\n");
                        messageNumber++;
                    }
                    else // WebSocketMessageType.Binary
                    {
                        ReceivedUnexpectedBinaryMessage?.Invoke(this, default);
                        Console.WriteLine("unexpected MessageType: " + receiveResult.MessageType.ToString());
                    }
                }
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (WebSocketException e)
            {
                // probably affected by localisation...
                if (e.Message == "The 'System.Net.WebSockets.InternalClientWebSocket' instance cannot be used for communication because it has been transitioned into the 'Aborted' state.")
                    return;

                ReceivedCrashed?.Invoke(this, default);
                return;
            }
            catch (Exception)
            {
                ReceivedCrashed?.Invoke(this, default);
                return;
            }
        }

        private readonly SemaphoreSlim _closeSemaphoreSlim = new SemaphoreSlim(1);
        private List<DrfMessage> _drfMessages = new List<DrfMessage>();
        private static readonly object _messagesLock = new Object();
        private ClientWebSocket _clientWebSocket;
        private bool _isClosed = true;
        private const int PARTIAL_RECEIVE_BUFFER_SIZE = 4000;
        private const int RECEIVE_BUFFER_SIZE = 10 * PARTIAL_RECEIVE_BUFFER_SIZE;
        private CancellationTokenSource _cancelationTokenSource; // may require disposing
    }
}
