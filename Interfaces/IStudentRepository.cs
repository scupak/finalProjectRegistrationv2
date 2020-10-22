using Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Interfaces
{
    public interface IStudentRepository
    {
        IEnumerable<Student> GetAll();
        Student GetById(int id);
        void Add(Student s);
        void Update(Student s);
        void Remove(Student s);
    }
}
