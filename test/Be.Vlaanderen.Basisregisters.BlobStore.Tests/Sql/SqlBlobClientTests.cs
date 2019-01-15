namespace Be.Vlaanderen.Basisregisters.BlobStore.Sql
{
    using System;
    using System.Threading.Tasks;
    using Xunit;

    public class SqlBlobClientTests : BlobClientTests, IClassFixture<SqlServer>
    {
        private readonly SqlServer _server;

        public SqlBlobClientTests(SqlServer container)
        {
            _server = container ?? throw new ArgumentNullException(nameof(container));
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
