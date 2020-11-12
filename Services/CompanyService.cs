using Interfaces;
using Model;
using System;
using System.Collections.Generic;

namespace Services
{
    public class CompanyService
    {
        private readonly ICompanyRepository companyRepository;

        public CompanyService(ICompanyRepository repo)
        {
            companyRepository = repo ?? throw new ArgumentException("Company Repository is missing");
        }

        public void AddCompany(Company company)
        {
            if (company == null)
            {
                throw new ArgumentException("Company is missing");
            }
            if (!IsValidCompany(company))
            {
                throw new ArgumentException("Invalid Company property");
            }
            if (companyRepository.GetById(company.Id) != null)
            {
                throw new InvalidOperationException("Company already exists");
            }

            companyRepository.Add(company);
        }

        private bool IsValidCompany(Company c)
        {
            return
                !(c.Id <= 0
                || String.IsNullOrEmpty(c.Name)
                || String.IsNullOrEmpty(c.Address)
                || c.Zipcode <= 0
                || String.IsNullOrEmpty(c.PostalDistrict)
                || c.CompanyUri == ""
                || c.Projects == null);
        }

        public void UpdateCompany(Company company)
        {
            if (company == null)
            {
                throw new ArgumentException("Company is missing");
            }
            if (!IsValidCompany(company))
            {
                throw new ArgumentException("Invalid Company property");
            }
            if (companyRepository.GetById(company.Id) == null)
            {
                throw new InvalidOperationException("Company does not exist");
            }

            companyRepository.Update(company);
        }

        public void RemoveCompany(Company company)
        {
            if (company == null)
            {
                throw new ArgumentException("Company is missing");
            }
            if (companyRepository.GetById(company.Id) == null)
            {
                throw new InvalidOperationException("Company does not exist");
            }
            companyRepository.Remove(company);
        }

        public IEnumerable<Company> GetAllCompanies()
        {
            return companyRepository.GetAll();
        }

        public Company GetCompanyById(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Invalid Company id");
            }
            return companyRepository.GetById(id);
        }
    }
}
