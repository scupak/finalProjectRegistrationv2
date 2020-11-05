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

        #region CreateCustomerService

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

    }
}
