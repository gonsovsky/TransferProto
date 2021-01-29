using System;
using System.IO;
using Atoll.TransferService.Bundle.Server.Contract;
using Atoll.TransferService.Bundle.Server.Handler;
using TestContract;

namespace TestServer
{
    public class MyHotGetFileHandler : IHandler
    {
        private FileStream fileStream;

        public IHotGetHandlerContext Open(IHotGetHandlerContext ctx)
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
                this.fileStream = new FileStream(
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

        public IHotGetHandlerContext Read(IHotGetHandlerContext ctx) =>
            ctx.ReadFromStream(this.fileStream);

        public Boolean ReadEnd(IHotGetHandlerContext ctx)
        {
            return fileStream.Position == fileStream.Length;
        }

        public void Dispose()
        {
            try { this.fileStream.Close(); } catch { /* IGNORED */ }
            try { this.fileStream.Dispose(); } catch { /* IGNORED */ }
            this.fileStream = null;
        }
    }
}
