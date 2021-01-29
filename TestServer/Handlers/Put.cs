using System;
using System.IO;
using Atoll.TransferService.Bundle.Server.Contract;
using Atoll.TransferService.Bundle.Server.Handler;

namespace TestServer.Handlers
{
    public class MyHotPutFileHandler : Custom
    {
        public override IContext Open(IContext ctx)
        {
        //    var request = ctx.Request;

        //    string fileName;

        //    try
        //    {
        //        fileName = Encoding.UTF8.GetString(request.Data, 0, request.DataLength);
        //    }
        //    catch
        //    {
        //        return ctx.BadRequest("Failed to determine filename");
        //    }

        //    if (string.IsNullOrWhiteSpace(fileName)) return ctx.BadRequest("File name missing.");

        //    var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
        //    try
        //    {
        //        this.fileStream = new FileStream(
        //            filePath,
        //            FileMode.OpenOrCreate,
        //            FileAccess.Write,
        //            FileShare.None);
        //    }
        //    catch (Exception)
        //    {
        //        return ctx.Error("Failed to open file");
        //    }

        //    return ctx.Ok();
        return ctx.Ok();
        }

        public override IContext Read(IContext ctx)
        {
            throw new NotImplementedException();
        }

        public override IContext Write(IContext ctx)
        {
            ctx.WriteToStream(this.Stream);
            return ctx;
        }

        public override bool DataSent(IContext ctx)
        {
            return true;
        }

        public override bool DataReceived(IContext ctx)
        {
            return Ready;
        }
    }

}
