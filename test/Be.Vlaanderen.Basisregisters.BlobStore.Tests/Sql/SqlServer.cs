namespace Be.Vlaanderen.Basisregisters.BlobStore.Sql
{
    using System;
    using System.Data.SqlClient;
    using System.Threading.Tasks;

    public class SqlServer : ISqlServerDatabase
    {
        private readonly ISqlServerDatabase _inner;

        public SqlServer()
        {
#if DEBUG
            if (Environment.GetEnvironmentVariable("CI") == null)
            {
                _inner = new SqlServerEmbeddedContainer();
            }
            else
            {
                _inner = new SqlServerComposedContainer();
            }
#else
            _inner = new SqlServerComposedContainer();
#endif
        }

        public Task InitializeAsync()
        {
            return _inner.InitializeAsync();
        }

        public Task DisposeAsync()
        {
            return _inner.DisposeAsync();
        }

        public Task<SqlConnectionStringBuilder> CreateDatabase()
        {
            return _inner.CreateDatabase();
        }
    }
}
