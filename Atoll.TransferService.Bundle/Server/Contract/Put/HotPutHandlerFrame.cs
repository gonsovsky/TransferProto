using System;
using Corallite.Buffers;

// ReSharper disable once CheckNamespace
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
        /// Количество заполненных байт по результатам итерации записи.
        /// </summary>
        public int BytesWrite;

        /// <summary>
        /// Общее Количество полученных байт сегмента.
        /// </summary>
        public int TotalWrite;

        /// <summary>
        /// Общее Количество полученных байт сегмента.
        /// </summary>
        public int BufferSize;

        private readonly HotPutHandlerContext ctx;

        public HotPutHandlerFrame(HotPutHandlerContext ctx)
        {
            this.ctx = ctx;
            //Смотри ctor HotPutHandlerContext. Здесь не создается буфер, а принимается из HotContext.Buffer
        }

        public void Dispose()
        {
            if (Buffer != null)
                UniArrayPool<byte>.Shared.Return(this.Buffer);
            Buffer = null;
        }
    }

}