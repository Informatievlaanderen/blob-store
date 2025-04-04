namespace Be.Vlaanderen.Basisregisters.BlobStore.Memory
{
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

        public Task<BlobObject?> GetBlobAsync(BlobName name, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled<BlobObject?>(cancellationToken);
            }

            return _storage.TryGetValue(name, out var result)
                ? Task.FromResult(result)
                : Task.FromResult((BlobObject?)null);
        }

        public Task<bool> BlobExistsAsync(BlobName name, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled<bool>(cancellationToken);
            }

            return Task.FromResult(_storage.ContainsKey(name));
        }

        public async Task CreateBlobAsync(BlobName name, Metadata metadata, ContentType contentType, Stream content,
            CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                await Task.FromCanceled(cancellationToken);
            }

            async Task<byte[]> Copy(Stream input, CancellationToken ct)
            {
                using (var output = new MemoryStream())
                {
                    await input.CopyToAsync(output, 1024, ct);
                    return output.ToArray();
                }
            }

            var buffer = await Copy(content, cancellationToken);
            if (!_storage.TryAdd(name, new BlobObject(name, metadata, contentType,
                ct =>
                {
                    if (_storage.ContainsKey(name))
                    {
                        return Task.FromResult<Stream>(new ForwardOnlyStream(new MemoryStream(buffer, false)));
                    }

                    throw new BlobNotFoundException(name);
                })))
            {
                throw new BlobAlreadyExistsException(name);
            }
        }

        public Task DeleteBlobAsync(BlobName name, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled(cancellationToken);
            }

            _storage.TryRemove(name, out _);
            return Task.CompletedTask;
        }
    }
}
