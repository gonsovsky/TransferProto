using System.Net;
using System.Net.Sockets;
using Atoll.TransferService.Bundle.Server.Contract;

namespace Atoll.TransferService.Bundle.Server.Handler
{
    public class Context: IContext
    {
        public Context(Socket socket, HotServer srv, Config config)
        {
            Request = new Request(config.BufferSize) {Socket = socket};
            Server = srv;
        }

        public HttpStatusCode CallBackStatus;

        public HotServer Server { get; set; }

        public IHandler Handler { get; set; }

        public Request Request { get; }

        public Frame Frame { get; set; }

        public IContext Ok()
        {
            CallBackStatus = HttpStatusCode.OK;
            return this;
        }

        public IContext BadRequest(string message = null)
        {
            CallBackStatus = HttpStatusCode.BadRequest;
            return this;
        }

        public IContext NotFound(string message = null)
        {
            CallBackStatus = HttpStatusCode.NotFound;
            return this;
        }

        public IContext Error(string message = null)
        {
            CallBackStatus = HttpStatusCode.InternalServerError;
            return this;
        }

        public IContext NotImplemented(string message = null)
        {
            CallBackStatus = HttpStatusCode.NotImplemented;
            return this;
        }
    }
}
