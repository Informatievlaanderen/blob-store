using System.IO;
using System.Threading.Tasks;

namespace Be.Vlaanderen.Basisregisters.BlobStore
{
    using System.Threading;

    public interface IBlobClient
    {
        Task<BlobObject?> GetBlobAsync(BlobName name, CancellationToken cancellationToken = default);
        Task<bool> BlobExistsAsync(BlobName name, CancellationToken cancellationToken = default);
        Task CreateBlobAsync(BlobName name, Metadata metadata, ContentType contentType, Stream content, CancellationToken cancellationToken = default);
        Task DeleteBlobAsync(BlobName name, CancellationToken cancellationToken = default);
    }
}
