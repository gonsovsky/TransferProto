using System.IO;

namespace Atoll.TransferService
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
            
            stream.Seek(frame.ContentOffset, SeekOrigin.Begin);

            var buffer = frame.Buffer;
            var offset = frame.BufferOffset;
            var toRead = frame.Count;

            while (toRead > 0)
            {
                var read = stream.Read(buffer, offset, toRead);
                if (read < 1) break;

                offset += read;
                toRead -= read;
            }

            frame.BytesRead = frame.Count - toRead;
            return ctx.Ok();
        }

    }

}