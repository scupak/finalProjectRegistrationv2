using Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Services
{
    public class TeamService
    {
        private ITeamRepository repo;

        public TeamService(ITeamRepository repo)
        {
            this.repo = repo ?? throw new ArgumentException("Team Repository is missing");
        }
    }
}
