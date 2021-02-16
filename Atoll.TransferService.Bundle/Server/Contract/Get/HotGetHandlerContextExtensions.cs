using System;
using System.IO;
using System.Text;

// ReSharper disable once CheckNamespace
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
            ctx.Frame.BytesRead = 0;
            var frame = ctx.Frame;
            if (frame.ContentLength != 0 && frame.TotalRead >= frame.ContentLength)
            {
                frame.BytesRead = 0;
                return ctx.Ok();
            }
            stream.Seek(frame.ContentOffset + frame.TotalRead, SeekOrigin.Begin);
            var total = (int)Math.Min(frame.Count, frame.HaveToRead - frame.TotalRead);
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