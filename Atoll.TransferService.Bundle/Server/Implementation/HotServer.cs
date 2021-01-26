using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Atoll.TransferService.Bundle.Server.Contract;
using Atoll.TransferService.Bundle.Server.Contract.Get;

namespace Atoll.TransferService.Bundle.Server.Implementation
{

    /// <summary>
    /// Реализация сервера приема и передачи данных.
    /// </summary>
    public sealed class HotServer : IDisposable
    {
        private HotServerConfiguration config;

        private HotServerRouteCollection routes;

        private Socket listener;

        private readonly ManualResetEvent allDone =
            new ManualResetEvent(false);

        public void Start()
        {
            var localEndPoint = new IPEndPoint(IPAddress.Any, config.Port);
            listener = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(localEndPoint);
            listener.Listen(100);
            while (true)
            {
                allDone.Reset();
                listener.BeginAccept(
                    AcceptCallback,
                    listener);
                allDone.WaitOne();
            }
        }

        public void Stop()
        {
            listener.Shutdown(SocketShutdown.Both);
            listener.Close();
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            allDone.Set();
            var client = (Socket)ar.AsyncState;
            var handler = client.EndAccept(ar);
            var ctx = new HotGetHandlerContext(listener, config);
            handler.BeginReceive(ctx.Request.Data, 0, ctx.Request.DataLength, 0,
                ReadCallback, ctx);
        }

        public void ReadCallback(IAsyncResult ar)
        {
            var ctx = (HotGetHandlerContext)ar.AsyncState;
            var bytesRead = ctx.Socket.EndReceive(ar);
            if (bytesRead <= 0)
                return;
            if (ctx.Request.DataArrived(bytesRead))
            {
                try
                {
                    if (ctx.Handler == null)
                    {
                        if (routes.GetRoutes.TryGetValue(ctx.Request.Route, out IHotGetHandlerFactory factory))
                        {
                            ctx.Frame = new HotGetHandlerFrame(config);
                            ctx.Handler = factory.Create(ctx);
                            ctx.Handler.Open(ctx);
                        }
                        else
                        {
                            throw new ApplicationException($"Unregistered route {ctx.Request.Route}");
                        }
                    }
                    ctx.Handler.Read(ctx);
                    ctx.Socket.BeginSend(ctx.Frame.Buffer, 0, ctx.Frame.BytesRead, 0,
                        SendCallback, ctx);
                }
                finally
                {
                    ctx.Request.DataRelease();
                }
            }
            else
            {
                ctx.Socket.BeginReceive(ctx.Request.Data, ctx.Request.BytesRead, ctx.Request.DataLength, 0,
                    ReadCallback, ctx);
            }
        }

        private static void SendCallback(IAsyncResult ar)
        {
            var ctx = (HotGetHandlerContext) ar.AsyncState;
            ctx.Socket.EndSend(ar);
            ctx.Handler.Read(ctx);
            ctx.Socket.BeginSend(ctx.Frame.Buffer, 0, ctx.Frame.BytesRead, 0,
                SendCallback, ctx);
        }


        public HotServer UseConfig(HotServerConfiguration aCfg)
        {
            this.config = aCfg;
            return this;
        }

        public HotServer UseRoutes(HotServerRouteCollection aRoutes)
        {
            this.routes = new HotServerRouteCollection();
            foreach (var route in aRoutes.GetRoutes)
                this.routes.RouteGet(route.Key, route.Value);
            foreach (var route in aRoutes.PutRoutes)
                this.routes.RoutePut(route.Key, route.Value);
            return this;
        }

        public void Dispose()
        {
        }
    }
}