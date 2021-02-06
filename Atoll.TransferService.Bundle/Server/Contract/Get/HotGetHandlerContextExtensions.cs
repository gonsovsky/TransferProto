using System;
using System.IO;
using System.Text;

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
            stream.Seek(frame.ContentOffset + frame.TotalRead, SeekOrigin.Begin);
            var len = frame.ContentLength;
            if (frame.ContentLength == 0)
                len = stream.Length - stream.Position;
            var total = (int)Math.Min(frame.Count, Math.Min(len, stream.Length - stream.Position));
            var read = stream.Read(frame.Buffer, frame.BufferOffset, total);
            frame.BytesRead = read;
            frame.TotalRead += read;
            return ctx.Ok();
        }

        public static string MakeString(this byte[] data, int len)
        {
            return Encoding.UTF8.GetString(data, 0, len);
        }
    }
}