namespace Be.Vlaanderen.Basisregisters.BlobStore
{
    using System;
    using System.Threading.Tasks;
    using SqlClient;
    using Xunit;

    public class SqlBlobClientTests : BlobClientTests, IClassFixture<SqlServerContainer>
    {
        private readonly SqlServerContainer _container;

        public SqlBlobClientTests(SqlServerContainer container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
        }

        protected override async Task<IBlobClient> CreateClient()
        {
            var builder = await _container.CreateDatabase();

            await new SqlBlobSchema(builder)
                .CreateSchemaIfNotExists("blobs");

            return new SqlBlobClient(builder, "blobs");
        }
    }
}
