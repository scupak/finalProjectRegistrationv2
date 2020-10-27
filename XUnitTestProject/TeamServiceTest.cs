using Interfaces;
using Moq;
using Services;
using System;
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
    }
}
