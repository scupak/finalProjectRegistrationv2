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
    }
}
