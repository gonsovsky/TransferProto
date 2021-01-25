namespace Atoll.TransferService
{

    /// <summary>
    /// Описатель кадра ответа на запрос получения данных.
    /// </summary>
    public sealed class HotGetHandlerFrame 
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

    }

}