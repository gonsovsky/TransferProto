using System;
using System.Net.Sockets;

namespace Atoll.TransferService.Bundle.Server.Contract.Get
{
    public class HotGetHandlerContext: IHotGetHandlerContext
    {
        public HotGetHandlerContext(Socket socket, HotServerConfiguration config)
        {
            Request = new HotGetHandlerRequest(config.BufferSize);
            Socket = socket;
        }

        public Socket Socket { get; set; }

        public IHotGetHandler Handler { get; set; }

        public HotGetHandlerRequest Request { get; }

        public HotGetHandlerFrame Frame { get; set; }

        public IHotGetHandlerContext Ok()
        {
            throw new NotImplementedException();
        }

        public IHotGetHandlerContext BadRequest(string message = null)
        {
            throw new NotImplementedException();
        }

        public IHotGetHandlerContext NotFound(string message = null)
        {
            throw new NotImplementedException();
        }

        public IHotGetHandlerContext Error(string message = null)
        {
            throw new NotImplementedException();
        }

        public IHotGetHandlerContext NotImplemented(string message = null)
        {
            throw new NotImplementedException();
        }
    }
}
