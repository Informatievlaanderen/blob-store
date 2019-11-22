namespace Be.Vlaanderen.Basisregisters.BlobStore.Sql
{
    using System;
    using Microsoft.Data.SqlClient;
    using System.Threading;
    using System.Threading.Tasks;

    public class SqlServerComposedContainer : ISqlServerDatabase
    {
        private readonly SqlConnectionStringBuilder _builder;
        private int _db;

        public SqlServerComposedContainer()
        {
            if (Environment.GetEnvironmentVariable("SA_PASSWORD") == null)
            {
                throw new Exception("The SA_PASSWORD environment variable is missing.");
            }

            _builder =
                new SqlConnectionStringBuilder
                {
                    DataSource = $"tcp:localhost,1433",
                    InitialCatalog = "master",
                    UserID = "sa",
                    Password = Environment.GetEnvironmentVariable("SA_PASSWORD"),
                    Encrypt = false,
                    Enlist = false,
                    IntegratedSecurity = false
                };
        }

        public async Task InitializeAsync()
        {
            var builder = CreateMasterConnectionStringBuilder();

            async Task<TimeSpan> WaitUntilAvailable(int current)
            {
                if (current <= 30)
                {
                    try
                    {
                        using (var connection = new SqlConnection(builder.ConnectionString))
                        {
                            await connection.OpenAsync();
                            connection.Close();
                        }

                        return TimeSpan.Zero;
                    }
                    catch
                    {
                    }

                    return TimeSpan.FromSeconds(1);
                }

                throw new TimeoutException(
                    $"The sql server container did not become available in a timely fashion.");
            }

            var attempt = 0;
            var result = await WaitUntilAvailable(attempt++);
            while (result > TimeSpan.Zero)
            {
                await Task.Delay(result);
                result = await WaitUntilAvailable(attempt++);
            }
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        private SqlConnectionStringBuilder CreateMasterConnectionStringBuilder()
        {
            return new SqlConnectionStringBuilder(_builder.ConnectionString)
            {
                InitialCatalog = "master"
            };
        }

        private SqlConnectionStringBuilder CreateConnectionStringBuilder(string database) =>
            new SqlConnectionStringBuilder(_builder.ConnectionString)
            {
                InitialCatalog = database
            };

        public async Task<SqlConnectionStringBuilder> CreateDatabase()
        {
            var database = $"DB{Interlocked.Increment(ref _db)}";
            var text = $@"
CREATE DATABASE [{database}]
ALTER DATABASE [{database}] SET ALLOW_SNAPSHOT_ISOLATION ON
ALTER DATABASE [{database}] SET READ_COMMITTED_SNAPSHOT ON";
            using (var connection = new SqlConnection(CreateMasterConnectionStringBuilder().ConnectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(text, connection))
                {
                    await command.ExecuteNonQueryAsync();
                }
                connection.Close();
            }
            return CreateConnectionStringBuilder(database);
        }
    }
}
