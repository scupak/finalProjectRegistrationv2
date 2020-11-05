using System;

namespace Model
{
    public class Project
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ContactName { get; set; }
        public string ContactEmail { get; set; }
    }
}
