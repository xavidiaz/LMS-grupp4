using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Models.Entities
{
    public class Module
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public Course Course { get; set; } = null!;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public ICollection<Activity> Activities { get; set; } = [];

    }
}
