using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Corallite.Buffers;
using TestContract;

namespace Atoll.TransferService.Bundle.Proto
{
    public abstract class State: IDisposable
    {
        public byte[] Buffer;

        public int BufferSize;

        public int BufferLen;

        public int BytesTransmitted;

        public Packet Packet;

        public Socket Socket;

        protected bool HeadRecv;

        protected bool HeadSent;

        public virtual bool Send()
        {
            if (HeadSent)
                return false;
            HeadSent = true;
            Packet.ToByteArray(ref Buffer);
            BufferLen = Packet.MySize;
            return true;
        }

        public virtual bool HasSend()
        {
            return false;
        }

        public State(int bufferSize)
        {
            this.Buffer = UniArrayPool<byte>.Shared.Rent(bufferSize);
            this.BufferSize = bufferSize;
            this.BytesTransmitted = 0;
        }

        public void Dispose()
        {
            UniArrayPool<byte>.Shared.Return(this.Buffer);
        }

        public virtual void Close()
        {
            if (Socket != null)
            {
                Socket?.Shutdown(SocketShutdown.Both);
                Socket?.Close();
            }
        }

        public virtual bool DataTransmitted(int len)
        {
            BytesTransmitted += len;
            return false;
        }

        public abstract T Result<T>();

        public string Url => Packet.ToStruct<GetContract>().Url;

        public string FileName => Path.Combine(Helper.AssemblyDirectory, Url);

        public HttpStatusCode StatusCode => Packet.StatusCode;
    }
}
