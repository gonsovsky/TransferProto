using System;
using System.IO;

// ReSharper disable once CheckNamespace
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
            
            stream.Seek(frame.ContentOffset + frame.TotalWrite, SeekOrigin.Begin);
            var len = (int)Math.Min(frame.Count, frame.ContentLength - frame.TotalWrite);
            stream.Write(frame.Buffer, frame.BufferOffset, len);
            frame.BytesWrite = len;
            frame.TotalWrite += len;
            return ctx.Ok();
        }

    }

}