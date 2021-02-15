using System;
using System.Threading;
using Atoll.TransferService;
using Atoll.TransferService.Server.Implementation;

namespace TestServer
{
    internal class Program
    {
        static readonly CancellationTokenSource Cts = new CancellationTokenSource();

        private static void Main()
        {

            var routes = new HotServerRouteCollection()
                .RouteGet<MyHotGetFileHandler>(Routes.Download)
                .RouteGet<MyHotListFilesHandler>(Routes.List)
                .RoutePut<MyHotPutFileHandler>(Routes.Upload);

            using (var server = new HotServer())
            {
                server
                    .UseRoutes(routes)
                    .UseConfig(new HotServerConfiguration
                    {
                        Port = 3000, BufferSize = 512, Delay = 0, KeepAlive = 1
                    });

                server.Start(Cts);

                Console.WriteLine("Server started. Press 'Enter' to stop.");

                Console.ReadKey();
                server.Stop();

                Console.WriteLine("Ready to exit");
                Console.ReadKey();
            }
        }
    }
}