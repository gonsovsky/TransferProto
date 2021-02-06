using System;
using Corallite.Buffers;

namespace Atoll.TransferService
{
    /// <summary>
    /// Описатель ответа на запрос отправки данных.
    /// </summary>
    public sealed class HotPutHandlerFrame: IDisposable
    {
        /// <summary>
        /// Буфер, содержащий данные для приема.
        /// </summary>
        public byte[] Buffer;

        /// <summary>
        /// Отступ буфера <see cref="Buffer"/> до начала принятых данных.
        /// </summary>
        public int BufferOffset;

        /// <summary>
        /// Отступ отправленных данных относительно начала содержимого.
        /// </summary>
        public long ContentOffset;

        /// <summary>
        /// Длина отправленных данных относительно начала содержимого.
        /// </summary>
        public long ContentLength;

        /// <summary>
        /// Количество принятых данных.
        /// </summary>
        public int Count;

        /// <summary>
        /// Количество заполненных байт по результатам итерации чтения.
        /// </summary>
        public int BytesRead;

        /// <summary>
        /// Общее Количество отправленных байт сегмента.
        /// </summary>
        public int TotalRead;

        private readonly HotPutHandlerContext ctx;

        public HotPutHandlerFrame(HotPutHandlerContext ctx)
        {
            this.ctx = ctx;
            this.Buffer = UniArrayPool<byte>.Shared.Rent(ctx.Config.BufferSize);
        }

        public void Dispose()
        {
            if (Buffer != null)
                UniArrayPool<byte>.Shared.Return(this.Buffer);
            Buffer = null;
        }
    }

}