using Interfaces;
using Model;
using Moq;
using Services;
using System;
using System.Collections.Generic;
using Xunit;

namespace XUnitTestProject
{

    public class CustomerServiceTest
    {
        private SortedDictionary<int, Customer> allCustomers;
        private Mock<ICustomerRepository> repoMock;

        public CustomerServiceTest()
        {
            allCustomers = new SortedDictionary<int, Customer>();
            repoMock = new Mock<ICustomerRepository>();
            repoMock.Setup(repo => repo.Add(It.IsAny<Customer>())).Callback<Customer>(c => allCustomers.Add(c.Id, c));
            repoMock.Setup(repo => repo.Update(It.IsAny<Customer>())).Callback<Customer>(c => allCustomers[c.Id] = c);
            repoMock.Setup(repo => repo.Remove(It.IsAny<Customer>())).Callback<Customer>(c => allCustomers.Remove(c.Id));
            repoMock.Setup(repo => repo.GetAll()).Returns(() => allCustomers.Values);
            repoMock.Setup(repo => repo.GetById(It.IsAny<int>())).Returns<int>((id) => allCustomers.ContainsKey(id) ? allCustomers[id] : null);
        }


        #region CreateCustomerService
        [Fact]
        public void CreateCustomerService_ValidCustomerRepository()
        {
            // arrange
            ICustomerRepository repo = repoMock.Object;

            // act
            CustomerService service = new CustomerService(repo);

            // assert
            Assert.NotNull(service);
        }

        [Fact]
        public void CreateCustomerService_CustomerRepositoryIsNull_ExpectArgumentException()
        {
            // arrange
            CustomerService service = null;

            // act + assert
            var ex = Assert.Throws<ArgumentException>(() => service = new CustomerService(null));

            Assert.Equal("Customer Repository is missing", ex.Message);
        }

        #endregion

        #region AddCustomer

        [Theory]
        [InlineData(1, "Company", "Address", 1234, "District", "uri")]
        [InlineData(1, "Company", "Address", 1234, "District", null)]
        public void AddCustomer_ValidCustomer(int id, string name, string address, int zipcode, string district, string uri)
        {
            // arrange
            var customer = new Customer()
            {
                Id = id,
                Name = name,
                Address = address,
                Zipcode = zipcode,
                PostalDistrict = district,
                CompanyUri = uri,
                Projects = new List<Project>()
            };

            CustomerService service = new CustomerService(repoMock.Object);

            // act
            service.AddCustomer(customer);

            // assert
            Assert.Contains(customer, allCustomers.Values);
            repoMock.Verify(repo => repo.Add(It.Is<Customer>(c => c == customer)), Times.Once);
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
        public void AddCustomer_InvalidCustomer(int id, string name, string address, int zipcode, string district, string uri)
        {
            // arrange
            var customer = new Customer()
            {
                Id = id,
                Name = name,
                Address = address,
                Zipcode = zipcode,
                PostalDistrict = district,
                CompanyUri = uri,
                Projects = new List<Project>()
            };

            CustomerService service = new CustomerService(repoMock.Object);

            // act + assert
            var ex = Assert.Throws<ArgumentException>(() => service.AddCustomer(customer));

            Assert.Equal("Invalid Customer property", ex.Message);
            repoMock.Verify(repo => repo.Add(It.Is<Customer>(c => c == customer)), Times.Never);
        }

        [Fact]
        public void AddCustomer_CustomerIsNUll_ExpectArgumentException()
        {
            // arrange
            CustomerService service = new CustomerService(repoMock.Object);

            // act + assert
            var ex = Assert.Throws<ArgumentException>(() => service.AddCustomer(null));

            Assert.Equal("Customer is missing", ex.Message);
            repoMock.Verify(repo => repo.Add(It.Is<Customer>(c => c == null)), Times.Never);
        }

        [Fact]
        public void AddCustomer_CustomerAlreadyExists_InvalidOperationException()
        {
            // arrange
            
            var repo = repoMock.Object;

            // customer exists in customer repository
            var customer = new Customer()
            {
                Id = 1,
                Name = "Company",
                Address = "Address",
                Zipcode = 1234,
                PostalDistrict = "District",
                CompanyUri = "companyUrl"
            };
            allCustomers.Add(customer.Id, customer);

            var service = new CustomerService(repo);

            // act + assert

            var ex = Assert.Throws<InvalidOperationException>(() => service.AddCustomer(customer));

            Assert.Equal("Customer already exists", ex.Message);
            repoMock.Verify(repo => repo.Add(It.Is<Customer>(c => c == customer)), Times.Never);
        }

        #endregion

    }
}
