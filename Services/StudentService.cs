using Interfaces;
using System;

namespace Services
{
    public class StudentService
    {
        private IStudentRepository repo;

        public StudentService(IStudentRepository repo)
        {
            this.repo = repo ?? throw new ArgumentException("StudentRepository is missing");
        }
    }
}
