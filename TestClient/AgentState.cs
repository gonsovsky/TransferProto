using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Corallite.Buffers;

namespace TestClient
{
    public class AgentState: IDisposable
    {
        public Socket Socket;

        public byte[] Buffer;

        public int BufferSize;

        public int BufferLen;

        public int BytesRecv;

        public SendPacket SendPacket;

        protected bool HeadRecv;

        public bool HeadSent;

        public AgentState(int bufferSize, string route, string url, long offset, long length, Stream data, IFs fs)
        {
            this.fs =fs;
            this.SendData = data;
            this.Buffer = UniArrayPool<byte>.Shared.Rent(bufferSize);
            SendPacket = new SendPacket()
            {
                Route = route,
                Body = url,
                Offset = offset,
                Length = length
            };
            this.BufferSize = bufferSize;
        }

        protected Stream RecvStream;

        public RecvPacket RecvPacket;

        public bool DataReceived(int len)
        {
            BytesRecv += len;
            var headDelta = 0;
            if (!HeadRecv)
            {
                HeadRecv = true;
                BufferLen = len;
                if (BufferLen < RecvPacket.MinSize)
                    return false;
                RecvPacket = RecvPacket.FromByteArray(Buffer, len);
                headDelta = RecvPacket.MySize;
            }
            if (RecvStream == null && RecvPacket.StatusCode == HttpStatusCode.OK)
            {
                switch (SendPacket.Route)
                {
                    case "list":
                    case "upload":
                        RecvStream = new MemoryStream();
                        break;
                    case "download":
                        RecvStream = fs.Put(FileName);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            RecvStream?.Write(Buffer, headDelta, len - headDelta);
            return true;
        }

        public bool Send()
        {
            if (HeadSent)
                return false;
            HeadSent = true;
            SendPacket.ToByteArray(ref Buffer);
            BufferLen = SendPacket.MySize;

            //SendData.Seek(bytesSent, SeekOrigin.Begin);
            //var len = (int)Math.Min(BufferSize, (SendData.Length - SendData.Position));
            //var cnt = SendData.Read(Buffer, 0, len);
            //bytesSent += 1;
            //BufferLen = cnt;
            return true;
        }

        public bool SendDataX()
        {
            if (SendPacket.Route != "upload")
                return false;
            var len = (int)Math.Min(BufferSize, (SendData.Length - SendData.Position));
            var cnt = SendData.Read(Buffer, 0, len);
            bytesSent += 1;
            BufferLen = cnt;
            return true;
        }

        private int bytesSent; 

        public bool HasSend()
        {
            if (HeadSent == false)
                return true;
            if (SendData == null)
                return false;
            if (SendPacket.Route != "upload"|| SendData == null)
                return false;
            return SendData.Position < SendData.Length-1;
        }

        private readonly IFs fs;

        protected Stream SendData; 

        public void Dispose()
        {
            RecvStream?.Close();
            RecvStream = null;
            if (Buffer != null)
                UniArrayPool<byte>.Shared.Return(this.Buffer);
            Buffer = null;
        }

        public string Url => SendPacket.Url();

        public string FileName => Path.Combine(Helper.AssemblyDirectory, Url);

        public HttpStatusCode StatusCode => RecvPacket.StatusCode;
    }
}
