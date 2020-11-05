using Model;
using System.Collections.Generic;

namespace Interfaces
{
    public interface ICompanyRepository
    {
        void Add(Company c);
        void Update(Company c);
        void Remove(Company c);
        IEnumerable<Company> GetAll();
        Company GetById(int id);
    }
}
