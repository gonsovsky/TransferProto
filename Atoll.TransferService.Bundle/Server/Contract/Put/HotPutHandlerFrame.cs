namespace Atoll.TransferService.Bundle.Server.Contract.Put
{
    /// <summary>
    /// Описатель ответа на запрос отправки данных.
    /// </summary>
    public sealed class HotPutHandlerFrame 
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
        /// Отступ принятых данных относительно начала содержимого.
        /// </summary>
        public long ContentOffset;

        /// <summary>
        /// Количество принятых данных.
        /// </summary>
        public int Count;
    }
}