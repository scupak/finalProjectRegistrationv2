using Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Services
{
    public class StudentTeamService
    {
        private readonly IStudentRepository StudentsRepo;
        private readonly ITeamRepository TeamsRepo;

        public StudentTeamService(IStudentRepository studentRepository, ITeamRepository teamRepository)
        {
            this.StudentsRepo = studentRepository ?? throw new ArgumentException("StudentRepository is missing");
            this.TeamsRepo = teamRepository ?? throw new ArgumentException("TeamRepository is missing");
        }
    }
}
