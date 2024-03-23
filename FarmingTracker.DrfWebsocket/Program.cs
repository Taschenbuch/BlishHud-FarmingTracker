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
            //BenchmarkRunner.Run<JsonTest>();
            Console.ReadKey();
        }
    }
}
