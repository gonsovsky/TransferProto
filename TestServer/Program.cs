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

            using (var server = new HotServer()
            {
                OnRequest = (party, state) => { }
                // Console.WriteLine($"Hub Request : {state.Url} [{state.Gram.Start}/{state.Gram.Length}]")
                ,
                OnResponse = (party, state) => { }
                // Console.WriteLine($"Hub Response: {state.Url} [{state.Gram.Start}/{state.Gram.Length}]")
                ,
                OnAbort = (party, state, ex) => { }
                //  Console.WriteLine($"Hub Abort   : {ex.Message}")
            })
            {
                server
                    .UseRoutes(routes)
                    .UseConfig(new HotServerConfiguration { Port = 3000 });

                server.Start();
            }
        }
    }
}