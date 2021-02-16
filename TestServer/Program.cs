using System;
using System.Threading;
using Atoll.TransferService;
using Atoll.TransferService.Server.Implementation;

namespace TestServer
{
    internal class Program
    {
        static CancellationTokenSource Cts = new CancellationTokenSource();

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
                        Port = 3000, BufferSize = 256, Delay = 200, KeepAlive = 1
                    });

                server.Start(Cts);

                while (true)
                {
                    Console.WriteLine("Server started. Press 'Enter' to stop.");
                    Console.ReadKey();
                    server.Stop();

                    Console.WriteLine("Server stopped. Press 'Enter' to start.");
                    Console.ReadKey();
                    Cts = new CancellationTokenSource();
                    server.Start(Cts);
                }
            }
        }
    }
}