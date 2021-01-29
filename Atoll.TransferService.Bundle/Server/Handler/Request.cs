using System.Text;
using Atoll.TransferService.Bundle.Proto;

namespace Atoll.TransferService.Bundle.Server.Handler
{
    /// <summary>
    /// Описатель запроса получения данных.
    /// </summary> 
    public sealed class Request: State
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
            if (HeadRecv)
                return true;
            if (BytesTransmitted <= Packet.MinSize)
                return false;
            HeadRecv = true;
            Packet = Packet.FromByteArray(Buffer);
            this.Route = Packet.Route;
            this.HeadLength = Packet.HeadLen;
            this.Head = Packet.HeadData;
            return true;
        }

        public override T Result<T>()
        {
            throw new System.NotImplementedException();
        }

        public Request(int bufferSize) : base(bufferSize)
        {
        }
    }
}