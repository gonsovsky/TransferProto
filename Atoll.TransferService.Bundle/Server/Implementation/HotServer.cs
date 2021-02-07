using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Atoll.TransferService
{
    /// <summary>
    /// Реализация сервера приема и передачи данных.
    /// </summary>
    public sealed class HotServer : IDisposable
    {
        #region Start/Stop/Init
        private HotServerConfiguration config;

        private HotServerRouteCollection routesCollection;

        private Socket listener;

        private readonly ManualResetEvent allDone =
            new ManualResetEvent(false);

        private readonly CancellationTokenSource cancellationToken = 
            new CancellationTokenSource();

        public void Start()
        {
            allDone.Reset();
            var localEndPoint = new IPEndPoint(IPAddress.Any, config.Port);
            listener = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(localEndPoint);
            listener.Listen(100);
            while (!cancellationToken.IsCancellationRequested)
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
            cancellationToken.Cancel();
            listener.Shutdown(SocketShutdown.Both);
            listener.Close();
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
        #endregion

        #region Accept
        private void AcceptCallback(IAsyncResult ar)
        {
            allDone.Set();
            var client = (Socket)ar.AsyncState;
            var sock = client.EndAccept(ar);
            var ctxAccept = new HotContext(sock, config);
            sock.BeginReceive(ctxAccept.Buffer, 0, ctxAccept.BufferSize, 0,
                ReadCallback, ctxAccept);
            Console.WriteLine($"Client connected");
        }

        public void ReadCallback(IAsyncResult ar)
        {
            var ctxAccept = (HotContext)ar.AsyncState;
            var bytesRead = ctxAccept.Socket.EndReceive(ar);
            if (bytesRead <= 0)
                return;
            if (ctxAccept.DataReceived(bytesRead))
            {
                switch (ctxAccept.Accept.Route)
                {
                    case "download":
                    case "list":
                        var ctx = new HotGetHandlerContext(ctxAccept);
                        if (routesCollection.GetRoutes.TryGetValue(ctxAccept.Accept.Route, out var factory))
                        {
                            ctx.Handler = factory.Create(ctx);
                            ctx.Handler.Open(ctx);
                        }
                        ctxAccept.Dispose();
                        if (ctx.DataSent())
                            Send(ctx);
                        break;
                    case "upload":
                        var ctxUp = new HotPutHandlerContext(ctxAccept);
                        if (routesCollection.PutRoutes.TryGetValue(ctxAccept.Accept.Route, out var factoryUp))
                        {
                            ctxUp.Handler = factoryUp.Create(ctxUp);
                            ctxUp.Handler.Open(ctxUp);
                        }
                        ctxAccept.Dispose();
                        if (ctxUp.DataSent())
                            Send(ctxUp);
                        RecvUpload(ctxUp);
                        break;
                    default:
                        ctxAccept.UnknownRoute($"Route is not supported");
                        if (ctxAccept.DataSent())
                            Send(ctxAccept);
                        break;
                }
            }
            else
            {
                Recv(ctxAccept);
            }
        }
        #endregion

        #region Request And Response
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Recv(HotContext ctx, bool now = false)
        {
            if (now)
            {
                ctx.Socket.BeginReceive(ctx.Buffer, ctx.BufferOffset,
                    ctx.BufferSize - ctx.BufferOffset, 0,
                    ReadCallback, ctx);
                return;
            }
            if (config.Delay == 0)
                Recv(ctx, true);
            else
                Task.Delay(config.Delay).ContinueWith(a => Recv(ctx, true));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Send(HotContext ctx, bool now = false)
        {
            if (now)
            {
                ctx.Socket.BeginSend(ctx.TransmitData, 0, ctx.TransmitBytes, 0,
                    SendCallback, ctx);
                return;
            }
            if (config.Delay == 0)
                Send(ctx, true);
            else
                Task.Delay(config.Delay).ContinueWith(a => Send(ctx, true));
        }

        void SendCallback(IAsyncResult ar)
        {
            var ctx = (HotContext)ar.AsyncState;
            ctx.Socket.EndSend(ar);
            if (ctx.ResponseStatus != HttpStatusCode.OK)
            {
                Console.WriteLine($"Client disconnected {ctx.ResponseStatus}");
                ctx.Dispose();
                allDone.Set();
                return;
            }
            if (ctx.DataSent())
                Send(ctx);
            else if (!(ctx is HotPutHandlerContext))
            {
                Console.WriteLine($"Client disconnected");
                ctx.Dispose();
                allDone.Set();
            }
        }
        #endregion

        #region Upload
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RecvUpload(HotPutHandlerContext ctxUp, bool now = false)
        {
            if (now)
            {
                ctxUp.Socket.BeginReceive(ctxUp.Frame.Buffer, 0,
                    ctxUp.Frame.BufferSize, 0,
                    ReadCallbackUpload, ctxUp);
                return;
            }
            if (config.Delay == 0)
                RecvUpload(ctxUp, true);
            else
                Task.Delay(config.Delay).ContinueWith(a => RecvUpload(ctxUp, true));
        }

        public void ReadCallbackUpload(IAsyncResult ar)
        {
            var ctxUp = (HotPutHandlerContext)ar.AsyncState;
            var bytesRead = ctxUp.Socket.EndReceive(ar);
            if (bytesRead <= 0 || ctxUp.DataReceived(bytesRead) ==false)
            {
                Console.WriteLine($"Upload complete");
                ctxUp.Dispose();
                allDone.Set();
                return;
            }
            else
            {
                RecvUpload(ctxUp);
            }
        }
        #endregion
    }
}