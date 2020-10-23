using Interfaces;
using Model;
using System;
using System.Collections;
using System.Collections.Generic;

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

        public void UpdateStudent(Student student)
        {
            if (student == null)
            {
                throw new ArgumentException("Student is missing");
            }
            if (repo.GetById(student.Id) == null)
            {
                throw new InvalidOperationException("Update of non-existing student");
            }

            ThrowIfNotValidStudent(student);
            repo.Update(student);
        }
       
        public void RemoveStudent(Student student)
        {
            if (student == null)
            {
                throw new ArgumentException("Student is missing");
            }
            if (repo.GetById(student.Id) == null)
            {
                throw new InvalidOperationException("Attempt to remove non-existing student");
            }
            repo.Remove(student);
        }

        public IEnumerable<Student> GetAllStudents()
        {
            return repo.GetAll();
        }

        public Student GetStudentById(int id)
        {
            return repo.GetById(id);
        }
    }
}
