using System;
using System.IO;

namespace TestServer
{
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
