using Interfaces;
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
        private Mock<ITeamRepository> teamRepoMock;
        private Mock<IStudentRepository> studentRepoMock;

        private SortedDictionary<int, Student> allStudents;
        private SortedDictionary<int, Team> allTeams;

        public StudentTeamServiceTest()
        {
            allStudents = new SortedDictionary<int, Student>();

            allTeams = new SortedDictionary<int, Team>();

            teamRepoMock = new Mock<ITeamRepository>();
            teamRepoMock.Setup(repo => repo.Add(It.IsAny<Team>())).Callback<Team>(t => allTeams.Add(t.Id, t));
            teamRepoMock.Setup(repo => repo.Update(It.IsAny<Team>())).Callback<Team>(t => allTeams[t.Id] = t);
            teamRepoMock.Setup(repo => repo.Remove(It.IsAny<Team>())).Callback<Team>(t => allTeams.Remove(t.Id));
            teamRepoMock.Setup(repo => repo.GetAll()).Returns(() => allTeams.Values);
            teamRepoMock.Setup(repo => repo.GetById(It.IsAny<int>())).Returns<int>(id => allTeams.ContainsKey(id) ? allTeams[id] : null);

            studentRepoMock = new Mock<IStudentRepository>();
            studentRepoMock.Setup(repo => repo.Add(It.IsAny<Student>())).Callback<Student>(s => allStudents.Add(s.Id, s));
            studentRepoMock.Setup(repo => repo.Update(It.IsAny<Student>())).Callback<Student>(s => allStudents[s.Id] = s);
            studentRepoMock.Setup(repo => repo.Remove(It.IsAny<Student>())).Callback<Student>(s => allStudents.Remove(s.Id));
            studentRepoMock.Setup(repo => repo.GetAll()).Returns(() => allStudents.Values);
            studentRepoMock.Setup(repo => repo.GetById(It.IsAny<int>())).Returns<int>(id => allStudents.ContainsKey(id) ? allStudents[id] : null);
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

        [Fact]
        public void AddStudentToTeam_StudentAndTeamExistsStudentNotYetAssigned()
        {
            // arrange
            IStudentRepository studentRepo = studentRepoMock.Object;
            ITeamRepository teamRepo = teamRepoMock.Object;

            Student s1 = new Student() { Id = 1 };
            Student s2 = new Student() { Id = 2 };
            Student s3 = new Student() { Id = 3 };
            Student s4 = new Student() { Id = 4 };

            // all students exists in studentRepo
            studentRepo.Add(s1);
            studentRepo.Add(s2);
            studentRepo.Add(s3);
            studentRepo.Add(s4);

            Team t = new Team() { Id = 1, Students = new List<Student>() { s1, s2, s3 } };

            // team t exists in teamRepo
            teamRepo.Add(t);

            List<Student> studentsBeforeTest = new List<Student>(studentRepo.GetAll());
            List<Team> teamsBeforeTest = new List<Team>(teamRepo.GetAll());

            var service = new StudentTeamService(studentRepo, teamRepo);

            // act
            service.AddStudentToTeam(t, s4);

            // assert

            // student is now in the team
            Assert.NotNull(t.Students.Where<Student>(s => s == s4));

            // team object har been updated in the team repository
            teamRepoMock.Verify(repo => repo.Update(It.Is<Team>(team => team == t)), Times.Once);

            // verify that repositories contains same objects as before act.
            Assert.Equal(studentsBeforeTest, studentRepoMock.Object.GetAll());
            Assert.Equal(teamsBeforeTest, teamRepoMock.Object.GetAll());
        }

        [Fact]
        public void AddStudentToTeam_ToManyStudents_ExpectInvalidOperationException()
        {
            // arrange
            IStudentRepository studentRepo = studentRepoMock.Object;
            ITeamRepository teamRepo = teamRepoMock.Object;

            Student s1 = new Student() { Id = 1 };
            Student s2 = new Student() { Id = 2 };
            Student s3 = new Student() { Id = 3 };
            Student s4 = new Student() { Id = 4 };
            Student s5 = new Student() { Id = 5 };

            // all students exists in studentRepo
            studentRepo.Add(s1);
            studentRepo.Add(s2);
            studentRepo.Add(s3);
            studentRepo.Add(s4);
            studentRepo.Add(s5);

            // team t is full
            Team t = new Team() { Id = 1, Students = new List<Student>() { s1, s2, s3, s4 } };

            // team t exists in teamRepo
            teamRepo.Add(t);

            List<Student> studentsBeforeTest = new List<Student>(studentRepo.GetAll());
            List<Team> teamsBeforeTest = new List<Team>(teamRepo.GetAll());

            var service = new StudentTeamService(studentRepo, teamRepo);

            // act + assert
            var ex = Assert.Throws<InvalidOperationException>(() => service.AddStudentToTeam(t, s5));

            Assert.Equal("Team is full", ex.Message);
            teamRepoMock.Verify(repo => repo.Update(It.Is<Team>(team => team == t)), Times.Never);

            // verify that repositories contains same objects as before act.
            Assert.Equal(studentsBeforeTest, studentRepoMock.Object.GetAll());
            Assert.Equal(teamsBeforeTest, teamRepoMock.Object.GetAll());
        }

        [Fact]
        public void AddStudentToTeam_StudentIsAlreadyAssigned_ExpectInvalidOperationException()
        {
            // arrange
            IStudentRepository studentRepo = studentRepoMock.Object;
            ITeamRepository teamRepo = teamRepoMock.Object;

            Student s1 = new Student() { Id = 1 };
            Student s2 = new Student() { Id = 2 };

            // all students exists in studentRepo
            studentRepo.Add(s1);
            studentRepo.Add(s2);

            Team t1 = new Team() { Id = 1, Students = new List<Student>() { s1 } };
            Team t2 = new Team() { Id = 2, Students = new List<Student>() { s2 } };

            // all teams exists in teamRepo
            teamRepo.Add(t1);
            teamRepo.Add(t2);

            List<Student> studentsBeforeTest = new List<Student>(studentRepo.GetAll());
            List<Team> teamsBeforeTest = new List<Team>(teamRepo.GetAll());

            var service = new StudentTeamService(studentRepo, teamRepo);

            // act + assert
            var ex = Assert.Throws<InvalidOperationException>(() => service.AddStudentToTeam(t1, s2));

            Assert.Equal("Student is already assigned to a team", ex.Message);
            teamRepoMock.Verify(repo => repo.Update(It.Is<Team>(t => t == t1)), Times.Never);

            // verify that repositories contains same objects as before act.
            Assert.Equal(studentsBeforeTest, studentRepoMock.Object.GetAll());
            Assert.Equal(teamsBeforeTest, teamRepoMock.Object.GetAll());
        }

        [Fact]
        public void AddStudentToTeam_StudentIsNull_ExpectArgumentException()
        {
            // arrange
            IStudentRepository studentRepo = studentRepoMock.Object;
            ITeamRepository teamRepo = teamRepoMock.Object;

            var team = new Team() { Id = 1, Students = new List<Student>() };
            teamRepo.Add(team);

            List<Student> studentsBeforeTest = new List<Student>(studentRepo.GetAll());
            List<Team> teamsBeforeTest = new List<Team>(teamRepo.GetAll());

            var service = new StudentTeamService(studentRepo, teamRepo);

            // act + assert
            var ex = Assert.Throws<ArgumentException>(() => service.AddStudentToTeam(team, null));

            Assert.Equal("Student is missing", ex.Message);
            teamRepoMock.Verify(repo => repo.Update(It.Is<Team>(t => t == null)), Times.Never);

            // verify that repositories contains same objects as before act.
            Assert.Equal(studentsBeforeTest, studentRepoMock.Object.GetAll());
            Assert.Equal(teamsBeforeTest, teamRepoMock.Object.GetAll());
        }

        [Fact]
        public void AddStudentToTeam_TeamIsNull_ExpectArgumentException()
        {
            // arrange
            IStudentRepository studentRepo = studentRepoMock.Object;
            ITeamRepository teamRepo = teamRepoMock.Object;

            var student = new Student() { Id = 1 };
            studentRepo.Add(student);

            List<Student> studentsBeforeTest = new List<Student>(studentRepo.GetAll());
            List<Team> teamsBeforeTest = new List<Team>(teamRepo.GetAll());

            var service = new StudentTeamService(studentRepo, teamRepo);

            // act + assert
            var ex = Assert.Throws<ArgumentException>(() => service.AddStudentToTeam(null, student));

            Assert.Equal("Team is missing", ex.Message);
            teamRepoMock.Verify(repo => repo.Update(It.Is<Team>(t => t == null)), Times.Never);

            // verify that repositories contains same objects as before act.
            Assert.Equal(studentsBeforeTest, studentRepoMock.Object.GetAll());
            Assert.Equal(teamsBeforeTest, teamRepoMock.Object.GetAll());
        }

        [Fact]
        public void AddStudentToTeam_StudentDoesNotExist_ExpectArgumentException()
        {
            // arrange
            IStudentRepository studentRepo = studentRepoMock.Object;
            ITeamRepository teamRepo = teamRepoMock.Object;

            // student not in student repository
            var student = new Student() { Id = 1 };

            var team = new Team() { Id = 1, Students = new List<Student>() };
            // team exists in team repository
            teamRepo.Add(team);

            List<Student> studentsBeforeTest = new List<Student>(studentRepo.GetAll());
            List<Team> teamsBeforeTest = new List<Team>(teamRepo.GetAll());

            var service = new StudentTeamService(studentRepoMock.Object, teamRepoMock.Object);

            // act + assert
            var ex = Assert.Throws<ArgumentException>(() => service.AddStudentToTeam(team, student));

            Assert.Equal("Student not found", ex.Message);
            teamRepoMock.Verify(repo => repo.Update(It.Is<Team>(t => t == null)), Times.Never);

            // verify that repositories contains same objects as before act.
            Assert.Equal(studentsBeforeTest, studentRepoMock.Object.GetAll());
            Assert.Equal(teamsBeforeTest, teamRepoMock.Object.GetAll());
        }

        [Fact]
        public void AddStudentToTeam_TeamDoesNotExist_ExpectArgumentException()
        {
            // arrange
            IStudentRepository studentRepo = studentRepoMock.Object;
            ITeamRepository teamRepo = teamRepoMock.Object;

            var student = new Student() { Id = 1 };
            // student exists in student repository
            studentRepo.Add(student);

            // team not in team repository
            var team = new Team() { Id = 1, Students = new List<Student>() };

            List<Student> studentsBeforeTest = new List<Student>(studentRepo.GetAll());
            List<Team> teamsBeforeTest = new List<Team>(teamRepo.GetAll());

            var service = new StudentTeamService(studentRepoMock.Object, teamRepoMock.Object);

            // act + assert
            var ex = Assert.Throws<ArgumentException>(() => service.AddStudentToTeam(team, student));

            Assert.Equal("Team not found", ex.Message);
            teamRepoMock.Verify(repo => repo.Update(It.Is<Team>(t => t == null)), Times.Never);

            // verify that repositories contains same objects as before act.
            Assert.Equal(studentsBeforeTest, studentRepoMock.Object.GetAll());
            Assert.Equal(teamsBeforeTest, teamRepoMock.Object.GetAll());
        }

        #endregion

        #region MoveStudentToNewTeam

        [Fact]
        public void MoveStudentToNewTeam_LegalMove()
        {
            // Team t1 = new Team(){Id = 1}
        }

        #endregion
    }
}
