// ReSharper disable once CheckNamespace
namespace Atoll.TransferService
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

        public int BufferSize;

        public int Delay;
    }
}