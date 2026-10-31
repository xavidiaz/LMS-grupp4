namespace LMS.Shared.DTOs.CourseDtos;

public sealed record CourseDto(
        int Id,
        string Name,
        string Code,
        string Description,
        DateTime StartDate,
        DateTime EndDate
    );
