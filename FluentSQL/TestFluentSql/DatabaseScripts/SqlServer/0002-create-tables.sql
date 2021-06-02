USE FluentSqlUniversity

CREATE TABLE Student(
    StudentId INT NOT NULL,
    FirstName VARCHAR(255) NOT NULL,
    Age INT,
    UniversityName VARCHAR(255) NOT NULL,
    PRIMARY KEY (StudentId)
)

CREATE TABLE StudentCourse(
    StudentId INT NOT NULL,
    CourseId UNIQUEIDENTIFIER NOT NULL,
    PRIMARY KEY (StudentId,CourseId)
)

CREATE TABLE Course(
    CourseId UNIQUEIDENTIFIER NOT NULL,
    Name VARCHAR(255) NOT NULL,
    OccuringTime DATETIME NOT NULL,
    Duration BIGINT NOT NULL,
    UniversityName VARCHAR(255) NOT NULL,
    PRIMARY KEY (CourseId)
)

CREATE TABLE Teacher(
    FirstName VARCHAR(255) NOT NULL,
    LastName VARCHAR(255) NOT NULL,
    CourseId UNIQUEIDENTIFIER,
    UniversityName VARCHAR(255) NOT NULL,
    PRIMARY KEY (FirstName,LastName),
)

CREATE UNIQUE NONCLUSTERED INDEX Unique_Teacher_Course
ON Teacher(CourseId)
WHERE CourseId IS NOT NULL;

CREATE TABLE University (
    Name VARCHAR(255),
    PRIMARY KEY (Name)
)


ALTER TABLE Student
ADD FOREIGN KEY (UniversityName) REFERENCES University(Name)

ALTER TABLE Course
ADD FOREIGN KEY (UniversityName) REFERENCES University(Name)

ALTER TABLE StudentCourse
ADD FOREIGN KEY (CourseId) REFERENCES Course(CourseId)

ALTER TABLE StudentCourse
ADD FOREIGN KEY (StudentId) REFERENCES Student(StudentId)

ALTER TABLE Teacher
ADD FOREIGN KEY (CourseId) REFERENCES Course(CourseId)

ALTER TABLE Teacher
ADD FOREIGN KEY (UniversityName) REFERENCES University(Name)

