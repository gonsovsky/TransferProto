using System.IO;

namespace Atoll.TransferService.Bundle.Server.Contract.Get
{
    /// <summary>
    /// Статический класс, содержащий метода расширения для <see cref="IHotGetHandlerContext"/>.
    /// </summary>
    public static class HotGetHandlerContextExtensions
    {
        /// <summary>
        /// Выполнить чтение данных из потока.
        /// </summary>
        /// <param name="ctx">контекст запроса получения данных.</param>
        /// <param name="stream">поток для чтения данных.</param>
        /// <returns>экземпляр контекста чтения данных.</returns>
        public static IHotGetHandlerContext ReadFromStream(this IHotGetHandlerContext ctx, Stream stream) 
        {
            var frame = ctx.Frame;
            stream.Seek(frame.BytesProcessed, SeekOrigin.Begin);
            var cnt = stream.Read(frame.Buffer, frame.BytesProcessed, frame.BufferSize);
            frame.DataArrived(cnt);
            return ctx.Ok();
        }
    }
}