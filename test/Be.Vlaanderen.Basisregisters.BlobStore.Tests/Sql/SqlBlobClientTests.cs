namespace Be.Vlaanderen.Basisregisters.BlobStore.Sql
{
    using System;
    using System.Threading.Tasks;
    using Xunit;

    [Collection(nameof(SqlServerCollection))]
    public class SqlBlobClientTests : BlobClientTests
    {
        private readonly SqlServer _server;

        public SqlBlobClientTests(SqlServer server)
        {
            _server = server ?? throw new ArgumentNullException(nameof(server));
        }

        protected override async Task<IBlobClient> CreateClient()
        {
            var builder = await _server.CreateDatabase();

            await new SqlBlobSchema(builder)
                .CreateSchemaIfNotExists("blobs");

            return new SqlBlobClient(builder, "blobs");
        }
    }
}
