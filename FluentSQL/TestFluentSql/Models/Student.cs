using System;
using System.Collections.Generic;

namespace TestFluentSql.Models
{
    public class Student
    {
        public int StudentId { get; set; }
        public string FirstName { get; set; }
        public int Age { get; set; }
        public string UniversityName { get; set; }
        public University University { get; set; }
        public List<StudentCourse> StudentCourses { get; set; }
    }

    public class StudentCourse
    {
        public int StudentId { get; set; }
        public Guid CourseId { get; set; }
        public Course Course { get; set; }
        public Student Student { get; set; }
    }

    public class Course
    {
        public Guid CourseId { get; set; }
        public string Name { get; set; }
        public DateTime OccuringTime { get; set; }
        public TimeSpan Duration { get; set; }
        public string UniversityName { get; set; }
        public List<StudentCourse> StudentCourses { get; set; }
        public Teacher Teacher { get; set; }
        public University University { get; set; }
    }

    public class University
    {
        public string Name { get; set; }
        public List<Course> Courses { get; set; }
        public List<Student> Students { get; set; }
        public List<Teacher> Teachers { get; set; }
    }

    public class Teacher
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UniversityName { get; set; }
        public Guid? CourseId { get; set; }
        public Course Teaches { get; set; }
        public University University { get; set; }
    }
}