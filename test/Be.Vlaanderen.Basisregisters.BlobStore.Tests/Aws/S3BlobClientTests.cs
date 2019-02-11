namespace Be.Vlaanderen.Basisregisters.BlobStore.Aws
{
    using System.Collections.ObjectModel;
    using System.Threading;
    using System.Threading.Tasks;
    using Amazon;
    using Amazon.S3.Model;
    using Sql;
    using Xunit;

    [Collection(nameof(S3Collection))]
    public class S3BlobClientTests : BlobClientTests
    {
        private readonly S3Server _server;

        private static int _bucket;

        public S3BlobClientTests(S3Server server)
        {
            _server = server;
        }

        protected override async Task<IBlobClient> CreateClient()
        {
            var client = _server.CreateClient();
            var bucketName = $"bucket{Interlocked.Increment(ref _bucket)}";
            await client.PutBucketAsync(new PutBucketRequest
                {
                    UseClientRegion = true,
                    BucketName = bucketName
                });
            return new S3BlobClient(client, bucketName);
        }
    }
}
