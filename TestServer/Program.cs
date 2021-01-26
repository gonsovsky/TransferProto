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
                            .RouteGet("download", DefaultGetHandlerFactory<MyHotGetFileHandler>.Instance)
                            .RouteGet("list",     DefaultGetHandlerFactory<MyHotListFilesHandler>.Instance)
                            .RoutePut("upload",   DefaultPutHandlerFactory<MyHotPutFileHandler>.Instance);

            using (var server = new HotServer())
            {
                server
                    .UseRoutes(routes)
                    .UseConfig(new HotServerConfiguration { Port = 3000 });

                server.Start();
            
                Console.WriteLine("Server started. Press 'Enter' to stop.");
                Console.ReadLine();

                server.Stop();

                Console.WriteLine("Press 'Enter' to exit app.");
                Console.ReadLine();
            }
        }

    }


}