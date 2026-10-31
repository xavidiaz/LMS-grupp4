namespace LMS.Shared.DTOs.CourseDtos;

public sealed record NewCourseDto(
        string Name,
        string Code,
        string Description,
        DateTime StartDate,
        DateTime EndDate
    );
