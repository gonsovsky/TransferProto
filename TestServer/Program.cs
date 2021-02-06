using System;
using Atoll.TransferService.Bundle.Server.Contract;
using Atoll.TransferService.Bundle.Server.Implementation;

namespace TestServer
{
    internal class Program
    {
        private static void Main()
        {
            var routes = new HotServerRouteCollection()
                .RouteGet<MyHotGetFileHandler>("download")
                .RouteGet<MyHotListFilesHandler>("list")
                .RoutePut<MyHotPutFileHandler>("upload");

            using (var server = new HotServer())
            {
                server
                    .UseRoutes(routes)
                    .UseConfig(new HotServerConfiguration { Port = 3000, BufferSize = 1024});

                server.Start();

                Console.WriteLine("Server started. Press 'Enter' to stop.");
                Console.ReadLine();

                server.Stop();
            }
        }
    }
}