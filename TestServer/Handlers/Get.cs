using System;
using System.IO;
using Atoll.TransferService.Bundle.Server.Contract;
using Atoll.TransferService.Bundle.Server.Handler;
using TestContract;

namespace TestServer.Handlers
{
    public class Get : Custom
    {
        public override IContext Open(IContext ctx)
        {
            var request = ctx.Request;
            GetContract contract;
            try
            {
                contract = request.GetContract<GetContract>();
            }
            catch
            {
                return ctx.BadRequest("Failed to determine filename");
            }

            if (string.IsNullOrWhiteSpace(contract.Url)) return ctx.BadRequest("File name missing.");

            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, contract.Url);
            try
            {
                this.Stream = new FileStream(
                    filePath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read);
            }
            catch (FileNotFoundException)
            {
                return ctx.NotFound(string.Concat("File '", filePath, "' not found."));
            }
            catch (DirectoryNotFoundException)
            {
                return ctx.NotFound(string.Concat("File '", filePath, "' not found."));
            }
            catch (Exception)
            {
                return ctx.Error("Failed to open file");
            }

            return ctx.Ok();
        }

        public override IContext Read(IContext ctx)
        {
            ctx.ReadFromStream(this.Stream);
            return ctx;
        }

        public override IContext Write(IContext ctx)
        {
            throw new NotImplementedException();
        }

        public override Boolean DataSent(IContext ctx)
        {
            return Ready;
        }

        public override bool DataReceived(IContext ctx)
        {
            return true;
        }

    }
}
