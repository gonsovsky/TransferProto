using System;
using System.Collections.Concurrent;
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
            config.Validate();
            externalToken = token ?? throw new ArgumentNullException();

            localToken = CancellationTokenSource.CreateLinkedTokenSource(externalToken.Token);
            Task.Factory.StartNew(this.Start, localToken.Token);
        }

        private void Start()
        {
            Socket listener=null;
            allDone.Reset();
            try
            {
                var localEndPoint = new IPEndPoint(IPAddress.Any, config.Port);
                listener = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream, ProtocolType.Tcp);
                listener.Bind(localEndPoint);
                listener.Listen(100);
                while (!externalToken.IsCancellationRequested)
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
                }

            }
            catch (Exception e)
            {
                Misc.Log($"[Exception] [Start] {e.Message}");
            }
            finally
            {
                listener?.Close();
                listener?.Dispose();
            }
        }

        public void Stop()
        {
            allDone.Set();
            externalToken?.Cancel();
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
                Misc.Log($"[info] Client accepted");
            }
            catch (Exception ex)
            {
                ctxAccept?.Dispose();
                sock?.Shutdown(SocketShutdown.Both);
                sock?.Close();
            }
            allDone.Set();
        }
        #endregion

        #region ReUse
        private void Accomplish(HotContext ctx)
        {
            ctx.SendDone.WaitOne();
            ctx.ReceiveDone.WaitOne();
            var sock = ctx.Socket;
            Misc.Log($"[info] {ctx.GetType().Name} finished with: {ctx.ResponseStatus}");
            ctx.Dispose();
            allDone.Set();
            if (config.IsKeepAlive)
            {
                ctx = new HotContext(sock, config);
                Receive(ctx);
            }
        }

        private void Abort(HotContext ctx, Exception e, bool socketClose=false)
        {
            if (e != null)
                Misc.Log($"[info] Client aborted: {e.Message}");
            if (config.IsKeepAlive && socketClose)
            {
                ctx.Socket.Shutdown(SocketShutdown.Both);
                ctx.Socket.Close();
            }
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
            if (ctx is HotPutHandlerContext ctxUp) //file uploading branch. (Head allready received)
                ctxUp.Socket.BeginReceive(ctxUp.Frame.Buffer, 0,
                    config.RecvSize, 0, ReceiveCallBack, ctxUp);
            else
                ctx.Socket.BeginReceive(ctx.Buffer, ctx.BufferOffset, //receiving head of any request
                    config.RecvSize, 0, ReceiveCallBack, ctx);
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
                        ctxUpload.ReceiveDone.Set();
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
                            ctxGet.ReceiveDone.Set();
                            CheckAndSend(ctxGet);
                            break;
                        case Routes.Upload:
                            ctxUp = new HotPutHandlerContext(ctxAccept);
                            if (routesCollection.PutRoutes.TryGetValue(ctxAccept.Accept.Route, out var factoryUp))
                            {
                                ctxUp.Handler = factoryUp.Create(ctxUp);
                                ctxUp.Handler.Open(ctxUp);
                            }
                            CheckAndSend(ctxUp);
                            if (ctxUp.ResponseStatus == HttpStatusCode.OK)
                            {
                                var rest = ctxAccept.TotalReceived - ctxUp.Frame.BufferOffset;
                                if (rest != 0 && ctxUp.DataReceived(rest) == false) //read rest of bytes from HotContext
                                {
                                    ctxUp.ReceiveDone.Set();
                                    Accomplish(ctxUp);
                                    return;
                                }
                                Receive(ctxUp);
                            }
                            else
                            {
                                ctxUp.ReceiveDone.Set();
                            }
                            break;
                        default:
                            ctxAccept.ReceiveDone.Set();
                            ctxAccept.UnknownRoute($"Route is not supported");
                            CheckAndSend(ctxAccept);
                            break;
                    }
                }
                else
                {
                    if (ctxAccept.ResponseStatus == HttpStatusCode.RequestUriTooLong)
                    {
                        ctxAccept.BufferOffset = 0;
                        ctxAccept.ReceiveDone.Set();
                        CheckAndSend(ctxAccept);
                        ctxAccept.SendDone.WaitOne();
                        Abort(ctxAccept, null,true);
                        return;
                    }
                    Receive(ctxAccept);
                }
            }
            catch (Exception ex)
            {
                if (ctxGet!=null)
                    Abort(ctxGet,ex,true);
                if (ctxUp != null)
                    Abort(ctxUp, ex, true);
                Misc.Log($"[exception] [ReceiveCallBack] {ex.Message}");
            }
            finally
            {
                ctxAccept?.Dispose();
            }
        }
        #endregion

        #region Send
        private bool CheckAndSend(HotContext ctx)
        {
            var ok = ctx.DataSent();
            if (ok)
                Send(ctx);
            else ctx.SendDone.Set();
            return ok;
        }

#if NETSTANDARD
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#else
        [MethodImpl(256)]
#endif
        private void Send(HotContext ctx, bool now = false)
        {
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
            if (config.SendDelay == 0)
                Send(ctx, true);
            else
                Misc.Delay(config.SendDelay).ContinueWith(a => Send(ctx, true));
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
                    ctx.SendDone.Set();
                    Accomplish(ctx);
                }
                else if (!CheckAndSend(ctx))
                {
                    if (!(ctx is HotPutHandlerContext))
                        Accomplish(ctx);
                }
            }
            catch (Exception e)
            {
                Abort(ctx, e);
            }
        }
        #endregion
    }
}