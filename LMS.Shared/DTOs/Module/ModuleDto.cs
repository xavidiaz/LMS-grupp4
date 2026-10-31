using System;
using System.Collections.Generic;
using System.Text;

namespace LMS.Shared.DTOs.Module
{
    public record ModuleDto
    {
        public int Id { get; init; }
        public int CourseId { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public DateTime StartDate { get; init; }
        public DateTime EndDate { get; init; }
    }
}
