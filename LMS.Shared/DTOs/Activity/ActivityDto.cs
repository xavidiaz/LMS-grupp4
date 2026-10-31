using System;
using System.Collections.Generic;
using System.Text;

namespace LMS.Shared.DTOs.Activity
{
    public record ActivityDto
    {
        public int Id { get; init; }
        public int ModuleId { get; init; }
        public int ActionTypeId { get; init; }
        public string? ActionTypeName { get; init; }
        public string Title { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public DateTime StartDate { get; init; }
        public DateTime EndDate { get; init; }
    }
}
