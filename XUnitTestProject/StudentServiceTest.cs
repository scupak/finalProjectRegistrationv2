using Interfaces;
using Model;
using Moq;
using Services;
using System;
using System.Collections.Generic;
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
        public void AddStudent_ValidNonExistingStudent(int id, string name, string address, int zipcode, string postalDistrict, string email)
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

        [Theory]
        [InlineData(1, "Name", "Address", 1234, "PostalDistrict", "e@mail.dk")]
        [InlineData(1, "Name", "Address", 1234, "PostalDistrict", null)]
        public void UpdateStudent_ValidExistingStudent(int id, string name, string address, int zipcode, string postalDistrict, string email)
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

            // make sure the student exists before test
            repoMock.Setup(repo => repo.GetById(It.Is<int>(z => z == student.Id))).Returns(() => student);

            StudentService service = new StudentService(repoMock.Object);

            // act
            service.UpdateStudent(student);

            // assert
            repoMock.Verify(repo => repo.Update(It.Is<Student>(s => s == student)), Times.Once);
        }

        [Fact]
        public void UpdateStudent_StudentIsNull_ExpectArgumentException()
        {
            // arrange
            StudentService service = new StudentService(repoMock.Object);

            // act + assert
            var ex = Assert.Throws<ArgumentException>(() => service.UpdateStudent(null));

            Assert.Equal("Student is missing", ex.Message);
            repoMock.Verify(repo => repo.Update(It.Is<Student>(s => s == null)), Times.Never);
        }

        [Fact]
        public void UpdateStudent_NonExistingStudent_ExpectInvalidOperationException()
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

            // make sure the student does not exist before test
            repoMock.Setup(repo => repo.GetById(It.Is<int>(z => z == student.Id))).Returns(() => null);

            StudentService service = new StudentService(repoMock.Object);

            // act + assert
            var ex = Assert.Throws<InvalidOperationException>(() => service.UpdateStudent(student));

            Assert.Equal("Update of non-existing student", ex.Message);
            repoMock.Verify(repo => repo.Update(It.Is<Student>(s => s == student)), Times.Never);
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
        public void UpdateStudent_InvalidStudent_ExpectArgumentException(int id, string name, string address, int zipcode, string postalDistrict, string email, string field)
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

            // make sure the student exists before test
            repoMock.Setup(repo => repo.GetById(It.Is<int>(z => z == student.Id))).Returns(() => student);

            StudentService service = new StudentService(repoMock.Object);

            // act + assert
            var ex = Assert.Throws<ArgumentException>(() => service.UpdateStudent(student));

            Assert.Equal($"Invalid Student Property: {field}", ex.Message);
            repoMock.Verify(repo => repo.Update(It.Is<Student>(s => s == student)), Times.Never);
        }

        #endregion

        #region RemoveStudent

        [Fact]
        public void RemoveStudent_ExistingStudent()
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

            // act
            service.RemoveStudent(student);

            // assert
            repoMock.Verify(repo => repo.Remove(It.Is<Student>(s => s == student)), Times.Once);
        }

        [Fact]
        public void RemoveStudent_StudentIsNull_ExpectArgumentException()
        {
            // arrange
            StudentService service = new StudentService(repoMock.Object);

            // act + assert
            var ex = Assert.Throws<ArgumentException>(() => service.RemoveStudent(null));

            Assert.Equal("Student is missing", ex.Message);
            repoMock.Verify(repo => repo.Remove(It.Is<Student>(s => s == null)), Times.Never);
        }

        [Fact]
        public void RemoveStudent_NonExistingStudent_ExpectInvalidOperationException()
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

            // make sure the student does not exist before test
            repoMock.Setup(repo => repo.GetById(It.Is<int>(z => z == student.Id))).Returns(() => null);

            StudentService service = new StudentService(repoMock.Object);

            // act + assert
            var ex = Assert.Throws<InvalidOperationException>(() => service.RemoveStudent(student));

            // assert
            Assert.Equal("Attempt to remove non-existing student", ex.Message);
            repoMock.Verify(repo => repo.Remove(It.Is<Student>(s => s == student)), Times.Never);
        }

        #endregion

        #region GetAllStudents
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public void GetAllStudents(int listCount)
        {
            // arrange
            var data = new List<Student>()
            {
                new Student() { Id = 1},
                new Student() { Id = 2}
            };

            repoMock.Setup(x => x.GetAll()).Returns(() => data.GetRange(0, listCount));

            StudentService service = new StudentService(repoMock.Object);

            // act
            var result = service.GetAllStudents();

            // assert
            Assert.Equal(data.GetRange(0, listCount), result);
            repoMock.Verify(repo => repo.GetAll(), Times.Once);
        }
        #endregion

        #region GetStudentById

        [Fact]
        public void GetStudentById_existingStudent()
        {
            // arrange
            int id = 1;

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

            // act
            var result = service.GetStudentById(id);

            // assert
            Assert.Equal(student, result);
            repoMock.Verify(repo => repo.GetById(It.Is<int>(x => x == id)), Times.Once);
        }

        [Fact]
        public void GetStudentById_NonExistingStudent_ExpectNull()
        {
            // arrange
            int id = 1;

            // make sure the student does not exist before test
            repoMock.Setup(repo => repo.GetById(It.Is<int>(z => z == id))).Returns(() => null);

            StudentService service = new StudentService(repoMock.Object);

            // act
            var result = service.GetStudentById(id);

            // assert
            Assert.Null(result);
            repoMock.Verify(repo => repo.GetById(It.Is<int>(x => x == id)), Times.Once);
        }

        #endregion
    }
}
