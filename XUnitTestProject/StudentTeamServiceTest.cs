using Interfaces;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.Serialization;
using Model;
using Moq;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace XUnitTestProject
{
    public class StudentTeamServiceTest
    {
        private static Mock<ITeamRepository> teamRepoMock;
        private static Mock<IStudentRepository> studentRepoMock;

        public StudentTeamServiceTest()
        {
            teamRepoMock = new Mock<ITeamRepository>();
            studentRepoMock = new Mock<IStudentRepository>();
        }

        #region Create StudentTeamService

        [Fact]
        public void CreateStudentTeamService_ValidRepositories()
        {
            // act
            var service = new StudentTeamService(studentRepoMock.Object, teamRepoMock.Object);

            // assert
            Assert.NotNull(service);
            Assert.True(service is StudentTeamService);
        }

        [Fact]
        public void CreateStudentTeamService_StudentRepositoryIsNull_ExpectArgumentException()
        {
            // arrange
            StudentTeamService service = null;

            // act + assert
            var ex = Assert.Throws<ArgumentException>(() =>
            {
                service = new StudentTeamService(null, teamRepoMock.Object);
            });

            Assert.Equal("StudentRepository is missing", ex.Message);
            Assert.Null(service);
        }

        [Fact]
        public void CreateStudentTeamService_TeamRepositoryIsNull_ExpectArgumentException()
        {
            // arrange
            StudentTeamService service = null;

            // act + assert
            var ex = Assert.Throws<ArgumentException>(() =>
            {
                service = new StudentTeamService(studentRepoMock.Object, null);
            });

            Assert.Equal("TeamRepository is missing", ex.Message);
            Assert.Null(service);
        }

        #endregion

        #region AddStudentToTeam

        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        public void AddStudentToTeam_StudentAndTeamExistsStudentNotYetAssigned(int studentsCount)
        {
            // arrange
            var students = new List<Student>()
            {
                new Student(){ Id = 1},
                new Student(){ Id = 2},
                new Student(){ Id = 3},
                new Student(){ Id = 4}
            };

            var studentsInTeam = students.GetRange(0, studentsCount);

            Student student = new Student() { Id = 4 };
            Team team = new Team() { Id = 1, Students = studentsInTeam }; // team does not contain student with Id = 4

            studentRepoMock.Setup(repo => repo.GetAll()).Returns(() => students.ToArray());

            // Make sure student exists in StudentRepository before test
            studentRepoMock.Setup(sr => sr.GetById(It.Is<int>(id => id == student.Id))).Returns<int>(id => students.Where(s => s.Id == id).FirstOrDefault());

            // Make sure team exists in TeamRepository before test
            teamRepoMock.Setup(tr => tr.GetById(It.Is<int>(id => id == team.Id))).Returns(() => team);

            var service = new StudentTeamService(studentRepoMock.Object, teamRepoMock.Object);

            // act
            service.AddStudentToTeam(team, student);

            // assert
            Assert.NotNull(team.Students.Where<Student>(s => s.Id == student.Id));
            teamRepoMock.Verify(repo => repo.Update(It.Is<Team>(t => t == team)), Times.Once);

            // verify that repositories contains same objects as before act.
            teamRepoMock.Verify(repo => repo.Add(It.IsAny<Team>()), Times.Never);
            teamRepoMock.Verify(repo => repo.Remove(It.IsAny<Team>()), Times.Never);
            studentRepoMock.Verify(repo => repo.Add(It.IsAny<Student>()), Times.Never);
            studentRepoMock.Verify(repo => repo.Update(It.IsAny<Student>()), Times.Never);
            studentRepoMock.Verify(repo => repo.Remove(It.IsAny<Student>()), Times.Never);
        }

        [Fact]
        public void AddStudentToTeam_ToManyStudents_ExpectInvalidOperationException()
        {
            // arrange
            var students = new List<Student>()
            {
                new Student(){ Id = 1},
                new Student(){ Id = 2},
                new Student(){ Id = 3},
                new Student(){ Id = 4},
                new Student(){ Id = 5}
            };

            var studentsInTeam = students.GetRange(0, 4);

            Student student = new Student() { Id = 5 };
            Team team = new Team() { Id = 1, Students = studentsInTeam }; // team does not contain student with Id = 5

            // Make sure student exists in StudentRepository before test
            studentRepoMock.Setup(sr => sr.GetById(It.Is<int>(id => id == student.Id))).Returns<int>(id => students.Where(s => s.Id == id).FirstOrDefault());

            // Make sure team exists in TeamRepository before test
            teamRepoMock.Setup(tr => tr.GetById(It.Is<int>(id => id == team.Id))).Returns(() => team);

            var service = new StudentTeamService(studentRepoMock.Object, teamRepoMock.Object);

            // act + assert
            var ex = Assert.Throws<InvalidOperationException>(() => service.AddStudentToTeam(team, student));

            Assert.Equal("Team is full", ex.Message);
            teamRepoMock.Verify(repo => repo.Update(It.Is<Team>(t => t == team)), Times.Never);
        }

        [Fact]
        public void AddStudentToTeam_StudentIsAlreadyAssigned_ExpectInvalidOperationException()
        {
            // arrange
            var s1 = new Student() { Id = 1 };
            var s2 = new Student() { Id = 2 };

            var allStudents = new List<Student>() { s1, s2 };

            var t1 = new Team() { Id = 1, Students = new List<Student>() { s1 } };
            var t2 = new Team() { Id = 2, Students = new List<Student>() { s2 } };

            var allTeams = new List<Team>() { t1, t2 };

            teamRepoMock.Setup(repo => repo.GetById(It.IsAny<int>())).Returns<int>(id => allTeams.Where(t => t.Id == id).FirstOrDefault());
            teamRepoMock.Setup(repo => repo.GetAll()).Returns(() => allTeams);

            studentRepoMock.Setup(repo => repo.GetById(It.IsAny<int>())).Returns<int>(id => allStudents.Where(t => t.Id == id).FirstOrDefault());
            studentRepoMock.Setup(repo => repo.GetAll()).Returns(() => allStudents);

            var service = new StudentTeamService(studentRepoMock.Object, teamRepoMock.Object);

            // act + assert
            var ex = Assert.Throws<InvalidOperationException>(() => service.AddStudentToTeam(t1, s2));

            Assert.Equal("Student is already assigned to a team", ex.Message);
            teamRepoMock.Verify(repo => repo.Update(It.Is<Team>(t => t == t1)), Times.Never);
        }

        [Fact]
        public void AddStudentToTeam_StudentIsNull_ExpectArgumentException()
        {
            // arrange
            var team = new Team() { Id = 1 };

            var service = new StudentTeamService(studentRepoMock.Object, teamRepoMock.Object);

            // act + assert
            var ex = Assert.Throws<ArgumentException>(() => service.AddStudentToTeam(team, null));

            Assert.Equal("Student is missing", ex.Message);
            teamRepoMock.Verify(repo => repo.Update(It.Is<Team>(t => t == null)), Times.Never);
        }
        [Fact]
        public void AddStudentToTeam_TeamIsNull_ExpectArgumentException()
        {
            // arrange
            var student = new Student() { Id = 1 };

            var service = new StudentTeamService(studentRepoMock.Object, teamRepoMock.Object);

            // act + assert
            var ex = Assert.Throws<ArgumentException>(() => service.AddStudentToTeam(null, student));

            Assert.Equal("Team is missing", ex.Message);
            teamRepoMock.Verify(repo => repo.Update(It.Is<Team>(t => t == null)), Times.Never);
        }

        [Fact]
        public void AddStudentToTeam_StudentDoesNotExist_ExpectArgumentException()
        {
            // arrange
            var student = new Student() { Id = 1 };
            var team = new Team(){ Id = 1, Students = new List<Student>()};
        
            // student does not exist in the students repository
            studentRepoMock.Setup(repo => repo.GetById(It.Is<int>(id => id == student.Id))).Returns(() => null);
            
            // team exists in the team repository
            teamRepoMock.Setup(repo => repo.GetById(It.Is<int>(id => id == team.Id))).Returns(() => team);

            var service = new StudentTeamService(studentRepoMock.Object, teamRepoMock.Object);

            // act + assert
            var ex = Assert.Throws<ArgumentException>(() => service.AddStudentToTeam(team, student));

            Assert.Equal("Student not found", ex.Message);
            teamRepoMock.Verify(repo => repo.Update(It.Is<Team>(t => t == null)), Times.Never);
        }

        [Fact]
        public void AddStudentToTeam_TeamDoesNotExist_ExpectArgumentException()
        {
            // arrange
            var student = new Student() { Id = 1 };
            var team = new Team(){ Id = 1, Students = new List<Student>()};
        
            // student exists in the students repository
            studentRepoMock.Setup(repo => repo.GetById(It.Is<int>(id => id == student.Id))).Returns(() => student);
            
            // team does not exist in the team repository
            teamRepoMock.Setup(repo => repo.GetById(It.Is<int>(id => id == team.Id))).Returns(() => null);

            var service = new StudentTeamService(studentRepoMock.Object, teamRepoMock.Object);

            // act + assert
            var ex = Assert.Throws<ArgumentException>(() => service.AddStudentToTeam(team, student));

            Assert.Equal("Team not found", ex.Message);
            teamRepoMock.Verify(repo => repo.Update(It.Is<Team>(t => t == null)), Times.Never);
        }

        #endregion
    }
}
