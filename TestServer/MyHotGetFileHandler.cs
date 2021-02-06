using System;
using System.IO;
using System.Text;
using Atoll.TransferService;

namespace TestServer
{
    public class MyHotGetFileHandler : IHotGetHandler
    {

        private FileStream fileStream;

        public IHotGetHandlerContext Open(IHotGetHandlerContext ctx)
        {
            var request = ctx.Request;

            string fileName;

            try
            {
                fileName = Encoding.UTF8.GetString(request.Data, 0, request.DataLength);
            }
            catch
            {
                return ctx.BadRequest("Failed to determine filename");
            }

            if (string.IsNullOrWhiteSpace(fileName)) return ctx.BadRequest("File name missing.");

            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
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

        public void Dispose()
        {
            try { this.fileStream.Close(); } catch { /* IGNORED */ }
            try { this.fileStream.Dispose(); } catch { /* IGNORED */ }
            this.fileStream = null;
        }

    }
}
