namespace Be.Vlaanderen.Basisregisters.BlobStore.Sql
{
    using Xunit;

    [CollectionDefinition(nameof(SqlServerCollection))]
    public class SqlServerCollection : ICollectionFixture<SqlServer>
    {
    }
}