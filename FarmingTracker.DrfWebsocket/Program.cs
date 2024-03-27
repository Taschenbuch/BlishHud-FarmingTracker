using BenchmarkDotNet.Running;
using System;
using System.Threading.Tasks;

namespace FarmingTracker.DrfWebsocket
{
    public class Program
    {
        public static async Task Main()
        {
            //var drfServerUrl = "ws://localhost:8080";
            var drfServerUrl = "wss://drf.rs/ws";
            var drfWebSocketClient = new DrfWebSocketClient();
            
            drfWebSocketClient.ConnectFailed += (s, e) => Console.WriteLine("ConnectFailed");
            drfWebSocketClient.ConnectCrashed += (s, e) => Console.WriteLine("ConnectCrashed");
            drfWebSocketClient.AuthenticationFailed += (s, e) => Console.WriteLine("AuthenticationFailed");
            drfWebSocketClient.ReceivedUnexpectedBinaryMessage += (s, e) => Console.WriteLine("ReceivedUnexpectedBinaryMessage");
            drfWebSocketClient.ReceivedUnexpectedNotOpen += (s, e) => Console.WriteLine("ReceivedUnexpectedNotOpen");
            drfWebSocketClient.ReceivedCrashed += (s, e) => Console.WriteLine("ReceivedCrashed");

            Console.WriteLine("Connecting 1");
            await drfWebSocketClient.Connect("a886872e-d942-4766-b499-e5802359d93a", drfServerUrl);
            Console.WriteLine("Connected 1");

            await Task.Delay(10_000);

            Console.WriteLine("Connecting 2");
            await drfWebSocketClient.Connect("a886872e-d942-4766-b499-e5802359d93a", drfServerUrl);
            Console.WriteLine("Connected 2");

            await Task.Delay(10_000);

            Console.WriteLine("Closing");
            drfWebSocketClient.Close();
            Console.WriteLine("Closed");
            
            await Task.Delay(10_000);
            
            Console.WriteLine("connecting 3");
            await drfWebSocketClient.Connect("a886872e-d942-4766-b499-e5802359d93a", drfServerUrl);
            Console.WriteLine("connected 3");
            
            ////BenchmarkRunner.Run<JsonTest>();
            
            Console.ReadKey();
        }
    }
}
