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
            Request.Socket = socket;
            Server = srv;
        }

        public HttpStatusCode CallBackStatus;

        public HotServer Server { get; set; }

        public IHotGetHandler Handler { get; set; }

        public HotGetHandlerRequest Request { get; }

        public HotGetHandlerFrame Frame { get; set; }

        public IHotGetHandlerContext Ok()
        {
            CallBackStatus = HttpStatusCode.OK;
            return this;
        }

        public IHotGetHandlerContext BadRequest(string message = null)
        {
            CallBackStatus = HttpStatusCode.BadRequest;
            return this;
        }

        public IHotGetHandlerContext NotFound(string message = null)
        {
            CallBackStatus = HttpStatusCode.NotFound;
            return this;
        }

        public IHotGetHandlerContext Error(string message = null)
        {
            CallBackStatus = HttpStatusCode.InternalServerError;
            return this;
        }

        public IHotGetHandlerContext NotImplemented(string message = null)
        {
            CallBackStatus = HttpStatusCode.NotImplemented;
            return this;
        }
    }
}
