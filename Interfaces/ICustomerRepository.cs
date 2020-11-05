using Model;
using System.Collections.Generic;

namespace Interfaces
{
    public interface ICustomerRepository
    {
        void Add(Customer c);
        void Update(Customer c);
        void Remove(Customer c);
        IEnumerable<Customer> GetAll();
        Customer GetById(int id);
    }
}
