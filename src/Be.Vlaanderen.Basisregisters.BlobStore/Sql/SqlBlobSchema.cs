namespace Be.Vlaanderen.Basisregisters.BlobStore.Sql
{
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using System.Threading;
    using System.Threading.Tasks;

    public class SqlBlobSchema
    {
        private readonly SqlConnectionStringBuilder _builder;

        public SqlBlobSchema(SqlConnectionStringBuilder builder)
        {
            _builder = builder ?? throw new ArgumentNullException(nameof(builder));
        }

        public async Task CreateSchemaIfNotExists(string schema, CancellationToken cancellationToken = default)
        {
            var text = $@"
                IF NOT EXISTS (SELECT * FROM SYS.SCHEMAS WHERE [Name] = '{schema}')
                BEGIN
                    EXEC('CREATE SCHEMA [{schema}] AUTHORIZATION [dbo]')
                END

                IF NOT EXISTS (SELECT * FROM SYS.OBJECTS WHERE [Name] = 'Blob' AND [Type] = 'U' AND [Schema_ID] = SCHEMA_ID('{schema}'))
                BEGIN
                    CREATE TABLE [{schema}].[Blob]
                    (
                        [NameHash]            BINARY(32)         NOT NULL,
                        [Name]                NVARCHAR(512)      NOT NULL,
                        [Metadata]            NVARCHAR(MAX)      NOT NULL,
                        [ContentType]         NVARCHAR(129)      NOT NULL,
                        [Content]             VARBINARY(MAX)     NOT NULL,
                        CONSTRAINT PK_Blob    PRIMARY KEY        NONCLUSTERED (NameHash)
                    )
                END";
            using (var connection = new SqlConnection(_builder.ConnectionString))
            {
                await connection.OpenAsync(cancellationToken);
                using (var command = new SqlCommand(
                    text, connection)
                {
                    CommandType = CommandType.Text
                })
                {
                    await command.ExecuteNonQueryAsync(cancellationToken);
                }
            }
        }
    }
}