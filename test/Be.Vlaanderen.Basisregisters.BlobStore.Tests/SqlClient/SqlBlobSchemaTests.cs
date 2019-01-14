namespace Be.Vlaanderen.Basisregisters.BlobStore
{
    using System;
    using System.Data.SqlClient;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;
    using SqlClient;
    using Xunit;

    public class SqlBlobSchemaTests : IClassFixture<SqlServerContainer>
    {
        private readonly SqlServerContainer _fixture;

        public SqlBlobSchemaTests(SqlServerContainer fixture)
        {
            _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
        }

        [Fact]
        public async Task CreateSchemaIfNotExistsWhenTheSchemaDoesNotExistHasExpectedResult()
        {
            //Arrange
            var schema = Guid.NewGuid().ToString("N");
            var builder = await _fixture.CreateDatabase();
            var sut = new SqlBlobSchema(builder);

            //Act
            await sut.CreateSchemaIfNotExists(schema);

            //Assert
            using (var connection = new SqlConnection(builder.ConnectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand($"SELECT COUNT(*) FROM SYS.SCHEMAS WHERE [Name] = '{schema}'", connection))
                {
                    Assert.Equal(1, await command.ExecuteScalarAsync());
                }

                using (var command = new SqlCommand($"SELECT COUNT(*) FROM SYS.OBJECTS WHERE [Name] = 'Blob' AND [Type] = 'U' AND SCHEMA_ID = SCHEMA_ID('{schema}')", connection))
                {
                    Assert.Equal(1, await command.ExecuteScalarAsync());
                }

                connection.Close();
            }
        }

        [Fact]
        public async Task CreateSchemaIfNotExistsWhenTheSchemaExistHasExpectedResult()
        {
            //Arrange
            var schema = Guid.NewGuid().ToString("N");
            var builder = await _fixture.CreateDatabase();
            var sut = new SqlBlobSchema(builder);
            await sut.CreateSchemaIfNotExists(schema);

            //Act
            await sut.CreateSchemaIfNotExists(schema);

            //Assert
            using (var connection = new SqlConnection(builder.ConnectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand($"SELECT COUNT(*) FROM SYS.SCHEMAS WHERE [Name] = '{schema}'", connection))
                {
                    Assert.Equal(1, await command.ExecuteScalarAsync());
                }

                using (var command = new SqlCommand($"SELECT COUNT(*) FROM SYS.OBJECTS WHERE [Name] = 'Blob' AND [Type] = 'U' AND SCHEMA_ID = SCHEMA_ID('{schema}')", connection))
                {
                    Assert.Equal(1, await command.ExecuteScalarAsync());
                }

                connection.Close();
            }
        }
    }
}
