using System.Net;
using System.Net.Sockets;
using Corallite.Buffers;

// ReSharper disable once CheckNamespace
namespace Atoll.TransferService
{
    public class HotGetHandlerContext: HotContext, IHotGetHandlerContext
    {
        public IHotGetHandler Handler;

        public HotGetHandlerContext(HotContext source): base(source.Socket, source.Config)
        {
            this.Accept = source.Accept;
            this.Buffer = source.Buffer;
            this.BufferSize = source.BufferSize;
            Request = new HotGetHandlerRequest(Accept.Route, Accept.BodyData, Accept.BodyLen);
            Frame = new HotGetHandlerFrame(this);
            this.Frame.ContentLength = source.Accept.ContentLength;
            this.Frame.ContentOffset = source.Accept.ContentOffset;
        }

        public override void Dispose()
        {
            base.Dispose();
            if (Buffer != null)
                UniArrayPool<byte>.Shared.Return(this.Buffer);
            Buffer = null;
            if (!Config.IsKeepAlive)
            {
                Socket?.Shutdown(SocketShutdown.Both);
                Socket?.Close();
                Socket = null;
            }
            Handler?.Dispose();
            Handler = null;
            Request?.Dispose();
            Request = null;
            Frame?.Dispose();
            Frame = null;
        }

        public override bool DataReceived(int cnt)
        {
            return false;
        }

        public override bool DataSent(int datasize=0)
        {
            if (base.DataSent(Frame.HaveToRead))
                return true;
            if (TransmitBytes == 0)
                return false;
            Frame.Count = BufferSize;
            this.Handler.Read(this);
            return true;
        }

        public override byte[] TransmitData => Frame.Buffer;

        public override int TransmitBytes
        {
            get => Frame.BytesRead;
            set => Frame.BytesRead = value;
        }

        public HotGetHandlerRequest Request { get; set; }

        public HotGetHandlerFrame Frame { get; set; }

        public IHotGetHandlerContext Ok()
        {
            ResponseStatus = HttpStatusCode.OK;
            ResponseMessage = "";
            return this;
        }

        public IHotGetHandlerContext BadRequest(string message = "")
        {
            ResponseStatus = HttpStatusCode.BadRequest;
            ResponseMessage = message;
            return this;
        }

        public IHotGetHandlerContext NotFound(string message = "")
        {
            ResponseStatus = HttpStatusCode.NotFound;
            ResponseMessage = message;
            return this;
        }

        public IHotGetHandlerContext Error(string message = "")
        {
            ResponseStatus = HttpStatusCode.InternalServerError;
            ResponseMessage = message;
            return this;
        }

        public IHotGetHandlerContext NotImplemented(string message = "")
        {
            ResponseStatus = HttpStatusCode.NotImplemented;
            ResponseMessage = message;
            return this;
        }
    }
}
