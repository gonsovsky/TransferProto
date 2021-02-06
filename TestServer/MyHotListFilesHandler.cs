using System;
using System.IO;
using System.Text;
using Atoll.TransferService;

namespace TestServer
{
    public class MyHotListFilesHandler : IHotGetHandler
    {
        private MemoryStream responseStream;

        public IHotGetHandlerContext Open(IHotGetHandlerContext ctx)
        {
            this.responseStream = new MemoryStream();
            using (var wrap = new NonClosableStreamWrap(this.responseStream))
            using (var writer = new StreamWriter(wrap, Encoding.UTF8))
            {
                foreach (var name in Directory.EnumerateFiles(AppDomain.CurrentDomain.BaseDirectory, "*"))
                {
                    writer.WriteLine(System.IO.Path.GetFileName(name));
                }
                writer.Flush();
            }

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
