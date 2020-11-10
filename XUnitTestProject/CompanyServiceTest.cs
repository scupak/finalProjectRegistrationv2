using Interfaces;
using Model;
using Moq;
using Services;
using System;
using System.Collections.Generic;
using Xunit;

namespace XUnitTestProject
{

    public class CompanyServiceTest
    {
        private SortedDictionary<int, Company> allCompanies;
        private Mock<ICompanyRepository> repoMock;

        public CompanyServiceTest()
        {
            allCompanies = new SortedDictionary<int, Company>();
            repoMock = new Mock<ICompanyRepository>();
            repoMock.Setup(repo => repo.Add(It.IsAny<Company>())).Callback<Company>(c => allCompanies.Add(c.Id, c));
            repoMock.Setup(repo => repo.Update(It.IsAny<Company>())).Callback<Company>(c => allCompanies[c.Id] = c);
            repoMock.Setup(repo => repo.Remove(It.IsAny<Company>())).Callback<Company>(c => allCompanies.Remove(c.Id));
            repoMock.Setup(repo => repo.GetAll()).Returns(() => allCompanies.Values);
            repoMock.Setup(repo => repo.GetById(It.IsAny<int>())).Returns<int>((id) => allCompanies.ContainsKey(id) ? allCompanies[id] : null);
        }

        #region CreateCompanyService

        [Fact]
        public void CreateCompanyService_ValidCompanyRepository()
        {
            // arrange
            ICompanyRepository repo = repoMock.Object;

            // act
            CompanyService service = new CompanyService(repo);

            // assert
            Assert.NotNull(service);
        }

        [Fact]
        public void CreateCompanyService_CompanyRepositoryIsNull_ExpectArgumentException()
        {
            // arrange
            CompanyService service = null;

            // act + assert
            var ex = Assert.Throws<ArgumentException>(() => service = new CompanyService(null)); 

            Assert.Equal("Company Repository is missing", ex.Message);
        }

        #endregion

        #region AddCompany

        [Theory]
        [InlineData(1, "Company", "Address", 1234, "District", "uri")]
        [InlineData(1, "Company", "Address", 1234, "District", null)]
        public void AddCompany_ValidCompany(int id, string name, string address, int zipcode, string district, string uri)
        {
            // arrange
            var Company = new Company()
            {
                Id = id,
                Name = name,
                Address = address,
                Zipcode = zipcode,
                PostalDistrict = district,
                CompanyUri = uri,
                Projects = new List<Project>()
            };

            CompanyService service = new CompanyService(repoMock.Object);

            // act
            service.AddCompany(Company);

            // assert
            Assert.Contains(Company, allCompanies.Values);
            repoMock.Verify(repo => repo.Add(It.Is<Company>(c => c == Company)), Times.Once);
        }

        [Theory]
        [InlineData(0, "Company", "Address", 1234, "District", "uri")]      // invalid id: 0
        [InlineData(-1, "Company", "Address", 1234, "District", "uri")]     // invalid id: -1
        [InlineData(1, null, "Address", 1234, "District", "uri")]           // invalid name: null
        [InlineData(1, "", "Address", 1234, "District", "uri")]             // invalid name: ""
        [InlineData(1, "Company", null, 1234, "District", "uri")]           // invalid address: null
        [InlineData(1, "Company", "", 1234, "District", "uri")]             // invalid address: ""
        [InlineData(1, "Company", "Address", 0, "District", "uri")]         // invalid zipcode: 0
        [InlineData(1, "Company", "Address", -1, "District", "uri")]        // invalid zipcode: -1
        [InlineData(1, "Company", "Address", 1234, null, "uri")]            // invalid PostalDistrict: null
        [InlineData(1, "Company", "Address", 1234, "", "uri")]              // invalid PostalDistrict: ""
        [InlineData(1, "Company", "Address", 1234, "District", "")]         // invaild CompanyUri: ""
        public void AddCompany_InvalidCompany(int id, string name, string address, int zipcode, string district, string uri)
        {
            // arrange
            var Company = new Company()
            {
                Id = id,
                Name = name,
                Address = address,
                Zipcode = zipcode,
                PostalDistrict = district,
                CompanyUri = uri,
                Projects = new List<Project>()
            };

            CompanyService service = new CompanyService(repoMock.Object);

            // act + assert
            var ex = Assert.Throws<ArgumentException>(() => service.AddCompany(Company));

            Assert.Equal("Invalid Company property", ex.Message);
            repoMock.Verify(repo => repo.Add(It.Is<Company>(c => c == Company)), Times.Never);
        }

        [Fact]
        public void AddCompany_CompanyIsNUll_ExpectArgumentException()
        {
            // arrange
            CompanyService service = new CompanyService(repoMock.Object);

            // act + assert
            var ex = Assert.Throws<ArgumentException>(() => service.AddCompany(null));

            Assert.Equal("Company is missing", ex.Message);
            repoMock.Verify(repo => repo.Add(It.Is<Company>(c => c == null)), Times.Never);
        }

        [Fact]
        public void AddCompany_CompanyAlreadyExists_InvalidOperationException()
        {
            // arrange
            
            var repo = repoMock.Object;

            // Company exists in Company repository
            var Company = new Company()
            {
                Id = 1,
                Name = "Company",
                Address = "Address",
                Zipcode = 1234,
                PostalDistrict = "District",
                CompanyUri = "companyUrl",
                Projects = new List<Project>()
            };
            allCompanies.Add(Company.Id, Company);

            var service = new CompanyService(repo);

            // act + assert

            var ex = Assert.Throws<InvalidOperationException>(() => service.AddCompany(Company));

            Assert.Equal("Company already exists", ex.Message);
            repoMock.Verify(repo => repo.Add(It.Is<Company>(c => c == Company)), Times.Never);
        }

        #endregion

        #region UpdateCompany

        [Theory]
        [InlineData(1, "Company", "Address", 1234, "District", "uri")]
        [InlineData(1, "Company", "Address", 1234, "District", null)]
        public void UpdateCompany_ValidExistingCompany(int id, string name, string address, int zipcode, string postalDistrict, string url)
        {
            // arrange
            var company = new Company()
            {
                Id = id,
                Name = name,
                Address = address,
                Zipcode = zipcode,
                PostalDistrict = postalDistrict,
                CompanyUri = url,
                Projects = new List<Project>()
            };

            // company is valid and exists in company repository
            allCompanies.Add(company.Id, company);

            var service = new CompanyService(repoMock.Object);

            // act
            service.UpdateCompany(company);

            // assert
            Assert.Equal(repoMock.Object.GetById(company.Id), company);
            repoMock.Verify(repo => repo.Update(It.Is<Company>(c => c == company)), Times.Once);
        }


        [Theory]
        [InlineData(0, "Company", "Address", 1234, "District", "uri")]      // invalid id: 0
        [InlineData(-1, "Company", "Address", 1234, "District", "uri")]     // invalid id: -1
        [InlineData(1, null, "Address", 1234, "District", "uri")]           // invalid name: null
        [InlineData(1, "", "Address", 1234, "District", "uri")]             // invalid name: ""
        [InlineData(1, "Company", null, 1234, "District", "uri")]           // invalid address: null
        [InlineData(1, "Company", "", 1234, "District", "uri")]             // invalid address: ""
        [InlineData(1, "Company", "Address", 0, "District", "uri")]         // invalid zipcode: 0
        [InlineData(1, "Company", "Address", -1, "District", "uri")]        // invalid zipcode: -1
        [InlineData(1, "Company", "Address", 1234, null, "uri")]            // invalid PostalDistrict: null
        [InlineData(1, "Company", "Address", 1234, "", "uri")]              // invalid PostalDistrict: ""
        [InlineData(1, "Company", "Address", 1234, "District", "")]         // invaild CompanyUri: ""
        public void UpdateCompany_InvalidCompany_ExpectArgumentException(int id, string name, string address, int zipcode, string district, string uri)
        {
            // arrange
            var Company = new Company()
            {
                Id = id,
                Name = name,
                Address = address,
                Zipcode = zipcode,
                PostalDistrict = district,
                CompanyUri = uri,
                Projects = new List<Project>()
            };

            CompanyService service = new CompanyService(repoMock.Object);

            // act + assert
            var ex = Assert.Throws<ArgumentException>(() => service.UpdateCompany(Company));

            Assert.Equal("Invalid Company property", ex.Message);
            repoMock.Verify(repo => repo.Update(It.Is<Company>(c => c == Company)), Times.Never);
        }

        [Fact]
        public void UpdateCompany_CompanyIsNUll_ExpectArgumentException()
        {
            // arrange
            CompanyService service = new CompanyService(repoMock.Object);

            // act + assert
            var ex = Assert.Throws<ArgumentException>(() => service.UpdateCompany(null));

            Assert.Equal("Company is missing", ex.Message);
            repoMock.Verify(repo => repo.Update(It.Is<Company>(c => c == null)), Times.Never);
        }

        [Fact]
        public void UpdateCompany_CompanyDoesNotExist_InvalidOperationException()
        {
            // arrange
            
            var repo = repoMock.Object;

            // Company does not exist in Company repository
            var Company = new Company()
            {
                Id = 1,
                Name = "Company",
                Address = "Address",
                Zipcode = 1234,
                PostalDistrict = "District",
                CompanyUri = "companyUrl",
                Projects = new List<Project>()
            };

            var service = new CompanyService(repo);

            // act + assert
            var ex = Assert.Throws<InvalidOperationException>(() => service.UpdateCompany(Company));

            Assert.Equal("Company does not exist", ex.Message);
            repoMock.Verify(repo => repo.Update(It.Is<Company>(c => c == Company)), Times.Never);
        }

        #endregion
       
        #region RemoveCompany

        [Fact]
        public void RemoveCompany_ValidExistingCompany()
        {
            // arrange

            // company exists in Company Repository before test
            var company = new Company()
            {
                Id = 1,
                Name = "name",
                Address = "address",
                Zipcode = 1234,
                PostalDistrict = "postalDistrict",
                CompanyUri = "url",
                Projects = new List<Project>()
            };
            allCompanies.Add(company.Id, company);

            var service = new CompanyService(repoMock.Object);

            // act
            service.RemoveCompany(company);

            // assert
            Assert.Null(repoMock.Object.GetById(company.Id));
            repoMock.Verify(repo => repo.Remove(It.Is<Company>(c => c.Id == company.Id)), Times.Once);
        }

        [Fact]
        public void RemoveCompany_CompanyDoesNotExist_ExpectInvalidOperationException()
        {
            // arrange

            // company does not exist in Company Repository before test
            var company = new Company()
            {
                Id = 1,
                Name = "name",
                Address = "address",
                Zipcode = 1234,
                PostalDistrict = "postalDistrict",
                CompanyUri = "url",
                Projects = new List<Project>()
            };

            var service = new CompanyService(repoMock.Object);

            // act + assert
            var ex = Assert.Throws<InvalidOperationException>(() => service.RemoveCompany(company));

            // assert
            Assert.Equal("Company does not exist", ex.Message);
            repoMock.Verify(repo => repo.Remove(It.Is<Company>(c => c.Id == company.Id)), Times.Never);
        }

        [Fact]
        public void RemoveCompany_CompanyIsNull_ExpectArgumentException()
        {
            // arrange

            var service = new CompanyService(repoMock.Object);

            // act + assert
            var ex = Assert.Throws<ArgumentException>(() => service.RemoveCompany(null));

            // assert
            Assert.Equal("Company is missing", ex.Message);
            repoMock.Verify(repo => repo.Remove(It.Is<Company>(c => c == null)), Times.Never);
        }

        #endregion

        #region GetAllCompanies

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public void GetAllCompanies(int companyCount)
        {
            // arrange
            Company c1 = new Company(){ Id = 1};
            Company c2 = new Company(){ Id = 2}; 
            var companies = new List<Company>(){ c1, c2};

            // the companies in the repository
            var expected = companies.GetRange(0,companyCount);
            foreach (var c in expected)
            { 
                allCompanies.Add(c.Id, c);
            }

            var service = new CompanyService(repoMock.Object);

            // act
            var result = service.GetAllCompanies();

            // assert
            Assert.Equal(expected, result);
            repoMock.Verify(repo => repo.GetAll(), Times.Once);
        }

        #endregion
    }
}
