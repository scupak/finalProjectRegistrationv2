using Interfaces;
using Model;
using Moq;
using Services;
using System;
using System.Collections.Generic;
using Xunit;

namespace XUnitTestProject
{
    public class TeamServiceTest
    {
        private readonly Mock<ITeamRepository> repoMock;

        public TeamServiceTest()
        {
            repoMock = new Mock<ITeamRepository>();
        }

        #region CreateTeamService
        [Fact]
        public void CreateTeamService_ValidRepository()
        {
            // arrange
            ITeamRepository repo = repoMock.Object;

            // act
            var service = new TeamService(repo);

            // assert
            Assert.NotNull(service);
        }

        [Fact]
        public void CreateTeamService_RepositoryIsNull_ExpectArgumentException()
        {
            // arrange
            TeamService service = null;

            // act + assert
            var ex = Assert.Throws<ArgumentException>(() => service = new TeamService(null));

            Assert.Equal("Team Repository is missing", ex.Message);
            Assert.Null(service);
        }

        #endregion

        #region AddTeam

        [Fact]
        public void AddTeam_NonExistingValidTeam()
        {
            // arrange
            Team t = new Team()
            {
                Id = 1,
                Students = new List<Student>()
            };

            // Team t does not exist in the TeamRepository
            repoMock.Setup(repo => repo.GetById(It.Is<int>(id => id == t.Id))).Returns(() => null);

            TeamService service = new TeamService(repoMock.Object);

            // act
            service.AddTeam(t);

            // assert
            repoMock.Verify(repo => repo.Add(It.Is<Team>(team => team == t)), Times.Once);
        }

        [Fact]
        public void AddTeam_TeamExists_ExpectInvalidOperationException()
        {
            // arrange
            Team t = new Team()
            {
                Id = 1,
                Students = new List<Student>()
            };

            // Team t already exists in the TeamRepository
            repoMock.Setup(repo => repo.GetById(It.Is<int>(id => id == t.Id))).Returns(() => t);

            TeamService service = new TeamService(repoMock.Object);

            // act + assert
            var ex = Assert.Throws<InvalidOperationException>(() => service.AddTeam(t));

            Assert.Equal("Team already in Repository", ex.Message);
            repoMock.Verify(repo => repo.Add(It.Is<Team>(team => team == t)), Times.Never);
        }

        [Fact]
        public void AddTeam_TeamIsNull_ExpectArgumentException()
        {
            // arrange
            TeamService service = new TeamService(repoMock.Object);

            // act + assert
            var ex = Assert.Throws<ArgumentException>(() => service.AddTeam(null));

            Assert.Equal("Team is missing", ex.Message);
            repoMock.Verify(repo => repo.Add(It.Is<Team>(team => team == null)), Times.Never);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void AddTeam_InvalidTeamId_ExpectArgumentException(int teamId)
        {
            // arrange
            Team t = new Team()
            {
                Id = teamId,
                Students = new List<Student>()
            };

            TeamService service = new TeamService(repoMock.Object);

            // act + assert
            var ex = Assert.Throws<ArgumentException>(() => service.AddTeam(t));

            Assert.Equal("Invalid Team Id", ex.Message);
            repoMock.Verify(repo => repo.Add(It.Is<Team>(team => team == t)), Times.Never);
        }

        [Fact]
        public void AddTeam_InvalidStudentsList_ExpectArgumentException()
        {
            // arrange
            Team t = new Team()
            {
                Id = 1,
                Students = null
            };

            TeamService service = new TeamService(repoMock.Object);

            // act + assert
            var ex = Assert.Throws<ArgumentException>(() => service.AddTeam(t));

            Assert.Equal("Invalid Students property", ex.Message);
            repoMock.Verify(repo => repo.Add(It.Is<Team>(team => team == t)), Times.Never);
        }

        #endregion

        #region UpdateTeam

        [Fact]
        public void UpdateTeam_ValidExistingTeam()
        {
            Team team = new Team()
            {
                Id = 1,
                Students = new List<Student>() { new Student() { Id = 2 } }
            };

            // Team t already exists in Team Repository
            repoMock.Setup(repo => repo.GetById(It.Is<int>(id => id == team.Id))).Returns(() => team);

            TeamService service = new TeamService(repoMock.Object);

            // act
            service.UpdateTeam(team);

            // assert
            repoMock.Verify(repo => repo.Update(It.Is<Team>(t => t == team)), Times.Once);
        }

        [Fact]
        public void UpdateTeam_NonExistingTeam_ExpectInvalidOperationException()
        {
            // arrange
            Team team = new Team()
            {
                Id = 1,
                Students = new List<Student>()
            };

            // Team t does not exist in the TeamRepository
            repoMock.Setup(repo => repo.GetById(It.Is<int>(id => id == team.Id))).Returns(() => null);

            TeamService service = new TeamService(repoMock.Object);

            // act + assert
            var ex = Assert.Throws<InvalidOperationException>(() => service.UpdateTeam(team));

            Assert.Equal("Team does not exist in the Team Repository", ex.Message);
            repoMock.Verify(repo => repo.Update(It.Is<Team>(t => t == team)), Times.Never);
        }

        [Fact]
        public void UpdateTeam_TeamIsNull_ExpectArgumentException()
        {
            // arrange
            TeamService service = new TeamService(repoMock.Object);

            // act + assert
            var ex = Assert.Throws<ArgumentException>(() => service.UpdateTeam(null));

            Assert.Equal("Team is missing", ex.Message);
            repoMock.Verify(repo => repo.Update(It.Is<Team>(team => team == null)), Times.Never);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void UpdateTeam_InvalidTeamId_ExpectArgumentException(int teamId)
        {
            // arrange
            Team t = new Team()
            {
                Id = teamId,
                Students = new List<Student>()
            };

            TeamService service = new TeamService(repoMock.Object);

            // act + assert
            var ex = Assert.Throws<ArgumentException>(() => service.UpdateTeam(t));

            Assert.Equal("Invalid Team Id", ex.Message);
            repoMock.Verify(repo => repo.Update(It.Is<Team>(team => team == t)), Times.Never);
        }

        [Fact]
        public void UpdateTeam_InvalidStudentsList_ExpectArgumentException()
        {
            // arrange
            Team t = new Team()
            {
                Id = 1,
                Students = null
            };

            TeamService service = new TeamService(repoMock.Object);

            // act + assert
            var ex = Assert.Throws<ArgumentException>(() => service.UpdateTeam(t));

            Assert.Equal("Invalid Students property", ex.Message);
            repoMock.Verify(repo => repo.Update(It.Is<Team>(team => team == t)), Times.Never);
        }

        #endregion

        #region RemoveTeam

        [Fact]
        public void RemoveTeam_TeamExists()
        {
            // arrange
            Team team = new Team()
            {
                Id = 1
            };

            // Team team exists in the TeamRepository
            repoMock.Setup(repo => repo.GetById(It.Is<int>(id => id == team.Id))).Returns(() => team);

            TeamService service = new TeamService(repoMock.Object);

            // act
            service.RemoveTeam(team);

            // assert
            repoMock.Verify(repo => repo.Remove(It.Is<Team>(t => t == team)), Times.Once);
        }

        [Fact]
        public void RemoveTeam_TeamDoesNotExist_ExpectInvalidOperationException()
        {
            // arrange
            Team team = new Team()
            {
                Id = 1
            };

            // Team team does not exist in the TeamRepository
            repoMock.Setup(repo => repo.GetById(It.Is<int>(id => id == team.Id))).Returns(() => null);

            TeamService service = new TeamService(repoMock.Object);

            // act + assert
            var ex = Assert.Throws<InvalidOperationException>(() => service.RemoveTeam(team));

            Assert.Equal("Team does not exist", ex.Message);
            repoMock.Verify(repo => repo.Remove(It.Is<Team>(t => t == team)), Times.Never);
        }

        [Fact]
        public void RemoveTeam_TeamIsNull_ExpectArgumentException()
        {
            // arrange
            TeamService service = new TeamService(repoMock.Object);

            // act + assert
            var ex = Assert.Throws<ArgumentException>(() => service.RemoveTeam(null));

            Assert.Equal("Team is missing", ex.Message);
            repoMock.Verify(repo => repo.Remove(It.Is<Team>(t => t == null)), Times.Never);
        }

        #endregion

        #region GetTeamById

        [Fact]
        public void GetTeamById_TeamExists()
        {
            // arrange
            var teamId = 1;

            var team = new Team()
            {
                Id = teamId
            };

            // Team with teamId is in the TeamRepository
            repoMock.Setup(repo => repo.GetById(It.Is<int>(id => id == teamId))).Returns(() => team);

            var service = new TeamService(repoMock.Object);

            // act
            var result = service.GetTeamById(teamId);

            // assert
            Assert.NotNull(result);
            Assert.Equal(result, team);
            repoMock.Verify(repo => repo.GetById(It.Is<int>(id => id == teamId)), Times.Once);
        }

        [Fact]
        public void GetTeamById_TeamDoesNotExists_ExpectNull()
        {
            // arrange
            var teamId = 1;

            var team = new Team()
            {
                Id = teamId
            };

            // Team with teamId is in the TeamRepository
            repoMock.Setup(repo => repo.GetById(It.Is<int>(id => id == teamId))).Returns(() => null);

            var service = new TeamService(repoMock.Object);

            // act
            var result = service.GetTeamById(teamId);

            // assert
            Assert.Null(result);
            repoMock.Verify(repo => repo.GetById(It.Is<int>(id => id == teamId)), Times.Once);
        }

        #endregion

        #region GetAllTeams

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public void GetAllTeams(int teamCount)
        {
            // arrange
            var allTeams = new List<Team>()
            {
                new Team(){ Id = 1},
                new Team(){ Id = 2},
                new Team(){ Id = 3}
            };

            var teams = allTeams.GetRange(0, teamCount);

            // The repository returns the number of teams stated by the teamCount
            repoMock.Setup(repo => repo.GetAll()).Returns(() => teams.ToArray());

            var service = new TeamService(repoMock.Object);

            // act
            var result = service.GetAllTeams();

            // assert
            Assert.Equal(teams, result);
            repoMock.Verify(repo => repo.GetAll(), Times.Once);
        }

        #endregion
    }
}
