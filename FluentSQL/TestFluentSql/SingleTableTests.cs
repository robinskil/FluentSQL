using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.SqlClient;
using TestFluentSql.Models;
using Xunit;

namespace TestFluentSql
{
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
            var students = driver.From<Student>().Select(a => new Student {Age = a.Age, FirstName = a.FirstName})
                .ToList();
            Assert.Equal(1000, students.Count);
            for (int i = 0; i < 1000; i++)
            {
                Assert.Null(students[i].UniversityName);
                Assert.Equal(0, students[i].StudentId);
            }
        }

        [Fact]
        public void Test_Select_DuplicatedColumnUse_ToList_SingleTable()
        {
            var driver = _sqlServerDatabase.GetFluentDriver();
            var students = driver.From<Student>()
                .Select(a => new Student {FirstName = a.FirstName, UniversityName = a.FirstName}).ToList();
            Assert.Equal(1000, students.Count);
            for (int i = 0; i < 1000; i++)
            {
                Assert.Equal(students[i].FirstName, students[i].UniversityName);
                Assert.Equal(0, students[i].StudentId);
                Assert.Equal(0, students[i].Age);
            }
        }

        [Fact]
        public void Test_Select_DuplicatedColumnUse_ToList_SingleTable_2()
        {
            var driver = _sqlServerDatabase.GetFluentDriver();
            var students =  driver.From<Student>().Select(a => 
                new { FirstName = a.UniversityName, a.UniversityName }).ToList();
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
            var students = driver.From<Student>().Select(a => new {Age = a.Age, a.FirstName}).ToList();
            Assert.Equal(1000, students.Count);
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
            var students = driver.From<Student>().ToList();
            Assert.Equal(1000, students.Count);
        }
    }
}