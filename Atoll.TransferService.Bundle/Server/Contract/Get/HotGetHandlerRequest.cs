using Corallite.Buffers;

namespace Atoll.TransferService.Bundle.Server.Contract.Get
{
    /// <summary>
    /// Описатель запроса получения данных.
    /// </summary>
    public sealed class HotGetHandlerRequest: HotGetHandlerSomeone
    {
        /// <summary>
        /// Маршрут запроса.
        /// </summary>
        public readonly string Route;

        /// <summary>
        /// Данные запроса.
        /// </summary>
        public readonly byte[] Data;

        /// <summary>
        /// Длина данных запроса в массиве <see cref="Data"/>.
        /// </summary>
        public readonly int DataLength;

        /// <summary>
        /// Количество заполненных байт по результатам итерации чтения.
        /// </summary>
        public int BytesRead;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="route">маршрут запроса.</param>
        /// <param name="data">данные запроса.</param>
        /// <param name="dataLength">длина данных запроса в массиве <paramref name="data"/>.</param>
        public HotGetHandlerRequest(string route, byte[] data, int dataLength) 
        {
            this.Route = route;
            this.Data = data;
            this.DataLength = dataLength;
        }

        public HotGetHandlerRequest(int dataLength)
        {
            this.Data = UniArrayPool<byte>.Shared.Rent(dataLength);
            this.DataLength = dataLength;
        }

        public bool DataArrived(int cnt)
        {
            BytesRead += cnt;
            return false;
        }

        public void DataRelease()
        {
            UniArrayPool<byte>.Shared.Return(this.Data);
        }
    }
}