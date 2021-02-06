using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Corallite.Buffers;


namespace Atoll.TransferService
{
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
            //nothing
        }

        public virtual bool DataReceived(int cnt)
        {
            BufferOffset += cnt;
            if (this.Accept != null)
                return true;
            try
            {
                using (var reader = new BinaryReader(new MemoryStream(Buffer)))
                {
                    Accept = new HotAccept {RouteLen = reader.ReadInt32()};
                    Accept.RouteData  = reader.ReadBytes(Accept.RouteLen);
                    Accept.BodyLen = reader.ReadInt32();
                    Accept.BodyData = reader.ReadBytes(Accept.BodyLen);
                    Accept.ContentOffset = reader.ReadInt64();
                    Accept.ContentLength = (int) reader.ReadInt64();
                    Accept.Route = Accept.RouteData.MakeString(Accept.RouteLen);
                }
                return true;
            }
            catch (EndOfStreamException) //минимум данных пока не добрался
            {
                return false;
            }
        }

        public bool HeadSent;

        public virtual bool DataSent()
        {
            if (HeadSent) return false;
            HeadSent = true;
            using (var ms = new MemoryStream(SendData, true))
            {
                using (var writer = new BinaryWriter(ms))
                {
                    writer.Write((int)ResponseStatus);
                    writer.Write((int)ResponseMessage.Length);
                    writer.Write(Encoding.UTF8.GetBytes(ResponseMessage));
                }
            }
            SendBytes = 8 + ResponseMessage.Length;
            return true;
        }

        public virtual int SendBytes
        {
            get
            {
                //must be overrided
                throw new NotImplementedException();
            }
            set
            {
                //must be overrided
                throw new NotImplementedException();
            }
        }

        public virtual byte[] SendData
        {
            get
            {
                //must be overrided
                throw new NotImplementedException();
            }
        }
    }
}