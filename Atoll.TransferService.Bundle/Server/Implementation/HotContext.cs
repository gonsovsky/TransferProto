using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Corallite.Buffers;

// ReSharper disable once CheckNamespace
namespace Atoll.TransferService
{
    /// <summary>
    /// Время жизни экземпляра класса от события Accept до Receive т.е. достаточного для определения Route
    /// Далее экземпляр переходит в конкретный Get или Put Context. 
    /// </summary>
    public class HotContext: IDisposable
    {
        public class HotAccept
        {
            public int RouteLen;
            public byte[] RouteData;
            public int BodyLen;
            public byte[] BodyData;
            public long ContentOffset;
            public long ContentLength;
            public string Route;
        }

        public HotAccept Accept;

        public Socket Socket;

        public readonly HotServerConfiguration Config;

        public byte[] Buffer;

        public int BufferOffset;

        public int BufferSize;

        protected MemoryStream BufferStream;

        protected BinaryReader BufferReader;

        public HttpStatusCode ResponseStatus;

        public string ResponseMessage;

        public HotContext(Socket socket, HotServerConfiguration config)
        {
            this.Socket = socket;
            this.Config = config;
            this.Buffer = UniArrayPool<byte>.Shared.Rent(config.BufferSize);
            this.BufferSize = config.BufferSize;
        }

        public virtual void Dispose()
        {
            BufferReader?.Dispose();
            BufferReader = null;
            BufferStream?.Dispose();
            BufferStream = null;
        }

        public virtual bool DataReceived(int cnt)
        {
            BufferOffset += cnt;
            if (this.Accept != null)
                return true;
            if (BufferStream == null)
            {
                BufferStream = new MemoryStream(Buffer);
                BufferReader = new BinaryReader(BufferStream);
            }

            try
            {
                Accept = new HotAccept {RouteLen = BufferReader.ReadInt32()};
                if (BufferStream.Length < Accept.RouteLen + 4)
                    return false;
                Accept.RouteData = BufferReader.ReadBytes(Accept.RouteLen);
                Accept.BodyLen = BufferReader.ReadInt32();
                if (BufferStream.Length < Accept.RouteLen + Accept.BodyLen + 8)
                    return false;
                Accept.BodyData = BufferReader.ReadBytes(Accept.BodyLen);
                Accept.ContentOffset = BufferReader.ReadInt64();
                Accept.ContentLength = (int)BufferReader.ReadInt64();
                Accept.Route = Accept.RouteData.MakeString(Accept.RouteLen);

                var total = 26 + Accept.BodyLen + Accept.RouteLen;

                BufferOffset = total;

                return true;
            }
            catch (EndOfStreamException) //минимум данных пока не добрался
            {
                return false;
            }
        }

        public bool HeadSent;

        public virtual bool DataSent(int dataSize=0)
        {
            if (HeadSent) return false;
            HeadSent = true;
            using (var ms = new MemoryStream(TransmitData, true))
            {
                using (var writer = new BinaryWriter(ms))
                {
                    writer.Write((int)ResponseStatus);
                    writer.Write(ResponseMessage.Length);
                    writer.Write(Encoding.UTF8.GetBytes(ResponseMessage));
                    writer.Write(dataSize);
                }
            }
            TransmitBytes = 12 + ResponseMessage.Length;
            return true;
        }

        public virtual int TransmitBytes { get; set; }

        public virtual byte[] TransmitData => Buffer;

        public HotContext UnknownRoute(string message = "")
        {
            ResponseStatus = HttpStatusCode.MethodNotAllowed;
            ResponseMessage = message;
            return this;
        }
    }
}