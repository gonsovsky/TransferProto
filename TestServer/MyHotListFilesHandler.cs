using System;
using System.IO;
using System.Net;
using System.Text;
using Atoll.TransferService.Bundle.Proto;
using Atoll.TransferService.Bundle.Server.Contract.Get;

namespace TestServer
{
    public class MyHotListFilesHandler : IHotGetHandler
    {
        private MemoryStream responseStream;

        public IHotGetHandlerContext Open(IHotGetHandlerContext ctx)
        {
            this.responseStream = new MemoryStream();
            var fs = new Fs(Helper.AssemblyDirectory);
            var data =fs.List("").ToJson();
            using (var wrap = new NonClosableStreamWrap(this.responseStream))
            using (var writer = new StreamWriter(wrap, Encoding.UTF8))
            {
                writer.Write(data);
                writer.Flush();
            }
            this.responseStream.Position = 0;
            return ctx.Ok();
        }

        public IHotGetHandlerContext Read(IHotGetHandlerContext ctx)
        {
            return ctx.ReadFromStream(this.responseStream);
        }

        public bool ReadEnd(IHotGetHandlerContext ctx)
        {
            return responseStream.Position == responseStream.Length;
        }

        public void Dispose()
        {
            try { this.responseStream.Dispose(); } catch { /* IGNORED */ }
        }

    }
}
