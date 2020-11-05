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
        private readonly Mock<ITeamRepository> teamRepoMock;
        private readonly Mock<IStudentRepository> studentRepoMock;

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
            var studentRepo = studentRepoMock.Object;
            var teamRepo = teamRepoMock.Object;

            var s1 = new Student() { Id = 1 };
            var s2 = new Student() { Id = 2 };
            var s3 = new Student() { Id = 3 };
            var s4 = new Student() { Id = 4 };

            // all students exists in studentRepo
            studentRepo.Add(s1);
            studentRepo.Add(s2);
            studentRepo.Add(s3);
            studentRepo.Add(s4);

            var t = new Team() { Id = 1, Students = new List<Student>() { s1, s2, s3 } };

            // team t exists in teamRepo
            teamRepo.Add(t);

            var studentsBeforeTest = new List<Student>(studentRepo.GetAll());
            var teamsBeforeTest = new List<Team>(teamRepo.GetAll());

            var service = new StudentTeamService(studentRepo, teamRepo);

            // act
            service.AddStudentToTeam(t, s4);

            // assert

            // student is now in the team
            Assert.NotNull(teamRepo.GetById(t.Id).Students.Where<Student>(s => s == s4));

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
            var studentRepo = studentRepoMock.Object;
            var teamRepo = teamRepoMock.Object;

            var s1 = new Student() { Id = 1 };
            var s2 = new Student() { Id = 2 };
            var s3 = new Student() { Id = 3 };
            var s4 = new Student() { Id = 4 };
            var s5 = new Student() { Id = 5 };

            // all students exists in studentRepo
            studentRepo.Add(s1);
            studentRepo.Add(s2);
            studentRepo.Add(s3);
            studentRepo.Add(s4);
            studentRepo.Add(s5);

            // team t is full
            var t = new Team() { Id = 1, Students = new List<Student>() { s1, s2, s3, s4 } };

            // team t exists in teamRepo
            teamRepo.Add(t);

            var studentsBeforeTest = new List<Student>(studentRepo.GetAll());
            var teamsBeforeTest = new List<Team>(teamRepo.GetAll());

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
            var studentRepo = studentRepoMock.Object;
            var teamRepo = teamRepoMock.Object;

            var s1 = new Student() { Id = 1 };
            var s2 = new Student() { Id = 2 };

            // all students exists in studentRepo
            studentRepo.Add(s1);
            studentRepo.Add(s2);

            var t1 = new Team() { Id = 1, Students = new List<Student>() { s1 } };
            var t2 = new Team() { Id = 2, Students = new List<Student>() { s2 } };

            // all teams exists in teamRepo
            teamRepo.Add(t1);
            teamRepo.Add(t2);

            var studentsBeforeTest = new List<Student>(studentRepo.GetAll());
            var teamsBeforeTest = new List<Team>(teamRepo.GetAll());

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
            var studentRepo = studentRepoMock.Object;
            var teamRepo = teamRepoMock.Object;

            var team = new Team() { Id = 1, Students = new List<Student>() };
            teamRepo.Add(team);

            var studentsBeforeTest = new List<Student>(studentRepo.GetAll());
            var teamsBeforeTest = new List<Team>(teamRepo.GetAll());

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
            var studentRepo = studentRepoMock.Object;
            var teamRepo = teamRepoMock.Object;

            var student = new Student() { Id = 1 };
            studentRepo.Add(student);

            var studentsBeforeTest = new List<Student>(studentRepo.GetAll());
            var teamsBeforeTest = new List<Team>(teamRepo.GetAll());

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
            var studentRepo = studentRepoMock.Object;
            var teamRepo = teamRepoMock.Object;

            // student not in student repository
            var student = new Student() { Id = 1 };

            var team = new Team() { Id = 1, Students = new List<Student>() };
            // team exists in team repository
            teamRepo.Add(team);

            var studentsBeforeTest = new List<Student>(studentRepo.GetAll());
            var teamsBeforeTest = new List<Team>(teamRepo.GetAll());

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
            var studentRepo = studentRepoMock.Object;
            var teamRepo = teamRepoMock.Object;

            var student = new Student() { Id = 1 };
            // student exists in student repository
            studentRepo.Add(student);

            // team not in team repository
            var team = new Team() { Id = 1, Students = new List<Student>() };

            var studentsBeforeTest = new List<Student>(studentRepo.GetAll());
            var teamsBeforeTest = new List<Team>(teamRepo.GetAll());

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
            // arrange
            var studentRepo = studentRepoMock.Object;
            var teamRepo = teamRepoMock.Object;

            var s1 = new Student() { Id = 1 };
            var s2 = new Student() { Id = 2 };

            // all students exists in studentRepo
            studentRepo.Add(s1);
            studentRepo.Add(s2);

            var t1 = new Team() { Id = 1, Students = new List<Student>() { s1, s2 } };
            var t2 = new Team() { Id = 2, Students = new List<Student>() { } };

            // team t exists in teamRepo
            teamRepo.Add(t1);
            teamRepo.Add(t2);

            var studentsBeforeTest = new List<Student>(studentRepo.GetAll());
            var teamsBeforeTest = new List<Team>(teamRepo.GetAll());

            var service = new StudentTeamService(studentRepo, teamRepo);

            // act
            service.MoveStudentToNewTeam(t1, t2, s2);

            // assert
            Assert.True(t1.Students.Count == 1);
            Assert.Contains(s1, t1.Students);

            Assert.True(t2.Students.Count == 1);
            Assert.Contains(s2, t2.Students);

            teamRepoMock.Verify(repo => repo.Update(It.Is<Team>(t => t == t1)), Times.Once);
            teamRepoMock.Verify(repo => repo.Update(It.Is<Team>(t => t == t2)), Times.Once);

            // verify that repositories contains same objects as before act.
            Assert.Equal(studentsBeforeTest, studentRepoMock.Object.GetAll());
            Assert.Equal(teamsBeforeTest, teamRepoMock.Object.GetAll());
        }

        [Fact]
        public void MoveStudentToNewTeam_FromTeamIsNull_ExpectArgumentException()
        {
            // arrange
            var studentRepo = studentRepoMock.Object;
            var teamRepo = teamRepoMock.Object;

            var student = new Student() { Id = 1 };
            studentRepo.Add(student);

            var toTeam = new Team() { Id = 1, Students = new List<Student>() };
            teamRepo.Add(toTeam);

            var studentsBeforeTest = new List<Student>(studentRepo.GetAll());
            var teamsBeforeTest = new List<Team>(teamRepo.GetAll());

            var service = new StudentTeamService(studentRepo, teamRepo);

            // act + assert
            var ex = Assert.Throws<ArgumentException>(() => service.MoveStudentToNewTeam(null, toTeam, student));

            Assert.Equal("From Team is missing", ex.Message);
            teamRepoMock.Verify(repo => repo.Update(It.Is<Team>(t => t == null)), Times.Never);
            teamRepoMock.Verify(repo => repo.Update(It.Is<Team>(t => t == toTeam)), Times.Never);

            // verify that repositories contains same objects as before act.
            Assert.Equal(studentsBeforeTest, studentRepoMock.Object.GetAll());
            Assert.Equal(teamsBeforeTest, teamRepoMock.Object.GetAll());
        }

        [Fact]
        public void MoveStudentToNewTeam_ToTeamIsNull_ExpectArgumentException()
        {
            // arrange
            var studentRepo = studentRepoMock.Object;
            var teamRepo = teamRepoMock.Object;

            var student = new Student() { Id = 1 };
            studentRepo.Add(student);

            var fromTeam = new Team() { Id = 1, Students = new List<Student>() };
            teamRepo.Add(fromTeam);

            var studentsBeforeTest = new List<Student>(studentRepo.GetAll());
            var teamsBeforeTest = new List<Team>(teamRepo.GetAll());

            var service = new StudentTeamService(studentRepo, teamRepo);

            // act + assert
            var ex = Assert.Throws<ArgumentException>(() => service.MoveStudentToNewTeam(fromTeam, null, student));

            Assert.Equal("To Team is missing", ex.Message);
            teamRepoMock.Verify(repo => repo.Update(It.Is<Team>(t => t == fromTeam)), Times.Never);
            teamRepoMock.Verify(repo => repo.Update(It.Is<Team>(t => t == null)), Times.Never);

            // verify that repositories contains same objects as before act.
            Assert.Equal(studentsBeforeTest, studentRepoMock.Object.GetAll());
            Assert.Equal(teamsBeforeTest, teamRepoMock.Object.GetAll());
        }

        [Fact]
        public void MoveStudentToNewTeam_StudentIsNull_ExpectArgumentException()
        {
            // arrange
            var studentRepo = studentRepoMock.Object;
            var teamRepo = teamRepoMock.Object;

            var fromTeam = new Team() { Id = 1, Students = new List<Student>() };
            var toTeam = new Team() { Id = 2, Students = new List<Student>() };
            teamRepo.Add(fromTeam);
            teamRepo.Add(toTeam);

            var studentsBeforeTest = new List<Student>(studentRepo.GetAll());
            var teamsBeforeTest = new List<Team>(teamRepo.GetAll());

            var service = new StudentTeamService(studentRepo, teamRepo);

            // act + assert
            var ex = Assert.Throws<ArgumentException>(() => service.MoveStudentToNewTeam(fromTeam, toTeam, null));

            Assert.Equal("Student is missing", ex.Message);
            teamRepoMock.Verify(repo => repo.Update(It.Is<Team>(t => t == fromTeam)), Times.Never);
            teamRepoMock.Verify(repo => repo.Update(It.Is<Team>(t => t == toTeam)), Times.Never);

            // verify that repositories contains same objects as before act.
            Assert.Equal(studentsBeforeTest, studentRepoMock.Object.GetAll());
            Assert.Equal(teamsBeforeTest, teamRepoMock.Object.GetAll());
        }

        [Fact]
        public void MoveStudentToNewTeam_FromTeamDoesNotExist_ExpectInvalidOperationException()
        {
            // arrange
            var studentRepo = studentRepoMock.Object;
            var teamRepo = teamRepoMock.Object;

            var student = new Student() { Id = 1 };
            studentRepo.Add(student);

            var fromTeam = new Team() { Id = 1, Students = new List<Student>() };
            var toTeam = new Team() { Id = 2, Students = new List<Student>() };

            // Only toTeam exists in team repository
            teamRepo.Add(toTeam);

            var studentsBeforeTest = new List<Student>(studentRepo.GetAll());
            var teamsBeforeTest = new List<Team>(teamRepo.GetAll());

            var service = new StudentTeamService(studentRepo, teamRepo);

            // act + assert
            var ex = Assert.Throws<InvalidOperationException>(() => service.MoveStudentToNewTeam(fromTeam, toTeam, student));

            Assert.Equal("From Team not found", ex.Message);
            teamRepoMock.Verify(repo => repo.Update(It.Is<Team>(t => t == fromTeam)), Times.Never);
            teamRepoMock.Verify(repo => repo.Update(It.Is<Team>(t => t == toTeam)), Times.Never);

            // verify that repositories contains same objects as before act.
            Assert.Equal(studentsBeforeTest, studentRepoMock.Object.GetAll());
            Assert.Equal(teamsBeforeTest, teamRepoMock.Object.GetAll());
        }

        [Fact]
        public void MoveStudentToNewTeam_ToTeamDoesNotExist_ExpectInvalidOperationException()
        {
            // arrange
            var studentRepo = studentRepoMock.Object;
            var teamRepo = teamRepoMock.Object;

            var student = new Student() { Id = 1 };
            studentRepo.Add(student);

            var fromTeam = new Team() { Id = 1, Students = new List<Student>() };
            var toTeam = new Team() { Id = 2, Students = new List<Student>() };

            // Only fromTeam exists in team repository
            teamRepo.Add(fromTeam);

            var studentsBeforeTest = new List<Student>(studentRepo.GetAll());
            var teamsBeforeTest = new List<Team>(teamRepo.GetAll());

            var service = new StudentTeamService(studentRepo, teamRepo);

            // act + assert
            var ex = Assert.Throws<InvalidOperationException>(() => service.MoveStudentToNewTeam(fromTeam, toTeam, student));

            Assert.Equal("To Team not found", ex.Message);
            teamRepoMock.Verify(repo => repo.Update(It.Is<Team>(t => t == fromTeam)), Times.Never);
            teamRepoMock.Verify(repo => repo.Update(It.Is<Team>(t => t == toTeam)), Times.Never);

            // verify that repositories contains same objects as before act.
            Assert.Equal(studentsBeforeTest, studentRepoMock.Object.GetAll());
            Assert.Equal(teamsBeforeTest, teamRepoMock.Object.GetAll());
        }

        [Fact]
        public void MoveStudentToNewTeam_StudentDoesNotExist_ExpectInvalidOperationException()
        {
            // arrange
            var studentRepo = studentRepoMock.Object;
            var teamRepo = teamRepoMock.Object;

            // student not in student repository
            var student = new Student() { Id = 1 };

            var fromTeam = new Team() { Id = 1, Students = new List<Student>() };
            var toTeam = new Team() { Id = 2, Students = new List<Student>() };

            // both teams exist in team repository
            teamRepo.Add(fromTeam);
            teamRepo.Add(toTeam);

            var studentsBeforeTest = new List<Student>(studentRepo.GetAll());
            var teamsBeforeTest = new List<Team>(teamRepo.GetAll());

            var service = new StudentTeamService(studentRepo, teamRepo);

            // act + assert
            var ex = Assert.Throws<InvalidOperationException>(() => service.MoveStudentToNewTeam(fromTeam, toTeam, student));

            Assert.Equal("Student not found", ex.Message);
            teamRepoMock.Verify(repo => repo.Update(It.Is<Team>(t => t == fromTeam)), Times.Never);
            teamRepoMock.Verify(repo => repo.Update(It.Is<Team>(t => t == toTeam)), Times.Never);

            // verify that repositories contains same objects as before act.
            Assert.Equal(studentsBeforeTest, studentRepoMock.Object.GetAll());
            Assert.Equal(teamsBeforeTest, teamRepoMock.Object.GetAll());
        }

        [Fact]
        public void MoveStudentToNewTeam_StudentNotMemberOfFromTeam_ExpectInvalidOperationException()
        {
            // arrange
            var studentRepo = studentRepoMock.Object;
            var teamRepo = teamRepoMock.Object;

            // student exists in student repository
            var student = new Student() { Id = 1 };
            studentRepo.Add(student);

            // both teams exist in team repository
            // student is not a member of fromTeam
            var fromTeam = new Team() { Id = 1, Students = new List<Student>() };
            var toTeam = new Team() { Id = 2, Students = new List<Student>() };
            teamRepo.Add(fromTeam);
            teamRepo.Add(toTeam);

            var studentsBeforeTest = new List<Student>(studentRepo.GetAll());
            var teamsBeforeTest = new List<Team>(teamRepo.GetAll());

            var service = new StudentTeamService(studentRepo, teamRepo);

            // act + assert
            var ex = Assert.Throws<InvalidOperationException>(() => service.MoveStudentToNewTeam(fromTeam, toTeam, student));

            Assert.Equal("Student is not a member of From Team", ex.Message);
            teamRepoMock.Verify(repo => repo.Update(It.Is<Team>(t => t == fromTeam)), Times.Never);
            teamRepoMock.Verify(repo => repo.Update(It.Is<Team>(t => t == toTeam)), Times.Never);

            // verify that repositories contains same objects as before act.
            Assert.Equal(studentsBeforeTest, studentRepoMock.Object.GetAll());
            Assert.Equal(teamsBeforeTest, teamRepoMock.Object.GetAll());
        }

        [Fact]
        public void MoveStudentToNewTeam_ToTeamIsFull_ExpectInvalidOperationException()
        {
            // arrange
            var studentRepo = studentRepoMock.Object;
            var teamRepo = teamRepoMock.Object;

            // students exists in student repository
            var s1 = new Student() { Id = 1 };
            var s2 = new Student() { Id = 2 };
            var s3 = new Student() { Id = 3 };
            var s4 = new Student() { Id = 4 };
            var s5 = new Student() { Id = 5 };
            studentRepo.Add(s1);
            studentRepo.Add(s2);
            studentRepo.Add(s3);
            studentRepo.Add(s4);
            studentRepo.Add(s5);

            // both teams exist in team repository
            // student is not a member of fromTeam
            var fromTeam = new Team() { Id = 1, Students = new List<Student>() { s5 } };
            var toTeam = new Team() { Id = 2, Students = new List<Student>() { s1, s2, s3, s4 } };
            teamRepo.Add(fromTeam);
            teamRepo.Add(toTeam);

            var studentsBeforeTest = new List<Student>(studentRepo.GetAll());
            var teamsBeforeTest = new List<Team>(teamRepo.GetAll());

            var service = new StudentTeamService(studentRepo, teamRepo);

            // act + assert
            var ex = Assert.Throws<InvalidOperationException>(() => service.MoveStudentToNewTeam(fromTeam, toTeam, s5));

            Assert.Equal("To Team is full", ex.Message);
            teamRepoMock.Verify(repo => repo.Update(It.Is<Team>(t => t == fromTeam)), Times.Never);
            teamRepoMock.Verify(repo => repo.Update(It.Is<Team>(t => t == toTeam)), Times.Never);

            // verify that repositories contains same objects as before act.
            Assert.Equal(studentsBeforeTest, studentRepoMock.Object.GetAll());
            Assert.Equal(teamsBeforeTest, teamRepoMock.Object.GetAll());
        }



        #endregion

        #region RemoveStudentFromTeam

        [Fact]
        public void RemoveStudentFromTeam_LegalRemove()
        {
            // arrange
            var studentRepo = studentRepoMock.Object;
            var teamRepo = teamRepoMock.Object;

            // student s exists
            var s = new Student() { Id = 1 };
            studentRepo.Add(s);

            // team t exists with student s as a member
            var t = new Team() { Id = 1, Students = new List<Student>() { s } };
            teamRepo.Add(t);

            var studentsBeforeTest = new List<Student>(studentRepo.GetAll());
            var teamsBeforeTest = new List<Team>(teamRepo.GetAll());

            var service = new StudentTeamService(studentRepo, teamRepo);

            // act
            service.RemoveStudentFromTeam(t, s);

            // assert
            Assert.True(!teamRepo.GetById(t.Id).Students.Contains(s));
            teamRepoMock.Verify(repo => repo.Update(It.Is<Team>(team => team == t)), Times.Once);

            // verify that repositories contains same objects as before act.
            Assert.Equal(studentsBeforeTest, studentRepoMock.Object.GetAll());
            Assert.Equal(teamsBeforeTest, teamRepoMock.Object.GetAll());
        }

        [Fact]
        public void RemoveStudentFromTeam_TeamIsNull_ExpectArgumentException()
        {
            // arrange
            var studentRepo = studentRepoMock.Object;
            var teamRepo = teamRepoMock.Object;

            // student s exists
            var s = new Student() { Id = 1 };
            studentRepo.Add(s);

            var studentsBeforeTest = new List<Student>(studentRepo.GetAll());
            var teamsBeforeTest = new List<Team>(teamRepo.GetAll());

            var service = new StudentTeamService(studentRepo, teamRepo);

            // act + assert
            var ex = Assert.Throws<ArgumentException>(() => service.RemoveStudentFromTeam(null, s));

            // assert
            Assert.Equal("Team is missing", ex.Message);
            teamRepoMock.Verify(repo => repo.Update(It.Is<Team>(team => team == null)), Times.Never);

            // verify that repositories contains same objects as before act.
            Assert.Equal(studentsBeforeTest, studentRepoMock.Object.GetAll());
            Assert.Equal(teamsBeforeTest, teamRepoMock.Object.GetAll());
        }

        [Fact]
        public void RemoveStudentFromTeam_StudentIsNull_ExpectArgumentException()
        {
            // arrange
            var studentRepo = studentRepoMock.Object;
            var teamRepo = teamRepoMock.Object;

            // team t exists
            var t = new Team() { Id = 1, Students = new List<Student>() };
            teamRepo.Add(t);

            var studentsBeforeTest = new List<Student>(studentRepo.GetAll());
            var teamsBeforeTest = new List<Team>(teamRepo.GetAll());

            var service = new StudentTeamService(studentRepo, teamRepo);

            // act + assert
            var ex = Assert.Throws<ArgumentException>(() => service.RemoveStudentFromTeam(t, null));

            // assert
            Assert.Equal("Student is missing", ex.Message);
            teamRepoMock.Verify(repo => repo.Update(It.Is<Team>(team => team == t)), Times.Never);

            // verify that repositories contains same objects as before act.
            Assert.Equal(studentsBeforeTest, studentRepoMock.Object.GetAll());
            Assert.Equal(teamsBeforeTest, teamRepoMock.Object.GetAll());
        }

        [Fact]
        public void RemoveStudentFromTeam_TeamDoesNotExist_ExpectInvalidOperationException()
        {
            // arrange
            var studentRepo = studentRepoMock.Object;
            var teamRepo = teamRepoMock.Object;

            // student s exists
            var s = new Student() { Id = 1 };
            studentRepo.Add(s);

            // team t does not exist
            var t = new Team() { Id = 1, Students = new List<Student>() };

            var studentsBeforeTest = new List<Student>(studentRepo.GetAll());
            var teamsBeforeTest = new List<Team>(teamRepo.GetAll());

            var service = new StudentTeamService(studentRepo, teamRepo);

            // act + assert
            var ex = Assert.Throws<InvalidOperationException>(() => service.RemoveStudentFromTeam(t, s));

            // assert
            Assert.Equal("Team not found", ex.Message);
            teamRepoMock.Verify(repo => repo.Update(It.Is<Team>(team => team == t)), Times.Never);

            // verify that repositories contains same objects as before act.
            Assert.Equal(studentsBeforeTest, studentRepoMock.Object.GetAll());
            Assert.Equal(teamsBeforeTest, teamRepoMock.Object.GetAll());
        }

        [Fact]
        public void RemoveStudentFromTeam_StudentDoesNotExist_ExpectInvalidOperationException()
        {
            // arrange
            var studentRepo = studentRepoMock.Object;
            var teamRepo = teamRepoMock.Object;

            // student s does not exist
            var s = new Student() { Id = 1 };

            // team t exists
            var t = new Team() { Id = 1, Students = new List<Student>() };
            teamRepo.Add(t);

            var studentsBeforeTest = new List<Student>(studentRepo.GetAll());
            var teamsBeforeTest = new List<Team>(teamRepo.GetAll());

            var service = new StudentTeamService(studentRepo, teamRepo);

            // act + assert
            var ex = Assert.Throws<InvalidOperationException>(() => service.RemoveStudentFromTeam(t, s));

            // assert
            Assert.Equal("Student not found", ex.Message);
            teamRepoMock.Verify(repo => repo.Update(It.Is<Team>(team => team == t)), Times.Never);

            // verify that repositories contains same objects as before act.
            Assert.Equal(studentsBeforeTest, studentRepoMock.Object.GetAll());
            Assert.Equal(teamsBeforeTest, teamRepoMock.Object.GetAll());
        }

        [Fact]
        public void RemoveStudentFromTeam_StudentNotMemberOfTeam_ExpectInvalidOperationException()
        {
            // arrange
            var studentRepo = studentRepoMock.Object;
            var teamRepo = teamRepoMock.Object;

            // student s exists
            var s = new Student() { Id = 1 };
            studentRepo.Add(s);

            // team t exists, but student s is not a member
            var t = new Team() { Id = 1, Students = new List<Student>() };
            teamRepo.Add(t);

            var studentsBeforeTest = new List<Student>(studentRepo.GetAll());
            var teamsBeforeTest = new List<Team>(teamRepo.GetAll());

            var service = new StudentTeamService(studentRepo, teamRepo);

            // act + assert
            var ex = Assert.Throws<InvalidOperationException>(() => service.RemoveStudentFromTeam(t, s));

            // assert
            Assert.Equal("Student is not a member of the team", ex.Message);
            teamRepoMock.Verify(repo => repo.Update(It.Is<Team>(team => team == t)), Times.Never);

            // verify that repositories contains same objects as before act.
            Assert.Equal(studentsBeforeTest, studentRepoMock.Object.GetAll());
            Assert.Equal(teamsBeforeTest, teamRepoMock.Object.GetAll());
        }

        #endregion

        #region GetNonAssignedStudents

        //[Theory]
        //[InlineData(2)]         // non-assignedStudents Ids = {}
        //[InlineData(1)]         // non-assignedStudents Ids = {2}
        //[InlineData(0)]         // non-assignedStudents Ids = {1, 2}
        //public void GetNonAssignedStudents(int assignedStudentsCount)
        //{
        //    // arrange
        //    var studentRepo = studentRepoMock.Object;
        //    var teamRepo = teamRepoMock.Object;

        //    // two students exists
        //    var s1 = new Student() { Id = 1 };
        //    var s2 = new Student() { Id = 2 };
        //    studentRepo.Add(s1);
        //    studentRepo.Add(s2);

        //    var allStudents = studentRepo.GetAll().ToList();
        //    var assignedStudents = allStudents.GetRange(0, assignedStudentsCount);
        //    var expectedResult = allStudents.Except(assignedStudents);

        //    // team t exists with student s1 as a member
        //    var t = new Team() { Id = 1, Students = assignedStudents};
        //    teamRepo.Add(t);

        //    var studentsBeforeTest = new List<Student>(studentRepo.GetAll());
        //    var teamsBeforeTest = new List<Team>(teamRepo.GetAll());

        //    var service = new StudentTeamService(studentRepo, teamRepo);

        //    // act
        //    IEnumerable<Student> result = service.GetNonAssignedStudents();

        //    // assert
        //    Assert.Equal(expectedResult, result.ToList());

        //    // verify that repositories contains same objects as before act.
        //    Assert.Equal(studentsBeforeTest, studentRepoMock.Object.GetAll());
        //    Assert.Equal(teamsBeforeTest, teamRepoMock.Object.GetAll());
        //}

        #endregion
    }
}
