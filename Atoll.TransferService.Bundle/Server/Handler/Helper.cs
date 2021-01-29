using System;
using System.IO;
using Atoll.TransferService.Bundle.Server.Contract;

namespace Atoll.TransferService.Bundle.Server.Handler
{
    /// <summary>
    /// Статический класс, содержащий метода расширения для <see cref="IContext"/>.
    /// </summary>
    public static class Helper
    {
        /// <summary>
        /// Выполнить чтение данных из потока.
        /// </summary>
        /// <param name="ctx">контекст запроса получения данных.</param>
        /// <param name="stream">поток для чтения данных.</param>
        /// <returns>экземпляр контекста чтения данных.</returns>
        public static IContext ReadFromStream(this IContext ctx, Stream stream) 
        {
            var frame = ctx.Frame;
            stream.Seek(frame.BytesTransmitted, SeekOrigin.Begin);
            var len = (int)Math.Min(frame.BufferSize, (stream.Length - stream.Position));
            var cnt = stream.Read(frame.Buffer, 0, len);
            frame.DataTransmitted(cnt);
            return ctx.Ok();
        }

        public static IContext WriteToStream(this IContext ctx, Stream stream)
        {
            var frame = ctx.Frame;

           // stream.Seek(frame.ContentOffset, SeekOrigin.Begin);
            //stream.Write(frame.Buffer, frame.BufferOffset, frame.Count);
            return null;
            //return ctx.Ok();
        }
    }
}