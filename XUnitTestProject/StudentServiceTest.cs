using Interfaces;
using Model;
using Moq;
using Services;
using System;
using System.Net.Sockets;
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

        #region CreateStudentService

        [Fact]
        public void CreateStudentService_WithValidRepository()
        {
            // arrange
            var repo = repoMock.Object;

            // act
            var service = new StudentService(repo);

            // assert
            Assert.NotNull(service);
        }

        [Fact]
        public void CreateStudentService_RepositoryIsNull_ExpectArgumentException()
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

        #endregion

        #region AddStudent

        [Theory]
        [InlineData(1, "Name", "Address", 1234, "PostalDistrict", "e@mail.dk")]
        [InlineData(1, "Name", "Address", 1234, "PostalDistrict", null)]
        public void AddStudent_ValidStudentNonExisting(int id, string name, string address, int zipcode, string postalDistrict, string email)
        {
            // arrange
            Student student = new Student()
            {
                Id = id,
                Name = name,
                Address = address,
                ZipCode = zipcode,
                PostalDistrict = postalDistrict,
                Email = email
            };

            // make sure the student does not exist before test
            repoMock.Setup(repo => repo.GetById(It.Is<int>(z => z == student.Id))).Returns(() => null);

            StudentService service = new StudentService(repoMock.Object);

            // act
            service.AddStudent(student);

            // assert
            repoMock.Verify(repo => repo.Add(It.Is<Student>(s => s == student)), Times.Once);
        }

        [Fact]
        public void AddStudent_StudentIsNull_ExpectArgumentException()
        {
            // arrange
            var service = new StudentService(repoMock.Object);

            // act + assert
            var ex = Assert.Throws<ArgumentException>(() => service.AddStudent(null));

            Assert.Equal("Student is missing", ex.Message);
            repoMock.Verify(repo => repo.Add(It.Is<Student>(s => s == null)), Times.Never);
        }

        [Theory]
        [InlineData(0, "Name", "Address", 1234, "PostalDistrict", "e@mail.dk", "Id")]     // invalid Id = 0
        [InlineData(-1, "Name", "Address", 1234, "PostalDistrict", "e@mail.dk", "Id")]    // invalid Id = -1
        [InlineData(1, null, "Address", 1234, "PostalDistrict", "e@mail.dk", "Name")]     // Name is null
        [InlineData(1, "", "Address", 1234, "PostalDistrict", "e@mail.dk", "Name")]       // Name is empty
        [InlineData(1, "Name", null, 1234, "PostalDistrict", "e@mail.dk", "Address")]     // Address is null
        [InlineData(1, "Name", "", 1234, "PostalDistrict", "e@mail.dk", "Address")]       // Address is empty
        [InlineData(1, "Name", "Address", 0, "PostalDistrict", "e@mail.dk", "ZipCode")]   // invalid ZipCode = 0
        [InlineData(1, "Name", "Address", -1, "PostalDistrict", "e@mail.dk", "ZipCode")]  // invalid ZipCode = -1
        [InlineData(1, "Name", "Address", 1234, null, "e@mail.dk", "PostalDistrict")]     // PostalDistrict is null
        [InlineData(1, "Name", "Address", 1234, "", "e@mail.dk", "PostalDistrict")]       // PostalDistrict is empty
        [InlineData(1, "Name", "Address", 1234, "PostalDistrict", "", "Email")]           // Email is empty
        public void AddStudent_InvalidStudent_ExpectArgumentException(int id, string name, string address, int zipcode, string postalDistrict, string email, string field)
        {
            // arrange
            Student student = new Student()
            {
                Id = id,
                Name = name,
                Address = address,
                ZipCode = zipcode,
                PostalDistrict = postalDistrict,
                Email = email
            };

            // make sure the student does not exist before test
            repoMock.Setup(repo => repo.GetById(It.Is<int>(z => z == student.Id))).Returns(() => null);

            StudentService service = new StudentService(repoMock.Object);

            // act + assert
            var ex = Assert.Throws<ArgumentException>(() => service.AddStudent(student));

            Assert.Equal($"Invalid Student Property: {field}", ex.Message);
            repoMock.Verify(repo => repo.Add(It.Is<Student>(s => s == student)), Times.Never);
        }

        [Fact]
        public void AddStudent_StudentExists_ExpectInvalidOperationException()
        {
            // arrange
            Student student = new Student()
            {
                Id = 1,
                Name = "name",
                Address = "address",
                ZipCode = 1234,
                PostalDistrict = "postalDistrict",
                Email = "email"
            };

            // make sure the student exists before test
            repoMock.Setup(repo => repo.GetById(It.Is<int>(z => z == student.Id))).Returns(() => student);

            StudentService service = new StudentService(repoMock.Object);

            // act + assert
            var ex = Assert.Throws<InvalidOperationException>(() => service.AddStudent(student));

            Assert.Equal("Student already exist", ex.Message);
            repoMock.Verify(repo => repo.Add(It.Is<Student>(s => s == student)), Times.Never);
        }
        #endregion

        #region UpdateStudent



        #endregion
    }
}
