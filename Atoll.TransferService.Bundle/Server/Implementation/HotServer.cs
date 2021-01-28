using System;
using System.Net;
using System.Net.Sockets;
using Atoll.TransferService.Bundle.Proto;
using Atoll.TransferService.Bundle.Server.Contract;
using Atoll.TransferService.Bundle.Server.Contract.Get;

namespace Atoll.TransferService.Bundle.Server.Implementation
{
    /// <summary>
    /// Реализация сервера приема и передачи данных.
    /// </summary>
    public sealed class HotServer : Party
    {
        private HotServerConfiguration config;

        private HotServerRouteCollection routes;

        private Socket listener;

        public void Start()
        {
            try
            {
                var localEndPoint = new IPEndPoint(IPAddress.Any, config.Port);
                listener = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream, ProtocolType.Tcp);
                listener.Bind(localEndPoint);
                listener.Listen(100);
                while (true)
                {
                    AllDone.Reset();
                    listener.BeginAccept(
                        AcceptCallback,
                        listener);
                    AllDone.WaitOne();
                }
            }
            catch (Exception e)
            {
                Abort(null, e);
            }
        }

        public void Stop()
        {
            listener.Shutdown(SocketShutdown.Both);
            listener.Close();
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            AllDone.Set();
            var client = (Socket)ar.AsyncState;
            var sock = client.EndAccept(ar);
            var ctx = new HotGetHandlerContext(sock, this, config);
            sock.BeginReceive(ctx.Request.Buffer, 0, ctx.Request.BufferSize, 0,
                ReadCallback, ctx);
        }

        public void ReadCallback(IAsyncResult ar)
        {
            var ctx = (HotGetHandlerContext)ar.AsyncState;
            var bytesRead = ctx.Request.Socket.EndReceive(ar);
            if (bytesRead <= 0)
                return;
            if (ctx.Request.DataTransmitted(bytesRead))
            {
                try
                {
                    if (ctx.Handler == null)
                    {
                        if (routes.GetRoutes.TryGetValue(ctx.Request.Route, out var factory))
                        {
                            ctx.Frame = new HotGetHandlerFrame(config.BufferSize)
                            {
                                Socket = ctx.Request.Socket,
                                Packet = ctx.Request.Packet
                            };
                            ctx.Handler = factory.Create(ctx);
                            ctx.Handler.Open(ctx);
                            ctx.Frame.Packet.StatusCode = ctx.CallBackStatus;
                            OnRequest?.Invoke(this, ctx.Request);
                        }
                        else
                        {
                            throw new ApplicationException($"Unregistered route {ctx.Request.Route}");
                        }
                    }
                    ctx.Frame.Send();
                    ctx.Frame.Socket.BeginSend(ctx.Frame.Buffer, 0, ctx.Frame.BufferLen, 0,
                        SendCallback, ctx);
                }
                finally
                {
                    ctx.Request.Dispose();
                }
            }
            else
            {
                ctx.Frame.Socket.BeginReceive(ctx.Request.Buffer, ctx.Request.BytesTransmitted,
                    ctx.Request.BufferSize - ctx.Request.BytesTransmitted, 0,
                    ReadCallback, ctx);
            }
        }

        void SendCallback(IAsyncResult ar)
        {
            var ctx = (HotGetHandlerContext)ar.AsyncState;
            ctx.Frame.Socket.EndSend(ar);
            if (ctx.Frame.StatusCode != HttpStatusCode.OK)
            {
                Complete(ctx.Frame);
                return;
            }
            if (ctx.Handler.ReadEnd(ctx))
            {
                Complete(ctx.Frame);
                ctx.Ok();
            }
            else
            {
                ctx.Handler.Read(ctx);
                ctx.Frame.Socket.BeginSend(ctx.Frame.Buffer, 0, ctx.Frame.BufferLen, 0,
                    SendCallback, ctx);
            }
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

        public override object Complete(State state)
        {
            state?.Close();
            state?.Dispose();
            return base.Complete(state);
        }
    }
}