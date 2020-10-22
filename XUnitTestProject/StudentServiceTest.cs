using Interfaces;
using Moq;
using Services;
using System;
using Xunit;

namespace XUnitTestProject
{
    public class StudentServiceTest
    {
        private Mock<IStudentRepository> repoMock;

        public StudentServiceTest()
        {
            repoMock = new Mock<IStudentRepository>();
        }

        [Fact]
        public void CreateStudentServiceWithValidRepository()
        {
            // arrange
            var repo = repoMock.Object;

            // act
            var service = new StudentService(repo);
            
            // assert
            Assert.NotNull(service);
        }

        [Fact]
        public void CreateStudentServiceRepositoryIsNullExpectArgumentException()
        {
            // arrange
            StudentService service = null;

            // act + assert
            var ex = Assert.Throws<ArgumentException>(() => 
            {
                service = new StudentService(null);
            });

            Assert.Equal("StudentRepository is missing", ex.Message);
            Assert.Null(service);
        }
    }
}
