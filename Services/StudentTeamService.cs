using Interfaces;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Services
{
    public class StudentTeamService
    {
        public const int MAX_STUDENTS = 4;

        private readonly IStudentRepository StudentsRepo;
        private readonly ITeamRepository TeamsRepo;

        public StudentTeamService(IStudentRepository studentRepository, ITeamRepository teamRepository)
        {
            this.StudentsRepo = studentRepository ?? throw new ArgumentException("StudentRepository is missing");
            this.TeamsRepo = teamRepository ?? throw new ArgumentException("TeamRepository is missing");
        }

        public void AddStudentToTeam(Team team, Student student)
        {
            if (team == null)
            {
                throw new ArgumentException("Team is missing");
            }
            
            if (student == null)
            {
                throw new ArgumentException("Student is missing");
            }

            var fetchedTeam = TeamsRepo.GetById(team.Id);
            
            if (fetchedTeam == null)
            {
                throw new ArgumentException("Team not found");
            }

            if (fetchedTeam.Students.Count == MAX_STUDENTS)
            {
                throw new InvalidOperationException("Team is full");
            }
            
            if (IsAssignedToTeam(student))
            {
                throw new InvalidOperationException("Student is already assigned to a team");
            }
            
            var fetchedStudent = StudentsRepo.GetById(student.Id);
            
            if (fetchedStudent == null)
            {
                throw new ArgumentException("Student not found");
            }

            fetchedTeam.Students.Add(fetchedStudent);
            TeamsRepo.Update(fetchedTeam);
        }

        private bool IsAssignedToTeam(Student student)
        {
            foreach (var team in TeamsRepo.GetAll())
            {
                if (team.Students.Contains(student))
                    return true;
            }
            return false;
        }

        public void MoveStudentToNewTeam(Team fromTeam, Team toTeam, Student student)
        {
            if (fromTeam == null)
            {
                throw new ArgumentException("From Team is missing");
            }
            if (toTeam == null)
            {
                throw new ArgumentException("To Team is missing");
            }
            if (student == null)
            {
                throw new ArgumentException("Student is missing");
            }

            var fetchedFromTeam = TeamsRepo.GetById(fromTeam.Id);

            if (fetchedFromTeam == null)
            {
                throw new InvalidOperationException("From Team not found");
            }

            var fetchedToTeam = TeamsRepo.GetById(toTeam.Id);

            if (fetchedToTeam == null)
            {
                throw new InvalidOperationException("To Team not found");
            }

            if (fetchedToTeam.Students.Count == MAX_STUDENTS)
            {
                throw new InvalidOperationException("To Team is full");
            }

            var fetchedStudent = StudentsRepo.GetById(student.Id);

            if (fetchedStudent == null)
            {
                throw new InvalidOperationException("Student not found");
            }

            if (! fetchedFromTeam.Students.Contains(fetchedStudent))
            {
                throw new InvalidOperationException("Student is not a member of From Team");
            }

            fetchedFromTeam.Students.Remove(fetchedStudent);
            fetchedToTeam.Students.Add(fetchedStudent);

            TeamsRepo.Update(fetchedFromTeam);
            TeamsRepo.Update(fetchedToTeam);
        }

        public void RemoveStudentFromTeam(Team team, Student student)
        {
            if (team == null)
            {
                throw new ArgumentException("Team is missing");
            }
            if (student == null)
            {
                throw new ArgumentException("Student is missing");
            }

            var fetchedTeam = TeamsRepo.GetById(team.Id);

            if (fetchedTeam == null)
            {
                throw new InvalidOperationException("Team not found");
            }

            var fetchedStudent = StudentsRepo.GetById(student.Id);

            if (fetchedStudent == null)
            {
                throw new InvalidOperationException("Student not found");
            }

            if (! fetchedTeam.Students.Contains(fetchedStudent))
            {
                throw new InvalidOperationException("Student is not a member of the team");
            }

            fetchedTeam.Students.Remove(fetchedStudent);
            TeamsRepo.Update(fetchedTeam);
        }

        //public IEnumerable<Student> GetNonAssignedStudents()
        //{
        //    var assignedStudents = new List<Student>();
        //    foreach (var team in TeamsRepo.GetAll())
        //    { 
        //        assignedStudents.AddRange(team.Students);
        //    }
        //    return StudentsRepo.GetAll().ToList().Except(assignedStudents);
        //}
    }
}
