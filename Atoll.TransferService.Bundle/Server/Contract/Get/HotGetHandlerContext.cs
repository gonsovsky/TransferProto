using System;
using System.Net;
using System.Net.Sockets;
using Atoll.TransferService.Bundle.Server.Implementation;

namespace Atoll.TransferService.Bundle.Server.Contract.Get
{
    public class HotGetHandlerContext: IHotGetHandlerContext
    {
        public HotGetHandlerContext(Socket socket, HotServer srv, HotServerConfiguration config)
        {
            Request = new HotGetHandlerRequest(config.BufferSize);
            Socket = socket;
            Server = srv;
        }

        public HttpStatusCode CallBack;

        public Socket Socket { get; set; }

        public HotServer Server { get; set; }

        public IHotGetHandler Handler { get; set; }

        public HotGetHandlerRequest Request { get; }

        public HotGetHandlerFrame Frame { get; set; }

        public IHotGetHandlerContext Ok()
        {
            CallBack = HttpStatusCode.OK;
            return this;
        }

        public IHotGetHandlerContext BadRequest(string message = null)
        {
            CallBack = HttpStatusCode.BadRequest;
            return this;
        }

        public IHotGetHandlerContext NotFound(string message = null)
        {
            CallBack = HttpStatusCode.NotFound;
            return this;
        }

        public IHotGetHandlerContext Error(string message = null)
        {
            CallBack = HttpStatusCode.InternalServerError;
            return this;
        }

        public IHotGetHandlerContext NotImplemented(string message = null)
        {
            CallBack = HttpStatusCode.NotImplemented;
            return this;
        }
    }
}
