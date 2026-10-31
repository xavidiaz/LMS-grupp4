using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Models.Entities
{
    public class Activity
    {
        public int Id { get; set; }
        public int? ModuleId { get; set; }  = null!;
        public Module Module { get; set; } = null!;
        public ActionType ActionType { get; set; } = null!;
        public int? ActionTypeId { get; set; }= null!;
        public  string Title { get; set; } = null!;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

    }
}
