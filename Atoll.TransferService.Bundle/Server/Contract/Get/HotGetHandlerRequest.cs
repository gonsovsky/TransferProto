namespace Atoll.TransferService
{

    /// <summary>
    /// Описатель запроса получения данных.
    /// </summary>
    public sealed class HotGetHandlerRequest 
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

    }

}