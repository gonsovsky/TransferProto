namespace Atoll.TransferService.Bundle.Server.Contract
{
    /// <summary>
    /// Описатель конфигурации сервера.
    /// </summary>
    public sealed class HotServerConfiguration
    {
        /// <summary>
        /// Порт сервера.
        /// </summary>
        public int Port;

        /// <summary>
        /// Максимальный размер буфера приема/отправки.
        /// </summary>
        public int BufferSize = 1024;
    }
}