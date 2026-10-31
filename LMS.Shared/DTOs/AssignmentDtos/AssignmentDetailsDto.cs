using LMS.Shared.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace LMS.Shared.DTOs.AssignmentDtos
{
    public record AssignmentDetailsDto
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public DateTime Deadline { get; init; }
        public AssignmentStatus AssignmentStatus { get; init; }
        public bool HasFeedback { get; init; }
        public string? Comment { get; init; }
        public decimal? Grade { get; init; }
        public DateTime? CreatedAt { get; init; }
    }
}
