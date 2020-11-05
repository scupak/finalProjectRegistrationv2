using Interfaces;
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
    }
}
