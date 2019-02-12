namespace Be.Vlaanderen.Basisregisters.BlobStore.Aws
{
    using System;
    using System.Threading.Tasks;
    using Amazon.S3;

    public class S3Server : IS3Server
    {
        private readonly IS3Server _inner;

        public S3Server()
        {
            if (Environment.GetEnvironmentVariable("CI") == null)
            {
                _inner = new S3ServerEmbeddedContainer();
            }
            else
            {
                _inner = new S3ServerComposedContainer();
            }
        }

        public Task InitializeAsync()
        {
            return _inner.InitializeAsync();
        }

        public Task DisposeAsync()
        {
            return _inner.DisposeAsync();
        }

        public AmazonS3Client CreateClient()
        {
            return _inner.CreateClient();
        }
    }
}