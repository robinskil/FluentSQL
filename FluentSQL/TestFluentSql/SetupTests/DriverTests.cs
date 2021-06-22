using System;
using FluentSQL;
using TestFluentSql.Models;
using Xunit;

namespace TestFluentSql.DriverTests
{
    [Collection("SqlServer")]
    public class DriverTests
    {
        private readonly SqlServerDatabase _sqlServerDatabase;
        
        public DriverTests(SqlServerDatabase sqlServerDatabase)
        {
            _sqlServerDatabase = sqlServerDatabase;
        }
        [Fact]
        public void Test_PrimaryKeyOptionsBuilder()
        {
            var options = new FluentSqlOptions()
                .SetPrimaryKey<Student>(s => new {s.StudentId})
                .AddForeignKeyReference<Student,StudentCourse>(
                    student => student.StudentCourses,
                    studentCourse => studentCourse.Student,
                    ((student, course) => student.StudentId == course.StudentId));
            var driver = new Driver(options);
        
        }
    }
}