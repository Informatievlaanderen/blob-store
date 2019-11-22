namespace Be.Vlaanderen.Basisregisters.BlobStore.Sql
{
    using System;
    using Microsoft.Data.SqlClient;
    using System.Threading.Tasks;
    using Xunit;

    [Collection(nameof(SqlServerCollection))]
    public class SqlBlobSchemaTests
    {
        private readonly SqlServer _server;

        public SqlBlobSchemaTests(SqlServer server)
        {
            _server = server ?? throw new ArgumentNullException(nameof(server));
        }

        [Fact]
        public async Task CreateSchemaIfNotExistsWhenTheSchemaDoesNotExistHasExpectedResult()
        {
            //Arrange
            var schema = Guid.NewGuid().ToString("N");
            var builder = await _server.CreateDatabase();
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
            var builder = await _server.CreateDatabase();
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
