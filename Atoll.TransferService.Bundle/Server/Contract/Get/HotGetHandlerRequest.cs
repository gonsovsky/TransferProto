using System.Text;
using Atoll.TransferService.Bundle.Proto;

namespace Atoll.TransferService.Bundle.Server.Contract.Get
{
    /// <summary>
    /// Описатель запроса получения данных.
    /// </summary> 
    public sealed class HotGetHandlerRequest: State
    {
        /// <summary>
        /// Маршрут запроса.
        /// </summary>
        public string Route;

        /// <summary>
        /// Длина данных запроса (заголовка) в массиве <see cref="Head"/>.
        /// </summary>
        public int HeadLength;

        /// <summary>
        /// Данные запроса  (заголовка).
        /// </summary>
        public byte[] Head;

        public T GetContract<T>()
        {
            var str = Encoding.UTF8.GetString(Head);
            var x = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(str);
            return x;
        }

        public override bool DataTransmitted(int cnt)
        {
            base.DataTransmitted(cnt);
            if (BytesTransmitted <= Packet.MinSize)
                return false;
            var packet = Packet.FromByteArray(Buffer);
            this.Route = packet.Route;
            this.HeadLength = packet.HeadLen;
            this.Head = packet.HeadData;
            return true;
        }

        public HotGetHandlerRequest(int bufferSize) : base(bufferSize)
        {
        }
    }
}