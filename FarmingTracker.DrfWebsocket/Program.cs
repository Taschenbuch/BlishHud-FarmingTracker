using BenchmarkDotNet.Running;
using System;
using System.Threading.Tasks;

namespace FarmingTracker.DrfWebsocket
{
    public class Program
    {

        public static async Task Main()
        {
            var drfApiToken = "62798427-63c5-4df7-b1fb-0a9c014d510c";
            //var drfApiToken = "wrong token"; // todo debug
            var drfWebSocketClient = CreateDrfWebSocket();

            Console.WriteLine("Connecting 1");
            await drfWebSocketClient.Connect(drfApiToken);
            Console.WriteLine("Connected 1");

            await Task.Delay(2_000);

            Console.WriteLine("Connecting 2");
            await drfWebSocketClient.Connect(drfApiToken);
            Console.WriteLine("Connected 2");

            await Task.Delay(1_000);

            Console.WriteLine("Dispose");
            drfWebSocketClient.Dispose();
            Console.WriteLine("Dispose");

            drfWebSocketClient = CreateDrfWebSocket();

            Console.WriteLine("connecting 3");
            await drfWebSocketClient.Connect(drfApiToken);
            Console.WriteLine("connected 3");

            ////BenchmarkRunner.Run<JsonTest>();

            Console.ReadKey();
        }

        private static DrfWebSocketClient CreateDrfWebSocket()
        {
            var drfWebSocketClient = new DrfWebSocketClient();
            drfWebSocketClient.WebSocketUrl = "ws://localhost:8080"; // todo debug

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
