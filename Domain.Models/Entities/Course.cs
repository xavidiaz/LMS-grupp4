using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Models.Entities
{
    public class Course
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public ICollection<ApplicationUser> ApplicationUsers { get; set; }
             = [];
        public ICollection<Module> Modules { get; set; } = [];
    }
}
