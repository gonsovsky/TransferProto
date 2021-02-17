using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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

        public int TotalReceived;

        protected MemoryStream BufferStream;

        protected BinaryReader BufferReader;

        public HttpStatusCode ResponseStatus;

        public string ResponseMessage;

        public HotContext(Socket socket, HotServerConfiguration config)
        {
            this.Socket = socket;
            this.Config = config;
            this.Buffer = UniArrayPool<byte>.Shared.Rent(config.MinBufferSize);
            this.BufferSize = config.MinBufferSize;
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
            TotalReceived += cnt;
            try
            {
                if (this.Accept != null)
                    return true;
                if (BufferStream == null)
                {
                    BufferStream = new MemoryStream(Buffer);
                    BufferReader = new BinaryReader(BufferStream);
                }
                try
                {
                    BufferStream.Seek(0, SeekOrigin.Begin);
                    var routeLen = BufferReader.ReadInt32();
                    if (BufferOffset >= routeLen + 4)
                    {
                        var routeData = BufferReader.ReadBytes(routeLen);
                        var bodyLen = BufferReader.ReadInt32();
                        if (BufferOffset >= routeLen + bodyLen + 8 + 16)
                        {
                            Accept = new HotAccept() {RouteLen = routeLen, RouteData = routeData, BodyLen = bodyLen};
                            Accept.BodyData = BufferReader.ReadBytes(Accept.BodyLen);
                            Accept.ContentOffset = BufferReader.ReadInt64();
                            Accept.ContentLength = (int) BufferReader.ReadInt64();
                            Accept.Route = Accept.RouteData.MakeString(Accept.RouteLen);
                            BufferOffset = 24 + Accept.BodyLen + Accept.RouteLen;
                            if (BufferOffset > Config.MaxBufferSize)
                            {
                                this.TooLarge("");
                                return false;
                            }
                            return true;
                        }
                    }
                    return false;
                }
                catch (EndOfStreamException) //минимум данных пока не добрался
                {
                    return false;
                }
            }
            finally
            {
                if (BufferOffset >= BufferSize-Config.RecvSize)
                {
                    //TODO: Оптимизировать, смотреть реальный размер буфера
                    var newSize = LimitBufferSize(BufferSize + Config.RecvSize+1);
                    if (newSize <= BufferOffset + Config.RecvSize)
                    {
                        this.TooLarge("");
                    }
                    else
                    {
                        var newBuf = UniArrayPool<byte>.Shared.Rent(newSize);
                        Array.Copy(Buffer, newBuf, BufferSize);
                        UniArrayPool<byte>.Shared.Return(Buffer);
                        Buffer = newBuf;
                        BufferSize = newSize;
                        BufferStream = new MemoryStream(Buffer);
                        BufferReader = new BinaryReader(BufferStream);
                    }
                }
            }
        }

        protected int LimitBufferSize(int want) => Math.Min(Config.MaxBufferSize, want);

        public bool HeadSent;

        public virtual bool DataSent(int dataSize=0)
        {
            if (HeadSent) return false;
            HeadSent = true;
            SendDataCount = 12 + ResponseMessage.Length;
            if (SendData.Length <= SendDataCount)
            {
                var newSize = LimitBufferSize(SendDataCount * 2);
                if (newSize <= SendDataCount)
                {
                    this.TooLarge("");
                    return false;
                }
                UniArrayPool<byte>.Shared.Return(SendData);
                SendData = UniArrayPool<byte>.Shared.Rent(newSize);
            }
            using (var ms = new MemoryStream(SendData, true))
            {
                using (var writer = new BinaryWriter(ms))
                {
                    writer.Write((int)ResponseStatus);
                    writer.Write(ResponseMessage.Length);
                    writer.Write(Encoding.UTF8.GetBytes(ResponseMessage));
                    writer.Write(dataSize);
                }
            }
            return true;
        }

        public virtual int SendDataCount { get; set; }

        public virtual byte[] SendData
        {
            get => Buffer;
            set => Buffer = value;
        }

        public HotContext UnknownRoute(string message = "")
        {
            ResponseStatus = HttpStatusCode.MethodNotAllowed;
            ResponseMessage = message;
            return this;
        }

        public HotContext TooLarge(string message = "")
        {
            ResponseStatus = HttpStatusCode.RequestUriTooLong;
            ResponseMessage = message;
            return this;
        }

        public readonly ManualResetEvent SendDone =
            new ManualResetEvent(false);

        public readonly ManualResetEvent ReceiveDone =
            new ManualResetEvent(false);
    }
}