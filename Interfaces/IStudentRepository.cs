using Model;
using System.Collections.Generic;

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
