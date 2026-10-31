using System;
using System.Collections.Generic;
using System.Text;

namespace LMS.Shared.DTOs.Module
{
    public record ModuleListDto
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public DateTime StartDate { get; init; }
        public DateTime EndDate { get; init; }
    }
}
