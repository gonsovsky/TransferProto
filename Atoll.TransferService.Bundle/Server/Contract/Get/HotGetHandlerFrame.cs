using Atoll.TransferService.Bundle.Proto;

namespace Atoll.TransferService.Bundle.Server.Contract.Get
{
    /// <summary>
    /// Описатель кадра ответа на запрос получения данных.
    /// </summary>
    public sealed class HotGetHandlerFrame: State 
    {
        public HotGetHandlerFrame(int bufferSize) : base(bufferSize)
        {
        }

        public override bool DataTransmitted(int len)
        {
            BufferLen = len;
            base.DataTransmitted(len);
            return true;
        }

        public override T Result<T>()
        {
            throw new System.NotImplementedException();
        }
    }
}