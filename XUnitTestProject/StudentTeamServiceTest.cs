using Interfaces;
using Moq;
using Services;
using System;
using System.Collections.Generic;
using System.Text;
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
    }
}
