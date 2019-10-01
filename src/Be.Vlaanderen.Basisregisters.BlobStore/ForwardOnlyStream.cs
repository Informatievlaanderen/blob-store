namespace Be.Vlaanderen.Basisregisters.BlobStore
{
    using System;
    using System.IO;

    //origin: https://github.com/adamhathcock/sharpcompress

    internal class ForwardOnlyStream : Stream
    {
        private readonly Stream _stream;
        private bool _disposed;

        public ForwardOnlyStream(Stream stream)
        {
            _stream = stream;
            _disposed = false;
        }

        protected override void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (!disposing) return;

            _stream.Dispose();
            _disposed = true;
            base.Dispose(true);
        }

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _stream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
    }
}
