using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Models.ReadModels
{
    public sealed class TeacherSubmissionReadModel
    {
        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string StudentEmail { get; set; } = string.Empty;
        public int ActivityId { get; set; }
        public string ActivityTitle { get; set; } = string.Empty;
        public DateTime Deadline { get; set; }
        public int? SubmissionId { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public string? FileName { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
