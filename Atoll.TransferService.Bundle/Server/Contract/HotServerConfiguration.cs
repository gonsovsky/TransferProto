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
        public int Port { get; set; }

        public int BufferSize { get; set; }

        public int Delay { get; set; }

        public int KeepAlive { get; set; }

        public int KeepAliveMsec => KeepAlive * 1000;

        public bool IsKeepAlive => KeepAlive > 0;
    }
}