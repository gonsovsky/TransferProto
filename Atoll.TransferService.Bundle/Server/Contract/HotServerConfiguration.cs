// ReSharper disable once CheckNamespace

using System;

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

        /// <summary>
        /// Размер буфера канала получения данных. 
        /// </summary>
        public int RecvSize { get; set; }

        /// <summary>
        /// Размер буфера канала отправки данных. 
        /// </summary>
        public int SendSize { get; set; }

        /// <summary>
        /// Минимальный размер буфера для хранения данных контекста. Будет расти до MaxBufferSize
        /// </summary>
        public int MinBufferSize { get; set; }

        /// <summary>
        /// Максимальный размер буфера для хранения данных контекста. В случае превышения возвращается HTTP.RequestUriTooLong
        /// </summary>
        public int MaxBufferSize { get; set; }

        /// <summary>
        /// Задержка между итерациями при отправки данных
        /// </summary>
        public int SendDelay { get; set; }

        /// <summary>
        /// Повторное использование соединений
        /// </summary>
        public int KeepAlive { get; set; }

        public int KeepAliveMsec => KeepAlive * 1000;

        public bool IsKeepAlive => KeepAlive > 0;

        public void Validate()
        {
            if (MaxBufferSize < MinBufferSize)
                throw new ArgumentOutOfRangeException();

            if (RecvSize > MinBufferSize)
                throw new ArgumentOutOfRangeException();

            if (SendSize > MinBufferSize)
                throw new ArgumentOutOfRangeException();
        }
    }
}