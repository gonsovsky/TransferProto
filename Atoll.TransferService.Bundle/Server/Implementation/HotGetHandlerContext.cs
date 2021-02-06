using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Corallite.Buffers;

namespace Atoll.TransferService
{
    public class HotGetHandlerContext: IHotGetHandlerContext, IDisposable
    {
        public Socket Socket;

        public readonly HotServerConfiguration Config;

        public byte[] Buffer;

        public int BufferOffset;

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
            Handler?.Dispose();
            Handler = null;
            Request?.Dispose();
            Request = null;
            Frame?.Dispose();
            Frame = null;
            Socket?.Shutdown(SocketShutdown.Both);
            Socket?.Close();
            Socket = null;
        }

        public bool DataReceived(int cnt)
        {
            BufferOffset += cnt;
            if (this.Request != null)
                return true;

            try
            {
                using (var reader = new BinaryReader(new MemoryStream(Buffer)))
                {
                    var routeLen = reader.ReadInt32();
                    var routeData = reader.ReadBytes(routeLen);
                    var bodyLen = reader.ReadInt32();
                    var bodyData = reader.ReadBytes(bodyLen);
                    Request = new HotGetHandlerRequest(routeData.MakeString(routeLen), bodyData, bodyLen);
                    Frame = new HotGetHandlerFrame(this)
                    {
                        ContentOffset = reader.ReadInt64(),
                        ContentLength = (int)reader.ReadInt64()
                    };
                }

                return true;
            }
            catch (EndOfStreamException) //минимум данных пока не добрался
            {
                return false;
            }
        }

        private bool headSent;

        public bool DataSent()
        {
            if (!headSent)
            {
                headSent = true;
                using (var ms = new MemoryStream(Frame.Buffer, true))
                {
                    using (var writer = new BinaryWriter(ms))
                    {
                        writer.Write((int) ResponseStatus);
                        writer.Write((int) ResponseMessage.Length);
                        writer.Write(Encoding.UTF8.GetBytes(ResponseMessage));
                    }
                }
                Frame.BytesRead = 8 + ResponseMessage.Length;
                return true;
            }
            if (Frame.BytesRead == 0) //раз в прошлый раз прочитали 0 из Handler'a- значит EOF
                return false;
            Frame.Count = BufferSize;
            this.Handler.Read(this);
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
