using System;
using Atoll.TransferService.Bundle.Server;

namespace TestServer
{
    internal class Program
    {
        private static void Main()
        {
            var routes = new RoutesCollection()
                            .RouteGet("get", HandlerFactory<MyHotGetFileHandler>.Instance)
                            .RouteGet("list",   HandlerFactory<MyHotListFilesHandler>.Instance)
                            .RoutePut("put", PutHandlerFactory<MyHotPutFileHandler>.Instance);

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
                    .UseConfig(new Config { Port = 3000 });

                server.Start();
            }
        }
    }
}