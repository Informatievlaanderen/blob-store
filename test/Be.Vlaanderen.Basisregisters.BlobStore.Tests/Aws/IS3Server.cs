namespace Be.Vlaanderen.Basisregisters.BlobStore.Aws
{
    using Amazon.S3;
    using Xunit;

    public interface IS3Server : IAsyncLifetime
    {
        AmazonS3Client CreateClient();
    }
}