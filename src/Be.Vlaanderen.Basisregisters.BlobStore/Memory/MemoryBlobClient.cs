namespace Be.Vlaanderen.Basisregisters.BlobStore.Memory
{
    using System;
    using System.Collections.Concurrent;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    public class MemoryBlobClient : IBlobClient
    {
        private readonly ConcurrentDictionary<BlobName, BlobObject> _storage;

        public MemoryBlobClient()
        {
            _storage = new ConcurrentDictionary<BlobName, BlobObject>();
        }

        public Task<BlobObject> GetBlobAsync(BlobName name, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                throw new OperationCanceledException(cancellationToken);
            }

            return _storage.TryGetValue(name, out var result)
                ? Task.FromResult(result)
                : Task.FromResult<BlobObject>(null);
        }

        public Task PutBlobAsync(BlobName name, Metadata metadata, ContentType contentType, Stream content,
            CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                throw new OperationCanceledException(cancellationToken);
            }

            var stream = new MemoryStream();
            content.CopyTo(stream);
            if (!_storage.TryAdd(name, new BlobObject(name, metadata, contentType, ct => Task.FromResult<Stream>(new MemoryStream(stream.ToArray())))))
            {
                //TODO Exception
            }

            return Task.CompletedTask;
        }

        public Task DeleteBlobAsync(BlobName name, CancellationToken cancellationToken = default)
        {
            if (!_storage.TryRemove(name, out _))
            {
                //TODO Exception
            }

            return Task.CompletedTask;
        }
    }
}
