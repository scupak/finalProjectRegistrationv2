using Interfaces;
using Model;
using System;

namespace Services
{
    public class CustomerService
    {
        private readonly ICustomerRepository customerRepository;

        public CustomerService(ICustomerRepository repo)
        {
            if (repo == null)
            {
                throw new ArgumentException("Customer Repository is missing");
            }
            customerRepository = repo;
        }

        public void AddCustomer(Customer customer)
        {
            if (customer == null)
            {
                throw new ArgumentException("Customer is missing");
            }
            if (!IsValidCustomer(customer))
            {
                throw new ArgumentException("Invalid Customer property");
            }
            if (customerRepository.GetById(customer.Id) != null)
            {
                throw new InvalidOperationException("Customer already exists");
            }

            customerRepository.Add(customer);
        }

        private bool IsValidCustomer(Customer c)
        {
            return
                !(c.Id <= 0
                || String.IsNullOrEmpty(c.Name)
                || String.IsNullOrEmpty(c.Address)
                || c.Zipcode <= 0
                || String.IsNullOrEmpty(c.PostalDistrict)
                || c.CompanyUri == "");
        }
    }
}
