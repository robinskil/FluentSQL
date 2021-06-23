using Xunit;

namespace TestFluentSql
{
    [CollectionDefinition("SqlServer")]
    public class SqlServerDatabaseCollection : ICollectionFixture<SqlServerDatabase>
    {
    }
}