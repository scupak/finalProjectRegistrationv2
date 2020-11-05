using System.Collections.Generic;

namespace Model
{
    public class Company
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public int Zipcode { get; set; }
        public string PostalDistrict { get; set; }

        public string CompanyUri { get; set; }

        public List<Project> Projects { get; set;}
    }
}
