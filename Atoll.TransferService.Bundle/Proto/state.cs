using System;
using Corallite.Buffers;

namespace Atoll.TransferService.Bundle.Proto
{
    public class State: IDisposable
    {
        public byte[] Buffer;

        public int BufferSize;

        public int BufferLen;

        public int BytesTransmitted;

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

        public virtual bool DataTransmitted(int len)
        {
            BytesTransmitted += len;
            return false;
        }
    }
}
