using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Atoll.TransferService
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
            Console.WriteLine($"Client connected");
        }

        public void ReadCallback(IAsyncResult ar)
        {
            var ctx = (HotGetHandlerContext) ar.AsyncState;
            var bytesRead = ctx.Socket.EndReceive(ar);
            if (bytesRead <= 0)
                return;
            if (ctx.DataReceived(bytesRead))
            {
                if (ctx.Handler != null)
                    return;
                if (routesCollection.GetRoutes.TryGetValue(ctx.Request.Route, out var factory))
                {
                    ctx.Handler = factory.Create(ctx);
                    ctx.Handler.Open(ctx);
                }
                if (ctx.DataSent())
                    Send(ctx);
            }
            else
            {
                Recv(ctx);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Recv(HotGetHandlerContext ctx, bool now = false)
        {
            if (now)
            {
                ctx.Socket.BeginReceive(ctx.Buffer, ctx.BufferOffset,
                    ctx.BufferSize - ctx.BufferOffset, 0,
                    ReadCallback, ctx);
                return;
            }
            if (config.RecvDelay == 0)
                Recv(ctx, true);
            else
                Task.Delay(config.RecvDelay).ContinueWith(a => Recv(ctx, true));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Send(HotGetHandlerContext ctx, bool now = false)
        {
            if (now)
            {
                ctx.Socket.BeginSend(ctx.Frame.Buffer, 0, ctx.Frame.BytesRead, 0,
                        SendCallback, ctx);
                return;
            }
            if (config.SendDelay == 0)
                Send(ctx, true);
            else
                Task.Delay(config.SendDelay).ContinueWith(a => Send(ctx, true));
        }

        void SendCallback(IAsyncResult ar)
        {
            var ctx = (HotGetHandlerContext)ar.AsyncState;
            ctx.Socket.EndSend(ar);
            if (ctx.ResponseStatus != HttpStatusCode.OK)
            {
                Console.WriteLine($"Client disconnected");
                ctx.Dispose();
                allDone.Set();
                return;
            }
            if (ctx.DataSent())
                Send(ctx);
            else
            {
                Console.WriteLine($"Client disconnected");
                ctx.Dispose();
                allDone.Set();
            }
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