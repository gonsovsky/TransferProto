using System.IO;

namespace Atoll.TransferService
{

    /// <summary>
    /// Статический класс, содержащий метода расширения для <see cref="IHotPutHandlerContext"/>.
    /// </summary>
    public static class HotPutHandlerContextExtensions
    {

        /// <summary>
        /// Выполнить чтение данных из потока.
        /// </summary>
        /// <param name="ctx">контекст запроса получения данных.</param>
        /// <param name="stream">поток для чтения данных.</param>
        /// <returns>экземпляр контекста чтения данных.</returns>
        public static IHotPutHandlerContext WriteToStream(this IHotPutHandlerContext ctx, Stream stream) 
        {
            var frame = ctx.Frame;
            
            stream.Seek(frame.ContentOffset, SeekOrigin.Begin);
            stream.Write(frame.Buffer, frame.BufferOffset, frame.Count);

            return ctx.Ok();
        }

    }

}