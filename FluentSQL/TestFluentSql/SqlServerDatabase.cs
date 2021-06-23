using System;
using System.IO;
using System.Reflection;
using DbUp;
using FluentSQL;
using TestFluentSql.Models;

namespace TestFluentSql
{
    public class SqlServerDatabase : IDisposable
    {
        private Driver _currentDriver;

        private const string ConnectionString =
            "Server=127.0.0.1;Database=FluentSqlUniversity;User Id=SA;Password=Password123";

        private const string MasterConnectionString =
            "Server=127.0.0.1;Database=master;User Id=SA;Password=Password123";

        public SqlServerDatabase()
        {
            var location = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                @"DatabaseScripts/SqlServer/");
            EnsureDatabase.For.SqlDatabase(ConnectionString);

            var upgrader = DeployChanges.To.SqlDatabase(ConnectionString)
                .WithScriptsFromFileSystem(location)
                .LogToAutodetectedLog()
                .Build()
                .PerformUpgrade();
            if (!upgrader.Successful) throw new Exception("Couldn't create test database.", upgrader.Error);
        }

        private SqlServerProvider SqlServerProvider()
        {
            return new SqlServerProvider(ConnectionString);
        }

        public Driver GetFluentDriver()
        {
            if (_currentDriver == null)
            {
                _currentDriver = new Driver(
                    new FluentSqlOptions()
                        .SelectProvider(SqlServerProvider())
                        .SetPrimaryKey<Student>(s => new {s.StudentId})
                );
            }

            return _currentDriver;
        }

        public void Dispose()
        {
            DropDatabase.For.SqlDatabase(ConnectionString);
        }
    }
}