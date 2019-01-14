namespace Be.Vlaanderen.Basisregisters.BlobStore.Memory
{
    using System.Threading.Tasks;

    public class MemoryBlobClientTests : BlobClientTests
    {
        protected override Task<IBlobClient> CreateClient()
        {
            return Task.FromResult((IBlobClient)new MemoryBlobClient());
        }
    }
}
