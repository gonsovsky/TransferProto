using System.IO;
using System.Text;
using Atoll.TransferService.Bundle.Proto;
using Atoll.TransferService.Bundle.Server.Contract;
using Atoll.TransferService.Bundle.Server.Handler;

namespace TestServer.Handlers
{
    public class MyHotListFilesHandler : Custom
    {
        public override IContext Open(IContext ctx)
        {
            this.Stream = new MemoryStream();
            var fs = new Fs(Atoll.TransferService.Bundle.Proto.Helper.AssemblyDirectory);
            var data =fs.List("").ToJson();
            using (var wrap = new ImmortalStream(this.Stream))
            using (var writer = new StreamWriter(wrap, Encoding.UTF8))
            {
                writer.Write(data);
                writer.Flush();
            }
            this.Stream.Position = 0;
            return ctx.Ok();
        }
        
        public override IContext Read(IContext ctx)
        {
            ctx.ReadFromStream(this.Stream);
            return ctx;
        }

        public override IContext Write(IContext ctx)
        {
            throw new System.NotImplementedException();
        }

        public override bool DataSent(IContext ctx)
        {
            return Ready;
        }

        public override bool DataReceived(IContext ctx)
        {
            return true;
        }
    }
}
