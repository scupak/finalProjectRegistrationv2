using Interfaces;
using Model;
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

        public void AddStudent(Student student)
        {
            ThrowIfNotValidStudent(student);
            if (repo.GetById(student.Id) != null)
            {
                throw new InvalidOperationException("Student already exist");
            }
            repo.Add(student);
        }

        private void ThrowIfNotValidStudent(Student student)
        {
            if (student == null)
                throw new ArgumentException("Student is missing");
            if (student.Id <= 0)
                throw new ArgumentException("Invalid Student Property: Id");
            if (student.Name == null || student.Name == "")
                throw new ArgumentException("Invalid Student Property: Name");
            if (student.Address == null || student.Address == "")
                throw new ArgumentException("Invalid Student Property: Address");
            if (student.ZipCode <= 0)
                throw new ArgumentException("Invalid Student Property: ZipCode");
            if (student.PostalDistrict == null || student.PostalDistrict == "")
                throw new ArgumentException("Invalid Student Property: PostalDistrict");
            if (student.Email == "")
                throw new ArgumentException("Invalid Student Property: Email");
        }
    }
}
