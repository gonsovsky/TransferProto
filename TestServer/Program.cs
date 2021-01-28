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
                            .RouteGet("listen",   DefaultGetHandlerFactory<MyHotListFilesHandler>.Instance)
                            .RoutePut("upload",   DefaultPutHandlerFactory<MyHotPutFileHandler>.Instance);

            var server = new HotServer()
            {
                OnRequest = (party, state) => 
                    Console.WriteLine($"Server Request  {state.Packet.CommandId}: {state.Url}"),
                OnResponse = (party, state) => 
                    Console.WriteLine($"Server Response {state.Packet.CommandId}: {state.StatusCode}"),
                OnAbort = (party, state, ex) => 
                    Console.WriteLine($"Server Abort    {state.Packet.CommandId}: {ex?.Message}")
            };
            {
                server
                    .UseRoutes(routes)
                    .UseConfig(new HotServerConfiguration { Port = 3000 });

                server.Start();
            }
        }
    }
}