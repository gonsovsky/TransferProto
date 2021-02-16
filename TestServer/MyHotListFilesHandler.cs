using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Atoll.TransferService;

namespace TestServer
{
    public class MyHotListFilesHandler : MyHotFileHandler, IHotGetHandler
    {
        private MemoryStream responseStream;

        public IHotGetHandlerContext Open(IHotGetHandlerContext ctx)
        {
            var request = ctx.Request;

            string baseDir;

            try
            {
                baseDir = Encoding.UTF8.GetString(request.Data, 0, request.DataLength);
                baseDir = AbsPath(baseDir);
                if (Directory.Exists(baseDir) == false)
                    throw new ApplicationException("Failed to change directory");
            }
            catch(Exception e)
            {
                return ctx.BadRequest(e.Message);
            }

            this.responseStream = new MemoryStream();
            using (var wrap = new NonClosableStreamWrap(this.responseStream))
            using (var writer = new StreamWriter(wrap, Encoding.UTF8))
            {
                foreach (var name in Directory.EnumerateFiles(baseDir, "*", SearchOption.AllDirectories))
                {
                    var rel = MyHotFileHandler.AbsoluteToRelativePath(name, baseDir);
                    writer.WriteLine(rel);
                }
                writer.Flush();
            }
            
            //необходимо для режима KeepAlive
            ctx.Frame.HaveToRead = (int)responseStream.Length;

            responseStream.Position = 0;
            return ctx.Ok();
        }

        public IHotGetHandlerContext Read(IHotGetHandlerContext ctx)
        {
            return ctx.ReadFromStream(this.responseStream);
        }

        public void Dispose()
        {
            try { this.responseStream.Dispose(); } catch { /* IGNORED */ }
        }
    }
}
