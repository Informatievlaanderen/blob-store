namespace Be.Vlaanderen.Basisregisters.BlobStore.SqlClient
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    public class SqlBlobClient : IBlobClient
    {
        private const CommandBehavior ReaderBehavior =
            CommandBehavior.SingleRow | CommandBehavior.SequentialAccess | CommandBehavior.CloseConnection;

        private readonly SqlConnectionStringBuilder _builder;
        private readonly SqlCommandText _text;

        public SqlBlobClient(SqlConnectionStringBuilder builder, string schema)
        {
            _builder = builder ?? throw new ArgumentNullException(nameof(builder));
            _text = new SqlCommandText(schema);
        }

        public async Task<BlobObject> GetBlobAsync(BlobName name, CancellationToken cancellationToken = default)
        {
            var nameParameter = CreateSqlParameter(
                "@Name",
                SqlDbType.NVarChar,
                BlobName.MaxLength,
                name.ToString());

            using (var connection = new SqlConnection(_builder.ConnectionString))
            {
                await connection.OpenAsync(cancellationToken);
                using (var command = new SqlCommand(_text.GetBlob(), connection)
                {
                    CommandType = CommandType.Text,
                    Parameters = { nameParameter }
                })
                {
                    using (var reader = await command.ExecuteReaderAsync(ReaderBehavior, cancellationToken))
                    {
                        if (!reader.IsClosed && reader.Read())
                        {
                            return new BlobObject(
                                name,
                                MetadataFromString(reader.GetString(0)),
                                ContentType.Parse(reader.GetString(1)),
                                async contentCancellationToken =>
                                {
                                    var contentConnection = new SqlConnection(_builder.ConnectionString);
                                    await contentConnection.OpenAsync(contentCancellationToken);
                                    var contentCommand = new SqlCommand(_text.GetBlobContent(), contentConnection)
                                    {
                                        CommandType = CommandType.Text,
                                        Parameters = { nameParameter }
                                    };
                                    var contentReader = await contentCommand.ExecuteReaderAsync(ReaderBehavior, contentCancellationToken);
                                    if (!contentReader.IsClosed && contentReader.Read())
                                    {
                                        return new DisposableStream(
                                            contentReader.GetStream(0),
                                            contentReader,
                                            contentCommand,
                                            contentConnection);
                                    }
                                    return new MemoryStream();
                                });
                        }

                        return null;
                    }
                }
            }
        }

        public async Task PutBlobAsync(BlobName name, Metadata metadata, ContentType contentType, Stream content, CancellationToken cancellationToken = default)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));

            var nameParameter = CreateSqlParameter(
                "@Name",
                SqlDbType.NVarChar,
                BlobName.MaxLength,
                name.ToString());
            var metadataParameter = CreateSqlParameter(
                "@Metadata",
                SqlDbType.NVarChar,
                -1,
                MetadataToString(metadata));
            var contentTypeParameter = CreateSqlParameter(
                "@ContentType",
                SqlDbType.NVarChar,
                ContentType.MaxLength,
                contentType.ToString());
            var contentParameter = CreateSqlParameter(
                "@Content",
                SqlDbType.VarBinary,
                -1,
                content);
            using (var connection = new SqlConnection(_builder.ConnectionString))
            {
                await connection.OpenAsync(cancellationToken);
                using (var transaction = connection.BeginTransaction(IsolationLevel.Snapshot))
                {
                    using (var command = new SqlCommand(_text.PutBlob(), connection, transaction)
                    {
                        CommandType = CommandType.Text,
                        Parameters = {nameParameter, metadataParameter, contentTypeParameter, contentParameter}
                    })
                    {
                        await command.ExecuteNonQueryAsync(cancellationToken);
                    }
                    transaction.Commit();
                }
            }
        }

        public async Task DeleteBlobAsync(BlobName name, CancellationToken cancellationToken = default)
        {
            var nameParameter = CreateSqlParameter(
                "@Name",
                SqlDbType.NVarChar,
                BlobName.MaxLength,
                name.ToString());

            using (var connection = new SqlConnection(_builder.ConnectionString))
            {
                await connection.OpenAsync(cancellationToken);
                using (var transaction = connection.BeginTransaction(IsolationLevel.Snapshot))
                {
                    using (var command = new SqlCommand(_text.DeleteBlob(), connection, transaction)
                    {
                        CommandType = CommandType.Text,
                        Parameters = {nameParameter}
                    })
                    {
                        await command.ExecuteNonQueryAsync(cancellationToken);
                    }
                    transaction.Commit();
                }
            }
        }

        public async Task CreateSchemaIfNotExists(CancellationToken cancellationToken = default)
        {
            using (var connection = new SqlConnection(_builder.ConnectionString))
            {
                await connection.OpenAsync(cancellationToken);
                using (var command = new SqlCommand(_text.CreateSchemaIfNotExists(), connection)
                {
                    CommandType = CommandType.Text
                })
                {
                    await command.ExecuteNonQueryAsync(cancellationToken);
                }
            }
        }

        private static SqlParameter CreateSqlParameter(string name, SqlDbType sqlDbType, int size, object value)
        {
            return new SqlParameter(
                name,
                sqlDbType,
                size,
                ParameterDirection.Input,
                false,
                0,
                0,
                "",
                DataRowVersion.Default,
                value);
        }

        private static Metadata MetadataFromString(string value)
        {
            var metadata = Metadata.None;

            if (string.IsNullOrEmpty(value)) return metadata;

            using (var reader = new StringReader(value))
            {
                var line = reader.ReadLine();
                while (line != null)
                {
                    metadata = metadata.Add(MetadatumFromString(line));
                    line = reader.ReadLine();
                }
            }

            return metadata;
        }

        private static KeyValuePair<MetadataKey, string> MetadatumFromString(string value)
        {
            var separatorPosition = value.IndexOf('=');
            return new KeyValuePair<MetadataKey, string>(
                new MetadataKey(value.Substring(0, separatorPosition)),
                value.Substring(separatorPosition + 1));
        }

        private static string MetadataToString(Metadata metadata)
        {
            if(ReferenceEquals(metadata, Metadata.None)) return string.Empty;

            using (var writer = new StringWriter())
            {
                foreach (var metadatum in metadata)
                {
                    writer.WriteLine(MetadatumToString(metadatum));
                }

                return writer.ToString();
            }
        }

        private static string MetadatumToString(KeyValuePair<MetadataKey, string> metadatum)
        {
            return $"{metadatum.Key}={metadatum.Value}";
        }

        private class SqlCommandText
        {
            private readonly string _schema;

            public SqlCommandText(string schema)
            {
                _schema = schema ?? throw new ArgumentNullException(nameof(schema));
            }

            public string GetBlob() =>
                $"SELECT [Metadata], [ContentType] FROM [{_schema}].[Blob] WHERE [NameHash] = HashBytes('SHA2_256', @Name)";

            public string GetBlobContent() =>
                $"SELECT [Content] FROM [{_schema}].[Blob] WHERE [NameHash] = HashBytes('SHA2_256', @Name)";

            public string PutBlob() =>
                $"INSERT INTO [{_schema}].[Blob] ([NameHash], [Name], [Metadata], [ContentType], [Content]) VALUES (HashBytes('SHA2_256', @Name), @Name, @Metadata, @ContentType, @Content)";

            public string DeleteBlob() =>
                $"DELETE FROM [{_schema}].[Blob] WHERE [NameHash] = HashBytes('SHA2_256', @Name)";

            public string CreateSchemaIfNotExists() =>
                $@"IF NOT EXISTS (SELECT * FROM SYS.SCHEMAS WHERE [Name] = N'{_schema}')
BEGIN
    EXEC('CREATE SCHEMA {_schema} AUTHORIZATION [dbo]')
END

IF NOT EXISTS (SELECT * FROM SYS.SYSOBJECTS WHERE [Name] = 'Blob' AND [XType] = 'U' AND [Schema_ID] = (SELECT [Schema_ID] FROM SYS.SCHEMAS WHERE [Name] = N'{_schema}'))
BEGIN
    CREATE TABLE [{_schema}].[Blob]
    (
        [NameHash]            BINARY(32)         NOT NULL
        [Name]                NVARCHAR(512)      NOT NULL
        [Metadata]            NVARCHAR(MAX)      NOT NULL
        [ContentType]         NVARCHAR(129)      NOT NULL
        [Content]             VARBINARY(MAX)     NOT NULL
        CONSTRAINT PK_Blob    PRIMARY KEY        NONCLUSTERED (NameHash)
    )
END";
        }

        private class DisposableStream : Stream
        {
            private readonly Stream _inner;
            private readonly IDisposable[] _disposables;

            public DisposableStream(Stream inner, params IDisposable[] disposables)
            {
                _inner = inner ?? throw new ArgumentNullException(nameof(inner));
                _disposables = disposables ?? throw new ArgumentNullException(nameof(disposables));
            }

            public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
            {
                return _inner.BeginRead(buffer, offset, count, callback, state);
            }

            public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
            {
                return _inner.BeginWrite(buffer, offset, count, callback, state);
            }

            public override void Close()
            {
                _inner.Close();
            }

            public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
            {
                return _inner.CopyToAsync(destination, bufferSize, cancellationToken);
            }

            protected override void Dispose(bool disposing)
            {
                if (!disposing) return;

                _inner.Dispose();
                foreach (var disposable in _disposables)
                {
                    disposable.Dispose();
                }
            }

            public override int EndRead(IAsyncResult asyncResult)
            {
                return _inner.EndRead(asyncResult);
            }

            public override void EndWrite(IAsyncResult asyncResult)
            {
                _inner.EndWrite(asyncResult);
            }

            public override void Flush()
            {
                _inner.Flush();
            }

            public override Task FlushAsync(CancellationToken cancellationToken)
            {
                return _inner.FlushAsync(cancellationToken);
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                return _inner.Read(buffer, offset, count);
            }

            public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                return _inner.ReadAsync(buffer, offset, count, cancellationToken);
            }

            public override int ReadByte()
            {
                return _inner.ReadByte();
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                return _inner.Seek(offset, origin);
            }

            public override void SetLength(long value)
            {
                _inner.SetLength(value);
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                _inner.Write(buffer, offset, count);
            }

            public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                return _inner.WriteAsync(buffer, offset, count, cancellationToken);
            }

            public override void WriteByte(byte value)
            {
                _inner.WriteByte(value);
            }

            public override bool CanRead => _inner.CanRead;

            public override bool CanSeek => _inner.CanSeek;

            public override bool CanTimeout => _inner.CanTimeout;

            public override bool CanWrite => _inner.CanWrite;

            public override long Length => _inner.Length;

            public override long Position
            {
                get => _inner.Position;
                set => _inner.Position = value;
            }

            public override int ReadTimeout
            {
                get => _inner.ReadTimeout;
                set => _inner.ReadTimeout = value;
            }

            public override int WriteTimeout
            {
                get => _inner.WriteTimeout;
                set => _inner.WriteTimeout = value;
            }

            public override object InitializeLifetimeService()
            {
                return _inner.InitializeLifetimeService();
            }
        }
    }
}
