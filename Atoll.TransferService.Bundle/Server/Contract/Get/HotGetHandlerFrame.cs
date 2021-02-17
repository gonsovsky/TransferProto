using System;
using Corallite.Buffers;

// ReSharper disable once CheckNamespace
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

        /// <summary>
        /// Общее Количество байт которые следует отправить клиенту. Это значение складывается из
        /// полученного аргумента ContentLength/ContentOffset и размера ресурса открытого обработчиком.
        /// Значение необходимо для режима KeepAlive, что бы клиент знал когда ему следует закончить команду
        /// </summary>
        public int HaveToRead;

        private readonly HotGetHandlerContext ctx;

        public HotGetHandlerFrame(HotGetHandlerContext ctx)
        {
            this.ctx = ctx;
            this.Buffer = UniArrayPool<byte>.Shared.Rent(ctx.Config.MinBufferSize);
        }

        public void Dispose()
        {
            if (Buffer != null)
                UniArrayPool<byte>.Shared.Return(this.Buffer);
            Buffer = null;
        }
    }
}