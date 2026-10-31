using Domain.Models.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Models.ReadModels
{
    public sealed class StudentAssignmentReadModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Deadline { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool HasFeedback { get; set; }
        public string? Comment { get; set; }
        public decimal? Grade { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
