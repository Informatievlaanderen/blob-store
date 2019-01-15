namespace Be.Vlaanderen.Basisregisters.BlobStore.Sql
{
    using System.Data.SqlClient;
    using System.Threading.Tasks;
    using Xunit;

    public interface ISqlServerDatabase : IAsyncLifetime
    {
        //SqlConnectionStringBuilder CreateMasterConnectionStringBuilder();
        Task<SqlConnectionStringBuilder> CreateDatabase();
    }
}
