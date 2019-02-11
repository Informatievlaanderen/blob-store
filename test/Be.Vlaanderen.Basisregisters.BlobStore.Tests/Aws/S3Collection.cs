namespace Be.Vlaanderen.Basisregisters.BlobStore.Aws
{
    using Xunit;

    [CollectionDefinition(nameof(S3Collection))]
    public class S3Collection : ICollectionFixture<S3Server>
    {
    }
}