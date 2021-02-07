using System.Net;
using System.Net.Sockets;
using Corallite.Buffers;

// ReSharper disable once CheckNamespace
namespace Atoll.TransferService
{
    public class HotPutHandlerContext: HotContext, IHotPutHandlerContext
    {
        public IHotPutHandler Handler;

        public HotPutHandlerContext(HotContext source): base(source.Socket, source.Config)
        {
            this.Accept = source.Accept;
            this.Buffer = source.Buffer;
            this.BufferSize = source.BufferSize;
            Request = new HotPutHandlerRequest(Accept.Route, Accept.BodyData, Accept.BodyLen);
            Frame = new HotPutHandlerFrame(this);
            this.Frame.ContentLength = source.Accept.ContentLength;
            this.Frame.ContentOffset = source.Accept.ContentOffset;
        }

        public override void Dispose()
        {
            if (Buffer != null)
                UniArrayPool<byte>.Shared.Return(this.Buffer);
            Buffer = null;
            Socket?.Shutdown(SocketShutdown.Both);
            Socket?.Close();
            Socket = null;
            Handler?.Dispose();
            Handler = null;
            Request?.Dispose();
            Request = null;
            Frame?.Dispose();
            Frame = null;
        }

        public override bool DataReceived(int cnt)
        {
            Frame.Count = cnt;
            Handler.Write(this);
            if (Frame.TotalWrite >= Frame.ContentLength)
            {
                return false;
            }
            return true;
        }

        public override bool DataSent()
        {
            if (base.DataSent())
                return true;
            return false;
        }

        public HotPutHandlerRequest Request { get; set; }

        public HotPutHandlerFrame Frame { get; set; }

        public IHotPutHandlerContext Ok()
        {
            ResponseStatus = HttpStatusCode.OK;
            ResponseMessage = "";
            return this;
        }

        public IHotPutHandlerContext BadRequest(string message = "")
        {
            ResponseStatus = HttpStatusCode.BadRequest;
            ResponseMessage = message;
            return this;
        }

        public IHotPutHandlerContext NotFound(string message = "")
        {
            ResponseStatus = HttpStatusCode.NotFound;
            ResponseMessage = message;
            return this;
        }

        public IHotPutHandlerContext Error(string message = "")
        {
            ResponseStatus = HttpStatusCode.InternalServerError;
            ResponseMessage = message;
            return this;
        }

        public IHotPutHandlerContext NotImplemented(string message = "")
        {
            ResponseStatus = HttpStatusCode.NotImplemented;
            ResponseMessage = message;
            return this;
        }
    }
}
