namespace Be.Vlaanderen.Basisregisters.BlobStore
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    public class BlobObject
    {
        private readonly Func<CancellationToken, Task<Stream>> _openContent;

        public BlobObject(BlobName name, Metadata metadata, ContentType contentType, Func<CancellationToken, Task<Stream>> openContent)
        {
            Name = name;
            ContentType = contentType;
            Metadata = metadata;
            _openContent = openContent ?? throw new ArgumentNullException(nameof(openContent));
        }
        public BlobName Name { get; }
        public ContentType ContentType { get; }
        public Metadata Metadata { get; }

        public Task<Stream> OpenAsync(CancellationToken cancellationToken = default)
        {
            return _openContent(cancellationToken);
        }
    }
}
