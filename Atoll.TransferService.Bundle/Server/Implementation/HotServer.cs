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

        private HotServerRouteCollection routesCollection;

        private Socket listener;

        private readonly ManualResetEvent allDone = 
            new ManualResetEvent(false);

        public void Start()
        {
            allDone.Reset();
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
            var sock = client.EndAccept(ar);
            var ctx = new HotGetHandlerContext(sock,  config);
            sock.BeginReceive(ctx.Buffer, 0, ctx.BufferSize, 0,
                ReadCallback, ctx);
            Console.WriteLine($"Client connected: {sock.RemoteEndPoint.AddressFamily}");
        }

        public void ReadCallback(IAsyncResult ar)
        {
            var ctx = (HotGetHandlerContext) ar.AsyncState;
            var bytesRead = ctx.Socket.EndReceive(ar);
            if (bytesRead <= 0)
                return;
            if (ctx.DataReceived(bytesRead))
            {
                if (ctx.Handler == null)
                {
                    if (routesCollection.GetRoutes.TryGetValue(ctx.Request.Route, out var factory))
                    {
                        ctx.Handler = factory.Create(ctx);
                        ctx.Handler.Open(ctx);
                    }
                    if (ctx.DataSent())
                        ctx.Socket.BeginSend(ctx.Frame.Buffer, 0, ctx.Frame.Count, 0,
                            SendCallback, ctx);
                }
            }

            //ctx.Socket.BeginReceive(ctx.Buffer, ctx.Request.BytesTransmitted,
            //        ctx.BufferSize - ctx.Request.BytesTransmitted, 0,
            //        ReadCallback, ctx);
        }

        void SendCallback(IAsyncResult ar)
        {
            var ctx = (HotGetHandlerContext)ar.AsyncState;
            ctx.Socket.EndSend(ar);
            if (ctx.ResponseStatus != HttpStatusCode.OK)
            {
                ctx.Dispose();
                return;
            }
            //if (ctx.Handler.DataSent(ctx))
            //{
            //    Complete(ctx.Frame, ctx);
            //    ctx.Ok();
            //}
            //else
            //{
            //    if (ctx.Request.Packet.CommandId == Commands.Put)
            //    {
            //        ctx.Frame.Socket.BeginReceive(ctx.Frame.Buffer, 0, ctx.Frame.BufferSize, 0,
            //            ReadCallback2, ctx);
            //    }
            //    else
            //    {
            //        ctx.Handler.Read(ctx);
            //        ctx.Frame.Socket.BeginSend(ctx.Frame.Buffer, 0, ctx.Frame.BufferLen, 0,
            //            SendCallback, ctx);
            //    }
            //}
        }


        public HotServer UseConfig(HotServerConfiguration aCfg)
        {
            this.config = aCfg;
            return this;
        }

        public HotServer UseRoutes(HotServerRouteCollection aRoutesCollection)
        {
            this.routesCollection = new HotServerRouteCollection();
            foreach (var route in aRoutesCollection.GetRoutes)
                this.routesCollection.RouteGet(route.Key, route.Value);
            foreach (var route in aRoutesCollection.PutRoutes)
                this.routesCollection.RoutePut(route.Key, route.Value);
            return this;
        }

        public void Dispose()
        {
        }
    }
}