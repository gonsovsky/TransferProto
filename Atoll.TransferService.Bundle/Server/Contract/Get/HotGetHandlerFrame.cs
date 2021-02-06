using System;
using Corallite.Buffers;

namespace Atoll.TransferService
{
    /// <summary>
    /// Описатель кадра ответа на запрос получения данных.
    /// </summary>
    public sealed class HotGetHandlerFrame : IDisposable
    {
        /// <summary>
        /// Буфер для заполнения.
        /// </summary>
        public byte[] Buffer;

        /// <summary>
        /// Отступ буфера <see cref="Buffer"/> для начала заполнения.
        /// </summary>
        public int BufferOffset;

        /// <summary>
        /// Отступ содержимого для заполнения буфера <see cref="Buffer"/>.
        /// </summary>
        public long ContentOffset;

        /// <summary>
        /// Кол-во содержимого для отправки <see cref="Buffer"/>.
        /// </summary>
        public long ContentLength;

        /// <summary>
        /// Количество данных для заполнения буфера <see cref="Buffer"/>.
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

        private readonly HotGetHandlerContext ctx;

        public HotGetHandlerFrame(HotGetHandlerContext ctx)
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