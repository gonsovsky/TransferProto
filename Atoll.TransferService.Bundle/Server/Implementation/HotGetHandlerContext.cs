using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Atoll.TransferService.Bundle.Server.Contract;
using Atoll.TransferService.Bundle.Server.Contract.Get;
using Corallite.Buffers;

namespace Atoll.TransferService.Bundle.Server.Implementation
{
    public class HotGetHandlerContext: IHotGetHandlerContext, IDisposable
    {
        public readonly Socket Socket;

        public readonly HotServerConfiguration Config;

        public byte[] Buffer;

        public readonly int BufferSize;

        public IHotGetHandler Handler;

        public HttpStatusCode ResponseStatus;
        
        public string ResponseMessage;

        public HotGetHandlerContext(Socket socket, HotServerConfiguration config)
        {
            this.Socket = socket;
            this.Config = config;
            this.Buffer = UniArrayPool<byte>.Shared.Rent(config.BufferSize);
            this.BufferSize = config.BufferSize;
        }

        public void Dispose()
        {
            if (Buffer != null)
                UniArrayPool<byte>.Shared.Return(this.Buffer);
            Buffer = null;
            Request?.Dispose();
            Request = null;
            Frame?.Dispose();
            Frame = null;
        }

        public bool DataReceived(int cnt)
        {
            if (this.Request != null)
                return true;

            if (cnt < 8) //TODO: я не знаю, как это решить не имея некой структуры ответственной за "конверт" с данными
                return false;

            using (var reader = new BinaryReader(new MemoryStream(Buffer)))
            {
                var routeLen = reader.ReadInt32();
                var routeData = reader.ReadBytes(routeLen);
                var bodyLen = reader.ReadInt32();
                var bodyData = reader.ReadBytes(bodyLen);
                Request = new HotGetHandlerRequest(routeData.MakeString(routeLen), bodyData, bodyLen);
                Frame = new HotGetHandlerFrame(this);
                Frame.ContentOffset = reader.ReadInt64();
                Frame.Count = (int)reader.ReadInt64();
            }
            return true;
        }

        public bool DataSent()
        {
            using (var ms = new MemoryStream(Frame.Buffer, true))
            {
                using (var writer = new BinaryWriter(ms))
                {
                    writer.Write((int)ResponseStatus);
                    writer.Write((int)ResponseMessage.Length);
                    writer.Write(Encoding.UTF8.GetBytes(ResponseMessage));
                }
            }
            Frame.Count = 8 + ResponseMessage.Length;
            return true;
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
