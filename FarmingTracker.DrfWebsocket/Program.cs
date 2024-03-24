using BenchmarkDotNet.Running;
using System;
using System.Threading.Tasks;

namespace FarmingTracker.DrfWebsocket
{
    public class Program
    {
        public static async Task Main()
        {
            var drfWebSocketClient = new DrfWebSocketClient();
            await drfWebSocketClient.Connect("a886872e-d942-4766-b499-e5802359d93a");
            //await Task.Delay(10000);
            //await drfWebSocketClient.Connect("a886872e-d942-4766-b499-e5802359d93a");

            //Console.WriteLine("Closing");
            //await drfWebSocketClient.Close();
            //Console.WriteLine("Closed");
            //await Task.Delay(10000);
            //Console.WriteLine("connecting");
            //await drfWebSocketClient.Connect("a886872e-d942-4766-b499-e5802359d93a");
            //Console.WriteLine("connected");
            ////BenchmarkRunner.Run<JsonTest>();
            Console.ReadKey();
        }
    }
}
