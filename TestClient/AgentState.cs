using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
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

        public bool KeepAlive;

        public bool HeadSent;

        public string file;

        public AgentState(Config cfg, string route, Contract a, Stream data, IFs fs)
        {
            this.fs =fs;
            this.BufferSize = cfg.BufferSize;
            this.KeepAlive = cfg.IsKeepAlive;
            this.Buffer = UniArrayPool<byte>.Shared.Rent(BufferSize);

            SendPacket = new SendPacket()
            {
                Route = route,
                Body = a.Url,
                Offset = a.Offset,
                Length = a.Length
            };
            if (data != null)
            {
                SendPacket.Length = data.Length - SendPacket.Offset;
            }
            this.SendData = data;
            this.file = a.File;
        }

        public Stream RecvStream;

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
                if (BytesRecv < RecvPacket.MySize)
                {

                }
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
            if (!HeadSent)
            {
                HeadSent = true;
                SendPacket.ToByteArray(ref Buffer);
                BufferLen = SendPacket.MySize;
                return true;
            }
            if (!HasSend())
                return false;
            //SendDataCount.Seek(bytesSent, SeekOrigin.Begin);
            var len = (int)Math.Min(BufferSize, (SendData.Length - SendData.Position));
            var cnt = SendData.Read(Buffer, 0, len);
            bytesSent += cnt;
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
            return SendData.Position < SendData.Length;
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
            if (!KeepAlive)
            {
                try
                {
                    this.Socket?.Shutdown(SocketShutdown.Both);
                    this.Socket?.Close();
                }
                catch (Exception e)
                {
                    //nothing
                }
                this.Socket = null;
            }
        }

        public string Url => SendPacket.Url();

        public string FileName 
        {
            get
            {
                if (string.IsNullOrEmpty(file))
                    return Path.Combine(Helper.AssemblyDirectory, Url);
                return Path.Combine(Helper.AssemblyDirectory, file);
            }
        }

        public HttpStatusCode StatusCode => RecvPacket.StatusCode;

        public string StringResult
        {
            get
            {
                RecvStream.Position = 0;
                using (StreamReader sr = new StreamReader(RecvStream))
                {
                    return sr.ReadToEnd().Replace("\r\n",", ");
                }
            }
        }
    }
}
