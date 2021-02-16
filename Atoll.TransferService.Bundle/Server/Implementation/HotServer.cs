using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Atoll.TransferService.Server.Implementation;

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

        private readonly ManualResetEvent allDone =
            new ManualResetEvent(false);

        private CancellationTokenSource externalToken;

        private CancellationTokenSource localToken;

        public void Start(CancellationTokenSource token)
        {
            externalToken = token ?? throw new ArgumentNullException();

            localToken = CancellationTokenSource.CreateLinkedTokenSource(externalToken.Token);
            Task.Factory.StartNew(this.Start, localToken.Token);
        }

        private void Start()
        {
            allDone.Reset();
            try
            {
                var localEndPoint = new IPEndPoint(IPAddress.Any, config.Port);
                var listener = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream, ProtocolType.Tcp);
                listener.Bind(localEndPoint);
                listener.Listen(100);
                while (!localToken.IsCancellationRequested)
                {
                    allDone.Reset();
                    try
                    {
                        listener.BeginAccept(
                            AcceptCallback,
                            listener);
                    }
                    catch (Exception e)
                    {
                        Misc.Log(e);
                    }
                    allDone.WaitOne();
                    localToken.Token.ThrowIfCancellationRequested();
                }
                //listener.Shutdown(SocketShutdown.Both);
                //listener.Close();
            }
            catch (SocketException e)
            {
                Misc.Log(e.Message);
            }
            catch (OperationCanceledException e)
            {
                if (e.CancellationToken == externalToken.Token)
                    Console.WriteLine("Server is stopped.");
            }
        }

        public void Stop()
        {
            localToken?.Cancel();
        }

        public HotServer UseConfig(HotServerConfiguration aCfg)
        {
            this.config = aCfg;
            return this;
        }

        private static readonly object Locker = new object();

        public HotServer UseRoutes(HotServerRouteCollection aRoutesCollection)
        {
            lock (Locker)
            {
                this.routesCollection = new HotServerRouteCollection();
                foreach (var route in aRoutesCollection.GetRoutes)
                    this.routesCollection.RouteGet(route.Key, route.Value);
                foreach (var route in aRoutesCollection.PutRoutes)
                    this.routesCollection.RoutePut(route.Key, route.Value);
                return this;
            }
        }

        public void Dispose()
        {
            Stop();
        }
        #endregion

        #region Accept

        private void AcceptCallback(IAsyncResult ar)
        {
            allDone.Set();
            Socket sock=null;
            HotContext ctxAccept=null;
            try
            {
                var client = (Socket) ar.AsyncState;
                sock = client.EndAccept(ar);
                if (config.IsKeepAlive)
                    sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                sock.NoDelay = true;
                ctxAccept = new HotContext(sock, config);
                Receive(ctxAccept);
                Misc.Log($"Client connected");
            }
            catch (SocketException ex)
            {
                ctxAccept?.Dispose();
                sock?.Shutdown(SocketShutdown.Both);
                sock?.Close();
                Misc.Log(ex.Message);
            }
        }
        #endregion

        #region ReUse
        private void Accomplish(HotContext ctx)
        {
            ctx.SendDone.WaitOne();
            ctx.ReceiveDone.WaitOne();
            var sock = ctx.Socket;
            Misc.Log($"Client finished with: {ctx.ResponseStatus}");
            ctx.Dispose();
            allDone.Set();
            if (config.IsKeepAlive)
            {
                ctx = new HotContext(sock, config);
                Receive(ctx);
            }
        }

        private void Abort(IDisposable ctx, Exception e)
        {
            Misc.Log($"Client aborted: {e.Message}");
            ctx.Dispose();
            allDone.Set();
        }
        #endregion

        #region Receive
#if NETSTANDARD
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#else
        [MethodImpl(256)]
#endif
        private void Receive(HotContext ctx)
        {
            ctx.ReceiveDone.Set();
            if (ctx is HotPutHandlerContext ctxUp) //file uploading branch. (Head allready received)
                ctxUp.Socket.BeginReceive(ctxUp.Frame.Buffer, 0,
                    ctxUp.Frame.BufferSize, 0, ReceiveCallBack, ctxUp);
            else
                ctx.Socket.BeginReceive(ctx.Buffer, ctx.BufferOffset, //receiving head of any request
                    ctx.BufferSize - ctx.BufferOffset, 0, ReceiveCallBack, ctx);
            return;
        }

        private void ReceiveCallBack(IAsyncResult ar)
        {
            var ctxAccept = (HotContext)ar.AsyncState;

            #region File uploading branch
            if (ctxAccept is HotPutHandlerContext ctxUpload)
            {
                try
                {
                    var bytesRead = ctxUpload.Socket.EndReceive(ar);
                    if (bytesRead <= 0 || ctxUpload.DataReceived(bytesRead) == false)
                    {
                        ctxAccept.ReceiveDone.Reset();
                        Accomplish(ctxUpload);
                        return;
                    }
                    localToken.Token.ThrowIfCancellationRequested();
                    Receive(ctxUpload);
                }
                catch (SocketException e)
                {
                    Abort(ctxUpload, e);
                }
                return;
            }
            #endregion

            HotGetHandlerContext ctxGet = null;
            HotPutHandlerContext ctxUp = null;
            try
            {
                var bytesRead = ctxAccept.Socket.EndReceive(ar);
                if (bytesRead <= 0)
                    return;
                localToken.Token.ThrowIfCancellationRequested();
                if (ctxAccept.DataReceived(bytesRead))
                {
                    switch (ctxAccept.Accept.Route)
                    {
                        case Routes.Download:
                        case Routes.List:
                            ctxGet = new HotGetHandlerContext(ctxAccept);
                            if (routesCollection.GetRoutes.TryGetValue(ctxAccept.Accept.Route, out var factory))
                            {
                                ctxGet.Handler = factory.Create(ctxGet);
                                ctxGet.Handler.Open(ctxGet);
                            }
                            if (ctxGet.DataSent())
                                Send(ctxGet);
                            break;
                        case Routes.Upload:
                            ctxUp = new HotPutHandlerContext(ctxAccept);
                            if (routesCollection.PutRoutes.TryGetValue(ctxAccept.Accept.Route, out var factoryUp))
                            {
                                ctxUp.Handler = factoryUp.Create(ctxUp);
                                ctxUp.Handler.Open(ctxUp);
                            }
                            if (ctxUp.DataSent())
                                Send(ctxUp);
                            if (ctxUp.DataReceived(bytesRead - ctxUp.Frame.BufferOffset) == false) //read rest of bytes from HotContext
                            {
                                Accomplish(ctxUp);
                                return;
                            }
                            Receive(ctxUp);
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
                    Receive(ctxAccept);
                }
            }
            catch (SocketException ex)
            {
                ctxGet?.Dispose();
                ctxUp?.Dispose();
                Misc.Log(ex.Message);
            }
            finally
            {
                ctxAccept?.Dispose();
            }
        }
        #endregion

        #region Send
#if NETSTANDARD
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#else
        [MethodImpl(256)]
#endif
        private void Send(HotContext ctx, bool now = false)
        {
            ctx.SendDone.Set();
            if (now)
            {
                try
                {
                    ctx.Socket.BeginSend(ctx.SendData, 0, ctx.SendDataCount, 0,
                        SendCallback, ctx);
                    return;
                }
                catch (SocketException e)
                {
                    Abort(ctx, e);
                    return;
                }
            }
            if (config.Delay == 0)
                Send(ctx, true);
            else
                Misc.Delay(config.Delay).ContinueWith(a => Send(ctx, true));
        }

        private void SendCallback(IAsyncResult ar)
        {
            var ctx = (HotContext)ar.AsyncState;
            try
            {
                localToken.Token.ThrowIfCancellationRequested();
                ctx.Socket.EndSend(ar);
                if (ctx.ResponseStatus != HttpStatusCode.OK)
                {
                    ctx.SendDone.Reset();
                    Accomplish(ctx);
                }
                else
                if (ctx.DataSent())
                    Send(ctx);
                else if (!(ctx is HotPutHandlerContext))
                {
                    ctx.SendDone.Reset();
                    Accomplish(ctx);
                }
            }
            catch (SocketException e)
            {
                Abort(ctx, e);
            }
        }
        #endregion
    }
}