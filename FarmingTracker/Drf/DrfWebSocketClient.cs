using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FarmingTracker
{
    public class DrfWebSocketClient
    {
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

        // do not use Dispose pattern here, because async/await is required. https://stackoverflow.com/a/56716998
        // and object will be reused which should not be the case when using .Dispose() (i think?)
        public async Task Close()
        {
            if (_isClosing)
                return;
            
            _isClosing = true;
            if(_clientWebSocket != null && (_clientWebSocket.State == WebSocketState.Open || _clientWebSocket.State == WebSocketState.CloseReceived || _clientWebSocket.State == WebSocketState.Connecting))
            {
                try
                {
                    // todo überflüssig? einfach nicht machen?
                    await _clientWebSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, string.Empty, default).ConfigureAwait(false);
                }
                catch (Exception) { /* NOOP */ }
            }

            _cancelationTokenSource.Cancel();
            _clientWebSocket?.Abort(); // includes .Dispose()
            _clientWebSocket = null;
            // todo cancelation token triggern (muss man exception davon awaiten? ja oder? läuft auf anderem thread?
            // todo oder egal, weil anderer thread es selbst handeln muss? wahrscheinlich einfacher
        }

        public async Task Connect(string drfApiToken)
        {
            try
            {
                //await Close(); // todo _isClosing reicht nicht, muss komplett geclosed sein bevor Connect() wieder starten kann. doofes await..

                _cancelationTokenSource = new CancellationTokenSource();
                // ClientWebSocket sends pong responses, when pinged. Does not send ping https://github.com/dotnet/runtime/issues/48729
                // windows tcp keep-alive timeout für idle tcp verbindungen: ca 2h
                var uri = new Uri("wss://drf.rs/ws");
                var authenticationBuffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes($"Bearer {drfApiToken}"));
                _clientWebSocket = new ClientWebSocket();

                try
                {
                    await _clientWebSocket.ConnectAsync(uri, _cancelationTokenSource.Token).ConfigureAwait(false); // todo try catch?
                }
                catch (Exception)
                {
                    // todo ConnectFailed event? retry wäre hier möglich. overkill?
                    // todo lieber in caller class handeln? weil dann kann ich bei bedarf neuen key übergeben
                    Console.WriteLine("ConnectFailed");
                    return;
                }

                try
                {
                    await _clientWebSocket.SendAsync(authenticationBuffer, WebSocketMessageType.Text, true, _cancelationTokenSource.Token).ConfigureAwait(false); // todo try catch?
                }
                catch (Exception)
                {
                    // todo AuthenticationFailed event?
                    Console.WriteLine("AuthenticationFailed");
                    return;
                }

                await Task.Run(() => Receive(_cancelationTokenSource.Token), _cancelationTokenSource.Token); // still fire and forget because of "async void Receive".
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }

        private async void Receive(CancellationToken cancellationToken)
        {
            // todo allgeminer try catch notwendig
            var messageNumber = 1;

            var receiveBuffer = new byte[RECEIVE_BUFFER_SIZE];
            WebSocketReceiveResult receiveResult;

            //while (true)
            //while (messageNumber <= 300)
            // todo while (!cancellationToken.IsCancellationRequested)? siehe socket.io. ich glaub das ist code smell? token wird ja schon in calls gehandled
            try
            {
                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (_clientWebSocket.State != WebSocketState.Open)
                    {
                        // todo event nötig?
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

                        var mapOrCharacterSelectHasBeenEnteredOrLeft = receivedJson[9] == 's'; // {"kind":"session_update" .... // todo test ob klappt
                        if (mapOrCharacterSelectHasBeenEnteredOrLeft)
                            continue;

                        var drfMessage = JsonConvert.DeserializeObject<DrfMessage>(receivedJson);

                        lock (_messagesLock)
                            _drfMessages.Add(drfMessage);
                        
                        Console.WriteLine($"{messageNumber} ({offsetInReceiveBuffer} bytes): {receivedJson}");
                        Console.WriteLine();
                        messageNumber++;
                    }
                    else // WebSocketMessageType.Binary
                    {
                        Console.WriteLine("unexpected MessageType: " + receiveResult.MessageType.ToString()); // todo alle Console.WriteLine durch logger tauschen
                    }
                }
            }
            catch (WebSocketException e)
            {
                // todo sprach abhängig leider. 
                if (e.Message == "The 'System.Net.WebSockets.InternalClientWebSocket' instance cannot be used for communication because it has been transitioned into the 'Aborted' state.")
                    return;
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("OperationCanceledException");
                // todo kein event nötig oder? weil cancel ist ja bewusst? oder nicht?
                return;
            }
        }

        //await _sendLock.WaitAsync(CancellationToken.None).ConfigureAwait(false); // todo sinnvoll hier? wahrscheinlich nicht
        //readonly SemaphoreSlim _sendLock; // todo lock allgemein? // todo weg?
        private List<DrfMessage> _drfMessages = new List<DrfMessage>();
        private static readonly object _messagesLock = new Object();
        private ClientWebSocket _clientWebSocket;
        private bool _isClosing;
        private const int PARTIAL_RECEIVE_BUFFER_SIZE = 4000;
        private const int RECEIVE_BUFFER_SIZE = 10 * PARTIAL_RECEIVE_BUFFER_SIZE;
        private CancellationTokenSource _cancelationTokenSource;
    }
}
