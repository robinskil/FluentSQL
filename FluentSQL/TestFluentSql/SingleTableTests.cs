using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using DbUp;
using FluentSQL;
using Microsoft.Data.SqlClient;
using TestFluentSql.Models;
using Xunit;

namespace TestFluentSql
{
    [CollectionDefinition("SqlServer")]
    public class SqlServerDatabaseCollection : ICollectionFixture<SqlServerDatabase>
    {
        
    }
    public class SqlServerDatabase : IDisposable
    {
        private Driver _currentDriver;
        private const string ConnectionString = "Server=127.0.0.1;Database=FluentSqlUniversity;User Id=SA;Password=Password123";
        private const string MasterConnectionString = "Server=127.0.0.1;Database=master;User Id=SA;Password=Password123";

        public SqlServerDatabase()
        {
            var location = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"DatabaseScripts/SqlServer/");
            EnsureDatabase.For.SqlDatabase(ConnectionString);
            
            var upgrader = DeployChanges.To.SqlDatabase(ConnectionString)
                .WithScriptsFromFileSystem(location)
                .LogToAutodetectedLog()
                .Build()
                .PerformUpgrade();
            if (!upgrader.Successful) throw new Exception("Couldn't create test database.",upgrader.Error);
        }

        private SqlServerProvider SqlServerProvider()
        {
            return new SqlServerProvider(ConnectionString);
        }

        public Driver GetFluentDriver()
        {
            if (_currentDriver == null)
            {
                _currentDriver = new Driver(SqlServerProvider());
            }
            return _currentDriver;
        }

        public void Dispose()
        {
            DropDatabase.For.SqlDatabase(ConnectionString);
        }
    }

    [Collection("SqlServer")]
    public class SingleTableSelectTests
    {
        private readonly SqlServerDatabase _sqlServerDatabase;

        public SingleTableSelectTests(SqlServerDatabase sqlServerDatabase)
        {
            _sqlServerDatabase = sqlServerDatabase;
        }
        
        [Fact]
        public void Test_Select_ToList_SingleTable()
        {
            var driver = _sqlServerDatabase.GetFluentDriver();
            var students =  driver.From<Student>().Select(a => new Student{Age = a.Age,FirstName = a.FirstName}).ToList();
            Assert.Equal(1000,students.Count);
            for (int i = 0; i < 1000; i++)
            {
                Assert.Null(students[i].UniversityName);
                Assert.Equal(0,students[i].StudentId);
            }
        }
        
        [Fact]
        public void Test_Select_DuplicatedColumnUse_ToList_SingleTable()
        {
            var driver = _sqlServerDatabase.GetFluentDriver();
            var students =  driver.From<Student>().Select(a => new Student{FirstName = a.FirstName, UniversityName = a.FirstName}).ToList();
            Assert.Equal(1000,students.Count);
            for (int i = 0; i < 1000; i++)
            {
                Assert.Equal(students[i].FirstName,students[i].UniversityName);
                Assert.Equal(0,students[i].StudentId);
                Assert.Equal(0,students[i].Age);
            }
        }
        
        [Fact]
        public void Test_Select_DuplicatedColumnUse_ToList_SingleTable_2()
        {
            var driver = _sqlServerDatabase.GetFluentDriver();
            var students =  driver.From<Student>().Select(a => 
                new {FirstName = a.FirstName, UniversityName = a.FirstName, StudentCourses = a.StudentCourses.Select(a => a).ToList() }).ToList();
            Assert.Equal(1000,students.Count);
            for (int i = 0; i < 1000; i++)
            {
                Assert.Equal(students[i].FirstName,students[i].UniversityName);
            }
        }
        
        [Fact]
        public void Test_Select_AnonymousObject_ToList_SingleTable()
        {
            var driver = _sqlServerDatabase.GetFluentDriver();
            var students =  driver.From<Student>().Select(a => new {Age = a.Age,FirstName = a.FirstName}).ToList();
            Assert.Equal(1000,students.Count);
            for (int i = 0; i < 1000; i++)
            {
                Assert.True(students[i].Age != default);
                Assert.False(string.IsNullOrEmpty(students[i].FirstName));
            }
        }
    }
    
    [Collection("SqlServer")]
    public class SingleTableTests
    {
        private readonly SqlServerDatabase _sqlServerDatabase;

        public SingleTableTests(SqlServerDatabase sqlServerDatabase)
        {
            _sqlServerDatabase = sqlServerDatabase;
        }
        
        [Fact]
        public void Test_ToList_SingleTable()
        {
            var driver = _sqlServerDatabase.GetFluentDriver();
            var students =  driver.From<Student>().ToList();
            Assert.Equal(1000,students.Count);
        }
        

    }
}