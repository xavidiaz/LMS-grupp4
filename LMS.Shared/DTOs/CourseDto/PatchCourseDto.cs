namespace LMS.Shared.DTOs.CourseDtos;

public sealed record PatchCourseDto(
        int Id,
        string? Name = null,
        string? Code = null,
        string? Description = null,
        DateTime? StartDate = null,
        DateTime? EndDate = null
    );
