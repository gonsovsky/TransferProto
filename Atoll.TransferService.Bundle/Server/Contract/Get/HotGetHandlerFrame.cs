using System;
using Atoll.TransferService.Bundle.Server.Implementation;
using Corallite.Buffers;

namespace Atoll.TransferService.Bundle.Server.Contract.Get
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
        /// Количество данных для заполнения буфера <see cref="Buffer"/>.
        /// </summary>
        public int Count;

        /// <summary>
        /// Количество заполненных байт по результатам итерации чтения.
        /// </summary>
        public int BytesRead;

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