using BenchmarkDotNet.Running;
using System;
using System.Threading.Tasks;

namespace FarmingTracker.DrfWebsocket
{
    public class Program
    {

        public static async Task Main()
        {
            var drfToken = "ab277fcd-8441-49fd-99f7-13a73890c448";
            //var drfToken = "wrong token"; // todo debug
            var drfWebSocketClient = CreateDrfWebSocket();

            Console.WriteLine("Connecting 1");
            await drfWebSocketClient.Connect(drfToken);
            Console.WriteLine("Connected 1");

            await Task.Delay(2_000);

            Console.WriteLine("Connecting 2");
            await drfWebSocketClient.Connect(drfToken);
            Console.WriteLine("Connected 2");

            await Task.Delay(1_000);

            Console.WriteLine("Dispose");
            drfWebSocketClient.Dispose();
            Console.WriteLine("Dispose");

            drfWebSocketClient = CreateDrfWebSocket();

            Console.WriteLine("connecting 3");
            await drfWebSocketClient.Connect(drfToken);
            Console.WriteLine("connected 3");

            ////BenchmarkRunner.Run<JsonTest>();

            Console.ReadKey();
        }

        private static DrfWebSocketClient CreateDrfWebSocket()
        {
            var drfWebSocketClient = new DrfWebSocketClient();
            drfWebSocketClient.WebSocketUrl = "ws://localhost:8080"; // todo debug

            drfWebSocketClient.Connecting += (s, e) => Console.WriteLine($"Connecting");
            drfWebSocketClient.Connected += (s, e) => Console.WriteLine($"Connected");
            drfWebSocketClient.ConnectFailed += (s, e) => Console.WriteLine($"ConnectFailed: {e.Data}");
            drfWebSocketClient.ConnectCrashed += (s, e) => Console.WriteLine($"ConnectCrashed: {e.Data}");
            drfWebSocketClient.SendAuthenticationFailed += (s, e) => Console.WriteLine($"SendAuthenticationFailed: {e.Data}");
            drfWebSocketClient.AuthenticationFailed += (s, e) => Console.WriteLine($"AuthenticationFailed: {e.Data}");
            drfWebSocketClient.ReceivedMessage += (s, e) => Console.WriteLine(e.Data);
            drfWebSocketClient.ReceivedUnexpectedBinaryMessage += (s, e) => Console.WriteLine("ReceivedUnexpectedBinaryMessage");
            drfWebSocketClient.UnexpectedNotOpenWhileReceiving += (s, e) => Console.WriteLine($"UnexpectedNotOpenWhileReceiving: {e.Data}");
            drfWebSocketClient.ReceiveCrashed += (s, e) => Console.WriteLine($"ReceiveCrashed: {e.Data}");

            return drfWebSocketClient;
        }
    }
}
