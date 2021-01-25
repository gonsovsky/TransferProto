using System;
using System.IO;
using System.Text;
using Atoll.TransferService;

namespace TestServer
{

    class Program
    {

        static void Main(string[] args)
        {
            var routes = new HotServerRouteCollection()
                            .RouteGet("download", DefaultGetHandlerFactory<MyHotGetFileHandler>.Instance)
                            .RouteGet("list",     DefaultGetHandlerFactory<MyHotListFilesHandler>.Instance)
                            .RoutePut("upload",   DefaultPutHandlerFactory<MyHotPutFileHandler>.Instance);

            using (var server = new HotServer())
            {
                server
                    .UseRoutes(routes)
                    .UseConfig(new HotServerConfiguration { Port = 3000 });

                server.Start();
            
                Console.WriteLine("Server started. Press 'Enter' to stop.");
                Console.ReadLine();

                server.Stop();
            }
        }

    }

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
                    writer.WriteLine(name);
                }

                writer.Flush();
            }

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

    public class MyHotPutFileHandler : IHotPutHandler
    {

        private FileStream fileStream;

        public IHotPutHandlerContext Open(IHotPutHandlerContext ctx)
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
                    FileMode.OpenOrCreate,
                    FileAccess.Write,
                    FileShare.None);
            }
            catch (Exception)
            {
                return ctx.Error("Failed to open file");
            }

            return ctx.Ok();
        }

        public IHotPutHandlerContext Write(IHotPutHandlerContext ctx) => 
            ctx.WriteToStream(this.fileStream);

        public void Dispose()
        {
            try { this.fileStream.Close(); } catch { /* IGNORED */ }
            try { this.fileStream.Dispose(); } catch { /* IGNORED */ }
            this.fileStream = null;
        }

    }

    public class NonClosableStreamWrap : Stream
    {

        private readonly Stream inner;

        public NonClosableStreamWrap(Stream inner)
        {
            this.inner = inner ?? throw new ArgumentNullException(nameof(inner));
        }

        public override void Flush() => 
            this.inner.Flush();

        public override long Seek(long offset, SeekOrigin origin) => 
            this.inner.Seek(offset, origin);

        public override void SetLength(long value) => 
            this.inner.SetLength(value);

        public override int Read(byte[] buffer, int offset, int count) => 
            this.inner.Read(buffer, offset, count);

        public override void Write(byte[] buffer, int offset, int count) => 
            this.inner.Write(buffer, offset, count);

        public override bool CanRead => this.inner.CanRead;

        public override bool CanSeek => this.inner.CanSeek;

        public override bool CanWrite => this.inner.CanWrite;

        public override long Length => this.inner.Length;

        public override long Position
        {
            get => this.inner.Position;
            set => this.inner.Position = value;
        }

    }

}