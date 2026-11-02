using LMS.Shared.DTOs.AssignmentDtos;
using LMS.Shared.DTOs.Module;

namespace LMS.Shared.DTOs.UserDtos;

// What a teacher sees about one student: course, modules and assignment status
public record StudentOverviewDto
{
    public UserDto Student { get; init; } = null!;
    public IEnumerable<ModuleDto> Modules { get; init; } = [];
    public int? CurrentModuleId { get; init; }
    public IEnumerable<AssignmentDetailsDto> Assignments { get; init; } = [];
}
